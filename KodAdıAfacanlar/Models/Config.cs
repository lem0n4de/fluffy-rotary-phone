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
            internal static string ConfigPath { get; } = @"config.json";
        }

        internal static Config config = ReadConfig();

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
        }
    }
}