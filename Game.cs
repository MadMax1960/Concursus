using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Force.Crc32;
using System.Text.Json;
using System.Windows;
using SevenZipExtractor;
using System.Threading;

namespace Concursus
{
    public class Game
    {
        public enum GameType
        {
            NoDRM,
            DRM,
            Invalid
        }

        private static string BACKUP_FOLDER = "backups";
        private static string MODS_FOLDER = "mods";
        private static List<string> PATHS_TO_IGNORE = new List<string>() { ModConfig.CONFIG_FILE, Game.MODS_FOLDER, Game.BACKUP_FOLDER, "catalog.json" };
        public string key { get; set; }
        public string GameName { get; set; }
        public string GamePath { get; set; }
        public string GameExecutable { get; set; }
        public string GameFolderDataName { get; set; }
        public int GameID { get; set; } // Add the GameID property
        public int GBRSS { get; set; } // Change to int
        public string GBONECLICK { get; set; } // Change to string
        public GameType Type { get; set; }
        public ObservableCollection<Mod> GameMods { get; set; }
        public IProgress<string> textProgress { get; set; }
        public static ObservableCollection<Mod> GetModsFromPath(string path)
        {
            try
            {
                ObservableCollection<Mod> mods = new ObservableCollection<Mod>();
                string mods_path = Path.Combine(path, MODS_FOLDER);
                if (!Directory.Exists(mods_path))
                    Directory.CreateDirectory(mods_path);
                foreach (var dir in Directory.EnumerateDirectories(mods_path))
                {
                    ModConfig mod_config = ModConfig.LoadFromModPath(dir);
                    if (mod_config == null)
                        continue;
                    mods.Add(new Mod()
                    {
                        Name = mod_config.name,
                        Author = mod_config.author,
                        Version = mod_config.version,
                        Description = mod_config.description,
                        mod_path = dir,
                        folder_name = Path.GetFileName(dir)
                    });
                }
                return mods;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Ran into an exception when getting mods from {path}!\n\n{e.Message}");
                return null;
            }
        }

        public void LoadDataFromKey()
        {
            List<string> mod_folders = new List<string>();
            List<bool> mod_statuses = new List<bool>();
            if (!File.Exists(this.key))
                return;
            string[] data = File.ReadAllText(this.key).Split('\n');
            foreach (string elem in data)
            {
                string[] split = elem.Split('|');
                string folder_name = String.Join('|', split.Take(split.Length - 1));
                string status = split[split.Length - 1];
                mod_folders.Add(folder_name);
                mod_statuses.Add((status == "True"));
            }
            this.GameMods = new ObservableCollection<Mod>(this.GameMods.OrderBy(e =>
            {
                int idx = mod_folders.IndexOf(e.folder_name);
                if (idx == -1)
                    return this.GameMods.Count;
                return idx;
            }));
            foreach (Mod mod in this.GameMods)
            {
                int idx = mod_folders.IndexOf(mod.folder_name);
                if (idx == -1)
                    continue;
                mod.enabled = mod_statuses[idx];
            }
        }

        public void Save()
        {
            this.textProgress.Report("Saving mods statuses...");
            string output = "";
            foreach (Mod mod in this.GameMods)
                output += $"{mod.folder_name}|{mod.enabled}\n";
            this.textProgress.Report("Writing mods statuses to key...");
            File.WriteAllText(this.key, output);
            this.textProgress.Report("Starting mod installation");
            Install();
        }

        private void PatchGame()
        {
            string target_folder = Path.Join(this.GamePath, this.GameFolderDataName, "StreamingAssets", "aa");
            string catalog_json = Path.Join(target_folder, "catalog.json");

            if (File.Exists(catalog_json))
            {
                Patch.PatchCRC(catalog_json, catalog_json);
            }
            else
            {
                //MessageBox.Show($"The file {catalog_json} does not exist. Patching operation cannot be performed.");
            }
        }


        private void RestoreBackup()
        {
            string backup_dir = Path.Combine(this.GamePath, Game.BACKUP_FOLDER);
            if (!Directory.Exists(backup_dir))
                return;
            CloneDirectory(backup_dir, this.GamePath, this.textProgress);
        }

        private static void CloneDirectory(string root, string dest, IProgress<string> textProgress)
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                if (Game.PATHS_TO_IGNORE.Contains(Path.GetFileName(directory)))
                    continue;
                //Get the path of the new directory
                var newDirectory = Path.Combine(dest, Path.GetFileName(directory));
                //Create the directory if it doesn't already exist
                Directory.CreateDirectory(newDirectory);
                //Recursively clone the directory
                CloneDirectory(directory, newDirectory, textProgress);
            }

            foreach (var file in Directory.GetFiles(root))
            {
                if (Game.PATHS_TO_IGNORE.Contains(Path.GetFileName(file)))
                    continue;
                textProgress.Report($"Restoring {file}");
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
            }
        }

        private void CheckDirForMods(string inital_mod_path, string path, ref Dictionary<string, List<string>> keyValuePairs)
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                if (Game.PATHS_TO_IGNORE.Contains(Path.GetFileName(directory)))
                    continue;
                CheckDirForMods(inital_mod_path, directory, ref keyValuePairs);
            }

            foreach (var file in Directory.GetFiles(path))
            {
                if (Game.PATHS_TO_IGNORE.Contains(Path.GetFileName(file)))
                    continue;
                string key = GetSourceFileFromDataPath(file.Substring(inital_mod_path.Length + 1));
                if (!keyValuePairs.ContainsKey(key))
                    keyValuePairs.Add(key, new List<string>());
                keyValuePairs[key].Add(file);
            }
        }

        private void CopyOriginalToBackup(Dictionary<string, List<string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, List<string>> entry in keyValuePairs)
            {
                string original_file = Path.Join(this.GamePath, entry.Key);
                string backup_file = Path.Join(this.GamePath, Game.BACKUP_FOLDER, entry.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(backup_file));
                try
                {
                    File.Copy(original_file, backup_file, true);
                }
                catch (Exception e)
                {
                    // Do nothing (in case people start adding their own bundle files and stuff)
                }
            }
        }

        public static void CreateNewCombinedAssetsFile(string original, List<string> modded_paths, string output, IProgress<string> textProgress = null)
        {
            if (textProgress == null)
                textProgress = new Progress<string>();
            if (modded_paths.Count == 1)
            {
                textProgress.Report($"Only one modded assets instance found! Overwriting...");
                File.Copy(modded_paths[0], output, true);
                return;
            }
            var am = new AssetsManager();

            textProgress.Report($"Loading original assets file... {original}");
            var og_assetInst = am.LoadAssetsFile(original, false);

            am.LoadClassPackage("classdata_large.tpk");
            am.LoadClassDatabaseFromPackage(og_assetInst.file.Metadata.UnityVersion);

            // Original Asset Index -> CRC32 Hash
            Dictionary<long, uint> og_asset_hash = new Dictionary<long, uint>();

            foreach (var entry in og_assetInst.file.AssetInfos)
            {
                // Ignore AssetBundle files
                if (entry.TypeId == (uint)AssetClassID.AssetBundle)
                    continue;
                var basefield = am.GetBaseField(og_assetInst, entry);
                og_asset_hash.Add((int)entry.ScriptTypeIndex, Crc32Algorithm.Compute(basefield.WriteToByteArray()));
            }


            foreach (string file in modded_paths)
            {
                textProgress.Report($"Loading modded assets file... {file}");

                // Modded Assets
                var mod_assetInst = am.LoadAssetsFile(file, false);

                foreach (var entry in mod_assetInst.file.AssetInfos)
                {
                    // Ignore AssetBundle files
                    if (entry.TypeId == (uint)AssetClassID.AssetBundle)
                        continue;

                    // Get the Base Field of the Asset
                    var baseField = am.GetBaseField(mod_assetInst, entry);

                    // Check to see if the Asset is modded by comparing its hash to the original hash. If its the same (not modified), then skip.
                    if (og_asset_hash[(int)entry.ScriptTypeIndex] == Crc32Algorithm.Compute(baseField.WriteToByteArray()))
                        continue;

                    textProgress.Report($"Asset Index {(int)entry.ScriptTypeIndex} Data Hash is different! Overwriting asset...");

                    // Convert the modded asset to a byte array
                    var newBytes = baseField.WriteToByteArray();

                    // Overwrite the original asset data
                    og_assetInst.file.AssetInfos[(int)entry.ScriptTypeIndex].SetNewData(newBytes);
                }

                textProgress.Report("Writing new assets table...");
                // Create empty byte array for new asset data
                byte[] newAssetData;
                using (var stream = new MemoryStream())
                using (var writer = new AssetsFileWriter(stream))
                {
                    // Write the new assets in and convert it to a byte array
                    og_assetInst.file.Write(writer);
                    newAssetData = stream.ToArray();
                }
                am.UnloadAll();

                // Write the new bundle to the output
                File.WriteAllBytes(output, newAssetData);
                textProgress.Report($"Assets file {output} is done writing!");
            }
        }

		public static void CreateNewCombinedBundle(string original, List<string> modded_paths, string output, IProgress<string> textProgress = null)
		{
			if (textProgress == null)
				textProgress = new Progress<string>();

			// Inform the user that merging is not implemented
			textProgress.Report("Merging for bundles is not implemented. Copying the highest priority mod file instead...");

			// Copy the highest priority mod file (assumed to be the first in the reversed list)
			File.Copy(modded_paths[0], output, true);

			textProgress.Report($"File copied from {modded_paths[0]} to {output}");
		}

		//if (textProgress == null)
		//	textProgress = new Progress<string>();

		//	if (original == null || modded_paths == null || output == null)
		//{
		//		textProgress.Report("Error: Invalid input paths.");
		//		return;
		//}

		//var am = new AssetsManager();

		//	var og_bundle = am.LoadBundleFile(original);
		//	var og_assetInst = am.LoadAssetsFileFromBundle(og_bundle, 0, false);

		//	Dictionary<long, uint> og_asset_hash = new Dictionary<long, uint>();

		//	foreach (var inf in og_assetInst.file.AssetInfos)
		//	{
		//	if (inf.TypeId == (uint)AssetClassID.AssetBundle)
		//	continue;

		//var baseField = am.GetBaseField(og_assetInst, inf);
		//og_asset_hash[(int)inf.ScriptTypeIndex] = Crc32Algorithm.Compute(baseField.WriteToByteArray());
		//}

		//foreach (string mod_file in modded_paths)
		//{
		//var mod_bundle = am.LoadBundleFile(mod_file);
		//var mod_assetInst = am.LoadAssetsFileFromBundle(mod_bundle, 0, false);

		//foreach (var modInf in mod_assetInst.file.AssetInfos)
		//{
		//if ((int)modInf.ScriptTypeIndex == (uint)AssetClassID.AssetBundle)
		//	continue;

		//var modBaseField = am.GetBaseField(mod_assetInst, modInf);
		//uint moddedHash = Crc32Algorithm.Compute(modBaseField.WriteToByteArray());

		//if (!og_asset_hash.ContainsKey((int)modInf.ScriptTypeIndex) ||
		//	og_asset_hash[(int)modInf.ScriptTypeIndex] != moddedHash)
		//{
		//textProgress.Report($"Overwriting asset with index {(int)modInf.ScriptTypeIndex}");
		//var newBytes = modBaseField.WriteToByteArray();
		//og_assetInst.file.AssetInfos[(int)modInf.ScriptTypeIndex].SetNewData(newBytes);
		//}
		//}
		//}

		//byte[] newAssetData;
		//using (var stream = new MemoryStream())
		//using (var writer = new AssetsFileWriter(stream))
		//{
		//og_assetInst.file.Write(writer);
		//newAssetData = stream.ToArray();
		//}

		// Unload all assets and bundles before saving the new bundle
		//am.UnloadAll();

		//textProgress.Report("Writing merged bundle to output file...");
		//File.WriteAllBytes(output, newAssetData);

		//textProgress.Report("Merged bundle creation complete.");
		//}


		public void CreateNewMod(string folder_name, ModConfig config)
        {
            // Combine the game path, "Mods" folder, and the new mod folder name
            string target_dir = Path.Combine(this.GamePath, Game.MODS_FOLDER, folder_name);

            // Check if the mod folder doesn't already exist
            if (!Directory.Exists(target_dir))
                Directory.CreateDirectory(target_dir); // Create the mod folder

            // Get the GameFolderDataName from the selected game
            string gameFolderDataName = this.GameFolderDataName; // Replace with the actual property or variable name

            // Create a new folder inside the mod folder based on GameFolderDataName
            string subfolder_path = Path.Combine(target_dir, gameFolderDataName);
            Directory.CreateDirectory(subfolder_path); // Create the subfolder

            // Create a "plugins" folder inside the mod folder
            string pluginsFolderPath = Path.Combine(target_dir, "plugins");
            Directory.CreateDirectory(pluginsFolderPath); // Create the plugins folder

            // Create a folder structure inside the GameFolderDataName folder
            string streamingAssetsPath = Path.Combine(subfolder_path, "StreamingAssets");
            string aaPath = Path.Combine(streamingAssetsPath, "aa");
            string standaloneWindows64Path = Path.Combine(aaPath, "StandaloneWindows64");

            // Create the folder structure
            Directory.CreateDirectory(streamingAssetsPath);
            Directory.CreateDirectory(aaPath);
            Directory.CreateDirectory(standaloneWindows64Path);

            // Write the mod configuration to a file within the mod folder
            File.WriteAllText(Path.Combine(target_dir, ModConfig.CONFIG_FILE), JsonSerializer.Serialize(config));

            // Refresh the game data (presumably to update the list of mods)
            this.Refresh();
        }

        public void Refresh()
        {
            ObservableCollection<Mod> new_mods = Game.GetModsFromPath(this.GamePath);
            List<string> old_mods_name = this.GameMods.Select(e => e.folder_name).ToList();
            List<string> new_mods_name = new_mods.Select(e => e.folder_name).ToList();
            List<string> new_mods_only = new_mods_name.Except(old_mods_name).ToList();
            List<string> mods_to_remove = old_mods_name.Where(e => !new_mods_name.Contains(e)).ToList();

            this.GameMods = new ObservableCollection<Mod>(this.GameMods.Where(e => !mods_to_remove.Contains(e.folder_name)).ToList());

            Mod[] new_mods_to_append = new_mods.Where(e => new_mods_only.Contains(e.folder_name)).ToArray();
            foreach (Mod mod in new_mods_to_append)
                this.GameMods.Add(mod);
        }

        public string GetSourceFileFromDataPath(string data_path)
        {
            int idx;
            switch (this.Type)
            {
                case GameType.DRM:
                    idx = Tables.NO_DRM_FILE_TABLE.IndexOf(data_path);
                    if (idx != -1)
                        return Tables.DRM_FILE_TABLE[idx];
                    else
                        return data_path;
                case GameType.NoDRM:
                    idx = Tables.DRM_FILE_TABLE.IndexOf(data_path);
                    if (idx != -1)
                        return Tables.NO_DRM_FILE_TABLE[idx];
                    else
                        return data_path;
                default:
                    return data_path;
            }
        }

        public void Install()
        {
            // Display a popup box with the game ID
            //MessageBox.Show($"Installing game with ID: {GameID}", "Installation Started", MessageBoxButton.OK, MessageBoxImage.Information);

            this.textProgress.Report("Copying plugins from mods...");
            CopyPluginsFromMods();

            this.textProgress.Report("Patching catalog.json...");
            PatchGame();
            this.textProgress.Report("Restoring backups for original files...");
            RestoreBackup(); // Restore backup everytime mods get installed
            Dictionary<string, List<string>> keyValuePairs = new Dictionary<string, List<string>>();
            this.textProgress.Report("Scanning mods for files...");
            foreach (Mod mod in this.GameMods)
            {
                if (!mod.enabled)
                    continue;
                this.textProgress.Report($"Scanning {mod.Name}...");
                string starting_mod_path = Path.Combine(mod.mod_path, this.GameFolderDataName);
                CheckDirForMods(mod.mod_path, starting_mod_path, ref keyValuePairs);
            }
            CopyOriginalToBackup(keyValuePairs);
            foreach (KeyValuePair<string, List<string>> entry in keyValuePairs)
            {
                var source_file = GetSourceFileFromDataPath(entry.Key);
                var source_path = Path.Combine(this.GamePath, source_file);
                // If its not a bundle file, then we can't merge it. Meaning we just copy the highest priority mod :Adachifalse:

                var ext = Path.GetExtension(source_file);

                switch (ext)
                {
                    case ".bundle":
                    case ".assets":
                    case "":
                        this.textProgress.Report($"Found {(ext == "" ? "blank extension (asset)" : ext)} file! Merging all {(ext == "" ? "blank extension (asset)" : ext)} files into one...");
                        // Reversing so that higher priority mods will be processed last (and thus overwriting previous mods)
                        entry.Value.Reverse();
                        if (ext == ".bundle")
                            CreateNewCombinedBundle(source_path, entry.Value, source_path, this.textProgress);
                        else
                            CreateNewCombinedAssetsFile(source_path, entry.Value, source_path, this.textProgress);
                        break;
                    default:
                        this.textProgress.Report("Not a file with merge support, so just copying the highest priority mod file over");
                        this.textProgress.Report($"Copying {entry.Value[0]} to {source_path}");
                        File.Copy(entry.Value[0], source_path, true);
                        break;
                }
            }
            this.textProgress.Report("Mods installation complete!");
        }

        public void EditModConfig(Mod mod, ModConfig newConfig)
        {
            // Update the mod's configuration
            mod.Name = newConfig.name;
            mod.Author = newConfig.author;
            mod.Version = newConfig.version;
            mod.Description = newConfig.description;

            // Find path
            string modFolderPath = Path.Combine(mod.mod_path, Game.MODS_FOLDER);
            string configFilePath = Path.Combine(modFolderPath, ModConfig.CONFIG_FILE);

            // Load the current mod configuration
            ModConfig existingConfig = ModConfig.LoadFromModPath(modFolderPath) ?? new ModConfig();

            // Update mod config
            existingConfig.name = newConfig.name;
            existingConfig.author = newConfig.author;
            existingConfig.version = newConfig.version;
            existingConfig.description = newConfig.description;

            // Serialize the updated configuration
            string serializedConfig = JsonSerializer.Serialize(existingConfig);


            string parentDirectory = Directory.GetParent(modFolderPath).FullName;


            Directory.CreateDirectory(parentDirectory);

            // Save the updated configuration to the parent directory because im too lazy to fix it saving in new thing
            File.WriteAllText(Path.Combine(parentDirectory, ModConfig.CONFIG_FILE), serializedConfig);
        }
        private void CopyPluginsFromMods()
        {
            // Get a list of all files in the game path
            List<string> existingFiles = Directory.GetFiles(this.GamePath, "*", SearchOption.AllDirectories)
                                                  .Select(file => Path.GetRelativePath(this.GamePath, file))
                                                  .ToList();

            // First iteration: Copy plugins from enabled mods
            foreach (Mod mod in this.GameMods.Where(mod => mod.enabled))
            {
                string modPluginsFolder = Path.Combine(mod.mod_path, "plugins");

                // Check if the mod has a "plugins" folder
                if (Directory.Exists(modPluginsFolder))
                {
                    this.textProgress.Report($"Copying plugins from {mod.Name}...");

                    // Copy the contents of the mod's "plugins" folder to the root of the game path
                    CopyDirectoryContents(modPluginsFolder, this.GamePath);
                }
            }

            // Second iteration: Delete files associated with disabled mods
            foreach (Mod mod in this.GameMods.Where(mod => !mod.enabled))
            {
                string modPluginsFolder = Path.Combine(mod.mod_path, "plugins");

                // Check if the mod has a "plugins" folder
                if (Directory.Exists(modPluginsFolder))
                {
                    foreach (string file in Directory.GetFiles(modPluginsFolder, "*", SearchOption.AllDirectories))
                    {
                        string relativeFilePath = Path.GetRelativePath(modPluginsFolder, file);
                        string fullPath = Path.Combine(this.GamePath, relativeFilePath);

                        // Check if the file exists in the game path and delete it
                        if (existingFiles.Contains(relativeFilePath))
                        {
                            this.textProgress.Report($"Deleting file {relativeFilePath} from {mod.Name}...");
                            File.Delete(fullPath);
                        }
                    }
                }
            }
        }
        private void CopyDirectoryContents(string sourceDir, string targetDir)
        {
            // Create the target directory if it doesn't exist
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            // Copy each file in the source directory to the target directory
            foreach (string sourceFile in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(sourceFile);
                string destFile = Path.Combine(targetDir, fileName);
                File.Copy(sourceFile, destFile, true);
            }

            // Copy each subdirectory in the source directory to the target directory
            foreach (string sourceSubDir in Directory.GetDirectories(sourceDir))
            {
                string subDirName = Path.GetFileName(sourceSubDir);
                string destSubDir = Path.Combine(targetDir, subDirName);
                CopyDirectoryContents(sourceSubDir, destSubDir);
            }
        }
    }
}