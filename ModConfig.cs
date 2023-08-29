using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace Concursus
{
    public class ModConfig
    {
        public static string CONFIG_FILE = "config.json";
        public string name { get; set; }
        public string version { get; set; }
        public string author { get; set; }
        public string description { get; set; }

        public static ModConfig LoadFromModPath(string mod_dir)
        {
            string mod_config_path = Path.Combine(mod_dir, ModConfig.CONFIG_FILE);
            if (!File.Exists(mod_config_path))
                return null;
            string data = File.ReadAllText(mod_config_path);
            ModConfig config = JsonSerializer.Deserialize<ModConfig>(data)!;
            return config;
        }

        public bool SaveToPath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string mod_config_path = Path.Combine(path, ModConfig.CONFIG_FILE);
            File.WriteAllText(mod_config_path, JsonSerializer.Serialize(this));
            return File.Exists(mod_config_path);
        }
    }
}
