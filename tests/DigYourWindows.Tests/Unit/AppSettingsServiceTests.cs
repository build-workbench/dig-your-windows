using System.IO;
using DigYourWindows.UI.Services;

namespace DigYourWindows.Tests.Unit;

/// <summary>
/// Unit tests for AppSettingsService persistence (save/load and corrupt fallback).
/// </summary>
public class AppSettingsServiceTests
{
    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "dyw-settings-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    [Fact]
    public void SaveThenNewInstanceLoadsPersistedValues()
    {
        var dir = CreateTempDir();
        try
        {
            var service = new AppSettingsService(dir);
            service.Save(new AppSettings { FontFamily = "SimSun", ScalePercent = 125 });

            var reloaded = new AppSettingsService(dir);

            Assert.Equal("SimSun", reloaded.Current.FontFamily);
            Assert.Equal(125, reloaded.Current.ScalePercent);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void MissingFile_FallsBackToDefaults()
    {
        var dir = CreateTempDir();
        try
        {
            var service = new AppSettingsService(dir);

            Assert.Equal("Microsoft YaHei UI", service.Current.FontFamily);
            Assert.Equal(100, service.Current.ScalePercent);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void CorruptJson_FallsBackToDefaults()
    {
        var dir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(dir, "settings.json"), "{ not valid json !!!");

            var service = new AppSettingsService(dir);

            Assert.Equal("Microsoft YaHei UI", service.Current.FontFamily);
            Assert.Equal(100, service.Current.ScalePercent);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void OutOfRangeValues_FallsBackToDefaults()
    {
        var dir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(dir, "settings.json"), """{"FontFamily":"X","ScalePercent":999}""");

            var service = new AppSettingsService(dir);

            Assert.Equal(100, service.Current.ScalePercent);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
