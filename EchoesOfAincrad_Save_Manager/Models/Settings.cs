using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text;

namespace EchoesOfAincrad_Save_Manager.Models
{
    public class Settings
    {
        public static readonly string DefaultGameSavesPath = Path.Combine(Environment.ExpandEnvironmentVariables("%LocalAppData%"), "EchoesofAincrad", "Saved", "SaveGames");
        public const string DEFAULT_DATA_DIR = "SaveGames";

        public const int DEFAULT_MAX_SAVES = 100;
        public const int DEFAULT_BACKUP_INTERVAL = 300;

        public int MaxSaves { get; set; }
        public int BackupInterval { get; set; }
        public string GameSavesPath { get; set; }
        public string DataDir { get; set; }

        public Settings()
        {
            MaxSaves = DEFAULT_MAX_SAVES;
            BackupInterval = DEFAULT_BACKUP_INTERVAL;
            GameSavesPath = DefaultGameSavesPath;
            DataDir = DEFAULT_DATA_DIR;
        }

        public static Settings FromFile(string filename)
        {
            if (!File.Exists(filename))
            {
                Settings settings = new();
                File.WriteAllText(filename, JsonConvert.SerializeObject(settings, Formatting.Indented));

                return settings;
            }
            else
            {
                var text = File.ReadAllText(filename);

                try
                {
                    var settings = JsonConvert.DeserializeObject<Settings>(text);

                    if (settings is not null)
                        return settings;
                    else
                        return new();
                }
                catch (Exception ex)
                {
                    return new();
                }
            }
        }
    }
}
