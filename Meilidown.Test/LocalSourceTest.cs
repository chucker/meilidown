using Meilidown.Models.Sources;

namespace Meilidown.Test;

[TestClass]
[TestCategory("Models")]
public class LocalSourceTest
{
    public static IEnumerable<object[]> InvalidConfigurationProvider()
    {
        yield return new object[] { new Dictionary<string, string>() };
        yield return new object[] { new Dictionary<string, string> { { "Type", "git" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Path", "path" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Root", "root" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "git" }, { "Name", "name" }, { "Root", "root" } } };
    }

    [DataTestMethod]
    [DynamicData(nameof(InvalidConfigurationProvider), DynamicDataSourceType.Method)]
    public void TestConstructorThrowsForInvalidConfig(Dictionary<string, string> config)
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            var configuration = TestHelper.GetConfiguration(config);

            return new LocalSource(configuration);
        });
    }
    
    public static IEnumerable<object[]> ValidConfigurationProvider()
    {
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" }, { "Root", "root" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "LOCAL" }, { "Name", "name" }, { "Root", "root" }, { "Path", "path" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "LocAL" }, { "Name", "name" }, { "Root", "root" } } };
    }
    
    [DataTestMethod]
    [DynamicData(nameof(ValidConfigurationProvider), DynamicDataSourceType.Method)]
    public void TestConstructorDoesNotThrowForValidConfig(Dictionary<string, string> config)
    {
        var configuration = TestHelper.GetConfiguration(config);
        var source = new LocalSource(configuration);
        Assert.AreEqual(SourceType.Local, source.Type);
        Assert.AreEqual(config["Name"], source.Name);
        Assert.AreEqual(config["Root"], source.Root);
        Assert.AreEqual(config.ContainsKey("Path") ? config["Path"] : "", source.Path);
    }

    [TestMethod]
    public void TestUpdateAsync()
    {
        var configuration = TestHelper.GetConfiguration(new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" }, { "Root", "root" } });
        var source = new LocalSource(configuration);
        var task = source.UpdateAsync();
        Assert.IsTrue(task.IsCompleted);
    }

    public static IEnumerable<object[]> ToStringValuesProvider()
    {
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" }, { "Root", "root" } }, "Local Source: {Name: name, Root: root, Path: }" };
    }
    
    [DataTestMethod]
    [DynamicData(nameof(ToStringValuesProvider), DynamicDataSourceType.Method)]
    public void TestToString(Dictionary<string, string> config, string expected)
    {
        var configuration = TestHelper.GetConfiguration(config);
        var source = new LocalSource(configuration);
        Assert.AreEqual(expected, source.ToString());
    }
}