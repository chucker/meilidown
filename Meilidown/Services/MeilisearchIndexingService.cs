﻿using Meilidown.Interfaces;
using Meilidown.Models.Index;
using Meilisearch;
using Index = Meilisearch.Index;

namespace Meilidown.Services;

public class MeilisearchIndexingService : IIndexingService
{
    private const string IndexName = "meilidown";

    private readonly MeilisearchClient _client;
    private readonly ILogger<MeilisearchIndexingService> _logger;

    public MeilisearchIndexingService(MeilisearchClient client, ILogger<MeilisearchIndexingService> logger)
    {
        _client = client;
        _logger = logger;
    }

    private Index GetIndex()
    {
        return _client.Index(IndexName);
    }

    /// <inheritdoc />
    public async Task IndexAsync(IEnumerable<IndexFile> files, CancellationToken cancellationToken = default)
    {
        var health = await _client.HealthAsync(cancellationToken);
        _logger.LogInformation("Meilisearch is {Status}", health.Status);

        var settings = new Settings
        {
            FilterableAttributes = new[] { "uid", "name", "location", "content" },
            SortableAttributes = new[] { "name", "order", "location" },
            SearchableAttributes = new[] { "name", "location", "content" },
        };

        var filesIndex = GetIndex();
        foreach (var task in new Dictionary<string, Task<TaskInfo>>
                 {
                     { "Delete previous index", filesIndex.DeleteAllDocumentsAsync(cancellationToken) },
                     { "Add new index", filesIndex.AddDocumentsAsync(files, cancellationToken: cancellationToken) },
                     { "Update index settings", filesIndex.UpdateSettingsAsync(settings, cancellationToken) },
                 })
        {
            var info = await task.Value;
            var result = await _client.WaitForTaskAsync(info.Uid, cancellationToken: cancellationToken);
            _logger.LogInformation("Task '{Name}': {Status}", task.Key, result.Status);

            if (result.Error == null) continue;

            _logger.LogError("{@Error}", string.Join(Environment.NewLine, result.Error.Values));
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IndexFile>> GetIndexedFiles(CancellationToken cancellationToken = default)
    {
        return await GetIndex().GetDocumentsAsync<IndexFile>(new()
        {
            Limit = 10000,
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IndexFile?> GetIndexedFile(string location, CancellationToken cancellationToken = default)
    {
        var result = await GetIndex().SearchAsync<IndexFile>("", new()
        {
            Filter = $"location = \"{location}\"",
            Limit = 1,
        }, cancellationToken);

        return result.Hits.FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SearchResult>> Search(string query, CancellationToken cancellationToken = default)
    {
        var result = await GetIndex().SearchAsync<SearchResult>(query, new()
        {
            AttributesToHighlight = new[] { "name", "location", "content" },
            HighlightPreTag = "<mark>",
            HighlightPostTag = "</mark>",
            AttributesToCrop = new[] { "content" },
            CropLength = 25,
            AttributesToRetrieve = new[] { "name", "location", "content" },
        }, cancellationToken);

        return result.Hits;
    }
}