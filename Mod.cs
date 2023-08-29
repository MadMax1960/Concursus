using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Concursus
{
    public class Mod
    {
        public int mod_id { get; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public bool enabled { get; set; }
        public string mod_path { get; set; }
        public string folder_name { get; set; }

        public Mod()
        {
            this.mod_id = Mod.GenerateID();
        }

        private static List<int> generated_ids = new List<int>();
        private static int GenerateID()
        {
            Random rnd = new Random();
            int mod_id = rnd.Next(0, int.MaxValue);
            while (Mod.generated_ids.Contains(mod_id))
                mod_id = rnd.Next(0, int.MaxValue);
            Mod.generated_ids.Add(mod_id);
            return mod_id;
        }
    }
}
