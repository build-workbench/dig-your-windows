using System.IO;
using DigYourWindows.UI.Services;

namespace DigYourWindows.Tests.Unit;

/// <summary>
/// Unit tests for AppSettingsService persistence (save/load, corrupt fallback
/// and legacy single-font migration).
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
            service.Save(new AppSettings
            {
                EnglishFontFamily = "Consolas",
                ChineseFontFamily = "SimSun",
                ScalePercent = 125
            });

            var reloaded = new AppSettingsService(dir);

            Assert.Equal("Consolas", reloaded.Current.EnglishFontFamily);
            Assert.Equal("SimSun", reloaded.Current.ChineseFontFamily);
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

            Assert.Equal(string.Empty, service.Current.EnglishFontFamily);
            Assert.Equal(string.Empty, service.Current.ChineseFontFamily);
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

            Assert.Equal(string.Empty, service.Current.EnglishFontFamily);
            Assert.Equal(string.Empty, service.Current.ChineseFontFamily);
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
            File.WriteAllText(Path.Combine(dir, "settings.json"),
                """{"EnglishFontFamily":"Arial","ChineseFontFamily":"SimSun","ScalePercent":999}""");

            var service = new AppSettingsService(dir);

            Assert.Equal(100, service.Current.ScalePercent);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void LegacyFontFamily_MigratesToChineseFont()
    {
        var dir = CreateTempDir();
        try
        {
            // Old settings stored a single font in FontFamily
            File.WriteAllText(Path.Combine(dir, "settings.json"),
                """{"FontFamily":"SimSun","ScalePercent":110}""");

            var service = new AppSettingsService(dir);

            Assert.Equal("SimSun", service.Current.ChineseFontFamily);
            Assert.Equal(string.Empty, service.Current.EnglishFontFamily);
            Assert.Equal(string.Empty, service.Current.FontFamily);
            Assert.Equal(110, service.Current.ScalePercent);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void LegacySystemFontFamily_MigratesToEmptyFollowSystem()
    {
        var dir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(dir, "settings.json"),
                $$"""{"FontFamily":"{{AppSettings.SystemFontFamily}}","ScalePercent":100}""");

            var service = new AppSettingsService(dir);

            Assert.Equal(string.Empty, service.Current.ChineseFontFamily);
            Assert.Equal(string.Empty, service.Current.FontFamily);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void MigratedSettings_PersistWithoutReMigration()
    {
        var dir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(dir, "settings.json"),
                """{"FontFamily":"SimSun","ScalePercent":110}""");

            var first = new AppSettingsService(dir);
            first.Save(first.Current);

            var second = new AppSettingsService(dir);

            Assert.Equal("SimSun", second.Current.ChineseFontFamily);
            Assert.Equal(string.Empty, second.Current.EnglishFontFamily);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
