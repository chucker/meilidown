﻿using Meilidown.Models.Index;
using Meilidown.Models.Sources;
using Meilidown.Services;

namespace Meilidown.Test;

[TestClass]
[TestCategory("Services")]
public class MarkdownFileProcessingServiceTest
{
    public static IEnumerable<object[]> ProcessFileDataProvider()
    {
        var testConfig = new LocalSource(TestHelper.GetConfiguration(new Dictionary<string, string>
        {
            { "Name", "Test" },
            { "Type", "Local" },
            { "Root", "./Data" },
        }));
        
        yield return new object[]
        {
            new[]
            {
                new SourceFile(testConfig, "Test.md"),
                new SourceFile(testConfig, "Image.md"),
            },
            new[]
            {
                new IndexFile(
                    "E36B91F69E929E7E4CE4DE4C6C8A1919",
                    "Test",
                    "# Test",
                    0,
                    "Test"
                ),
                new IndexFile(
                    "AC3AEF213ACC355D71D9E3A708283052",
                    "Image",
                    "![Image](%API_HOST%/File/test.png)\n![Image](%API_HOST%/File/..%2frelative.png)\n![Image](%API_HOST%/File/images/public/nested.png)",
                    0,
                    "Image"
                ),
            },
        };
    }

    [DataTestMethod]
    [DynamicData(nameof(ProcessFileDataProvider), DynamicDataSourceType.Method)]
    public async Task TestProcessAsync(IEnumerable<SourceFile> sourceFiles, IEnumerable<IndexFile> expectedFiles)
    {
        var logger = TestHelper.GetLogger<MarkdownFileProcessingService>();
        var service = new MarkdownFileProcessingService(logger);
        var indexFiles = await service.ProcessAsync(sourceFiles.ToAsyncEnumerable()).ToListAsync();

        foreach (var (expected, output) in expectedFiles.Zip(indexFiles))
        {
            Assert.AreEqual(expected.uid, output.uid);
            Assert.AreEqual(expected.name, output.name);
            Assert.AreEqual(expected.content, output.content);
            Assert.AreEqual(expected.order, output.order);
            Assert.AreEqual(expected.location, output.location);
        }
    }
}