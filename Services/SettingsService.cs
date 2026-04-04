using System;
using System.IO;
using System.Text.Json;

namespace QobuzRPC.Services;

public class AppSettings
{
    public string DiscordClientId { get; set; } = "1490007914461790399";
    public string LastFmApiKey { get; set; } = string.Empty;
    public double WindowWidth { get; set; } = 520;
    public double WindowHeight { get; set; } = 650;
    public double WindowX { get; set; } = double.NaN;
    public double WindowY { get; set; } = double.NaN;
}

public class SettingsService
{
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "QzRPC"
    );
    
    private static readonly string SettingsFile = Path.Combine(SettingsFolder, "settings.json");

    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            // something went wrong, just use defaults
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }

        return new AppSettings();
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            // make sure the folder exists
            Directory.CreateDirectory(SettingsFolder);
            
            var options = new JsonSerializerOptions
            {
                WriteIndented = true  // makes it readable
            };
            
            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(SettingsFile, json);
        }
        catch (Exception ex)
        {
            // oh well
            Console.WriteLine($"Error saving settings: {ex.Message}");
        }
    }
}
