using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Concursus.Classes
{
	public class GamebananaMod
	{
		const string IMAGE_ENDPOINT = "https://images.gamebanana.com/img/ss/mods/";
		const string MOD_INFO_ENDPOINT = @"https://api.gamebanana.com/Core/Item/Data?itemtype=Mod&itemid=<MOD_ID>&fields=Game%28%29.name%2Cname%2COwner%28%29.name%2Ctext%2CFiles%28%29.aFiles%28%29%2Cscreenshots%2CUpdates%28%29.aGetLatestUpdates%28%29";
		public struct Files
		{
			public string filename { get; set; }
			public string download_link { get; set; }
			public string description { get; set; }
			public string md5 { get; set; }
			public int filesize { get; set; }
		}
		public string mod_id;
		public string name { get; set; }
		public string submitter { get; set; }
		public Dictionary<string, Files> files { get; set; }
		public string description { get; set; }
		public string version { get; set; }
		public List<string> images { get; set; }
		public string mod_dir_path { get; set; }
		public string game_data_folder_name { get; set; }
		public string game_name { get; set; }
		GamebananaMod()
		{

		}

		public static GamebananaMod ParseFromJson(string text)
		{
			GamebananaMod mod = new GamebananaMod();
			Console.WriteLine(text);
			JsonArray? jsonObj = JsonSerializer.Deserialize<JsonArray>(text);
			if (jsonObj == null)
				return null;
			mod.name = (string)jsonObj[1].AsValue();
			mod.submitter = (string)jsonObj[2].AsValue();
			mod.description = (string)jsonObj[3].AsValue();

			mod.files = new Dictionary<string, Files>();

			foreach (var item in (JsonObject)jsonObj[4].AsObject())
				mod.files.Add(item.Key, new Files
				{
					filename = (string)item.Value["_sFile"].AsValue(),
					download_link = (string)item.Value["_sDownloadUrl"].AsValue(),
					description = (string)item.Value["_sDescription"].AsValue(),
					md5 = (string)item.Value["_sMd5Checksum"].AsValue(),
					filesize = (int)item.Value["_nFilesize"].AsValue(),
				});

			mod.images = new List<string>();

			JsonArray? images = JsonSerializer.Deserialize<JsonArray>((string)jsonObj[5].AsValue());
			if (images != null)
				for (int i = 0; i < images.Count; i++)
					mod.images.Add(String.Concat(IMAGE_ENDPOINT, (string)((JsonObject)images[i])["_sFile"].AsValue()));

			if (((JsonArray)jsonObj[6]).Count >= 1)
			{
				try
				{
					mod.version = (string)((JsonObject)((JsonArray)jsonObj[6])[0])["_sVersion"].AsValue();
				}
				catch (Exception)
				{
					mod.version = "???";
				}
			}
			else
				mod.version = "???";

			string game_name = (string)jsonObj[0].AsValue();
			mod.game_name = game_name;
			switch (game_name)
			{
				case "Etrian Odyssey HD":
					mod.mod_dir_path = Path.Combine(Properties.Settings.Default.EO1_Path, "mods");
					mod.game_data_folder_name = "Etrian Odyssey_Data";
					break;
				case "Etrian Odyssey II HD":
					mod.mod_dir_path = Path.Combine(Properties.Settings.Default.EO2_Path, "mods");
					mod.game_data_folder_name = "Etrian Odyssey 2_Data";
					break;
				case "Etrian Odyssey III HD":
					mod.mod_dir_path = Path.Combine(Properties.Settings.Default.EO3_Path, "mods");
					mod.game_data_folder_name = "Etrian Odyssey 3_Data";
					break;
				default:
					return null;
			}

			return mod;
		}

		public string GetValidFolderName()
		{
			string cleaned = String.Join("_", this.name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
			if (cleaned == String.Empty)
				cleaned = this.mod_id;
			return cleaned;
		}

		public ModConfig GetModConfig()
		{
			return new ModConfig()
			{
				name = this.name,
				version = this.version,
				description = this.description,
				author = this.submitter
			};
		}

		public static GamebananaMod GetModInfoFromID(string mod_id)
		{
			string url = MOD_INFO_ENDPOINT.Replace("<MOD_ID>", mod_id);
			Console.WriteLine(mod_id);
			string json = Utils.GetTextFromURL(url);
			if (json == null)
				return null;
			GamebananaMod mod = GamebananaMod.ParseFromJson(json);
			mod.mod_id = mod_id;
			return mod;
		}
	}
}
