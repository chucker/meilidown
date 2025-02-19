﻿using Meilidown.Interfaces;
using Meilidown.Models.Sources;

namespace Meilidown.Services;

public class ConfigurationSourcesProvider : ISourcesProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    
    public ConfigurationSourcesProvider(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public IEnumerable<ISource> GetSources()
    {
        return _configuration
            .GetSection("Sources")
            .GetChildren()
            .Where(s => s["Type"] != null)
            .Select<IConfigurationSection, ISource>(s => s["Type"] switch
            {
                "git" => new GitSource(s, _loggerFactory),
                "local" => new LocalSource(s),
                _ => throw new NotImplementedException($"The source type '{s["Type"]}' is not implemented. Must be one of: 'git', 'local'"),
            });
    }
}