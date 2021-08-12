using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace KodAdıAfacanlar.Models
{
    public static class ConfigManager
    {
        public class Config
        {
            public string DownloadDirectory { get; set; } =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "TUS");

            public string LastKnownSessionId { get; set; } = "";

            internal static string ConfigFolder { get; } = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Kod Adı Afacanlar");

            internal static string ConfigPath { get; } = Path.Combine(ConfigFolder, @"config.json");
            internal static string LessonDbPath { get; } = Path.Combine(ConfigFolder, @"lessons.json");
        }

        internal static Config config { get; set; }

        internal static void OnStart()
        {
            Directory.CreateDirectory(Config.ConfigFolder);
            if (!File.Exists(Config.ConfigPath))
            {
                File.Create(Config.ConfigPath).Close();
            }

            if (!File.Exists(Config.LessonDbPath))
            {
                File.Copy(@"lessons.json", Config.LessonDbPath);
            }

            config = ReadConfig();
        }

        internal static string GetLessonDbPath()
        {
            if (File.Exists(Config.LessonDbPath))
            {
                return Config.LessonDbPath;
            }

            if (File.Exists(@"lessons.json"))
            {
                File.Copy(@"lessons.json", Config.LessonDbPath);
                return Config.LessonDbPath;
            }

            throw new FileNotFoundException("lessons.json not found.");
        }

        internal static void SaveConfig()
        {
            var str = JsonSerializer.Serialize(config);
            File.WriteAllText(Config.ConfigPath, str);
        }

        internal static Config ReadConfig()
        {
            try
            {
                var str = File.ReadAllText(Config.ConfigPath);
                var config = JsonSerializer.Deserialize<Config>(str);
                if (config == null) throw new FileNotFoundException();
                return config;
            }
            catch (FileNotFoundException e)
            {
                config = new Config();
                SaveConfig();
                return config;
            }
            catch (JsonException e)
            {
                config = new Config();
                SaveConfig();
                return config;
            }
        }
    }
}