using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Concursus.Classes;
using SevenZipExtractor;

namespace Concursus
{
	/// <summary>
	/// Interaction logic for GBModPrompt.xaml
	/// </summary>
	public partial class GBModPrompt : Window
	{

		static string DESCRIPTION_HTML = @"
            <html>
                <head>
                    <style>
                        * {
                            font-family: Arial, Helvetica, sans-serif;
                        }
                    </style>
                </head>
                <body>
                    [REPLACE]
                </body>
            </html>
        ";

		public GamebananaMod mod;

		public GBModPrompt()
		{
			new GBModPrompt(null, null).ShowDialog();
			this.Close();
		}

		public GBModPrompt(string game_id = null, string mod_id = null)
		{
			this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			if (game_id == null && mod_id == null)
			{
				string[] args = Environment.GetCommandLineArgs();
				if (args.Length != 2)
					return;

				string arg = args[1];
				if (arg.StartsWith(Utils.MM_PROTOCOL_LINK))
					arg = arg.Substring((Utils.MM_PROTOCOL_LINK).Length);

				string[] res = arg.Trim().Replace("\\", "/").Trim('/').Split('_');
				if (res.Length != 2)
					return;
				game_id = res[0];
				mod_id = res[1];
			}

			mod = GamebananaMod.GetModInfoFromID(game_id, mod_id);

			InitializeComponent();
			Themes.UpdateForm(Themes.CURRENT_THEME, this);

			if (mod == null)
			{
				MessageBox.Show($"Failed getting mod with id {mod_id}!", "Failed getting mod", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(-1);
			}

			if (mod.mod_dir_path == "mods")
			{
				MessageBox.Show($"{mod.GameName} path has not been set yet! Please set it in the normal manager first before trying to install mods for it.", "Path not set", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(-1);
			}

			this.Title = $"Download {mod.name} for {mod.GameName}";
			this.txtGame.Text = mod.GameName;
			this.txtModName.Text = mod.name;
			this.txtSubmitter.Text = mod.submitter;
			this.txtVersion.Text = mod.version;


			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(mod.images[0], UriKind.Absolute);
			bitmap.EndInit();

			image.Source = bitmap;

			string description = DESCRIPTION_HTML.Replace("[REPLACE]", mod.description);
			broDescription.NavigateToString(description);

			cboFiles.ItemsSource = mod.files;
			cboFiles.SelectedIndex = 0;
		}

		private async void btnDownload_Click(object sender, RoutedEventArgs e)
		{
			Progress<string> progress = new Progress<string>(e => {
				txtProgress.Text += $"{e}\n";
				txtProgress.ScrollToEnd();
			});

			btnDownload.IsEnabled = false;

			string dl_link = mod.files[cboFiles.SelectedValue.ToString()].download_link;
			byte[] data = null;
			await Task.Run(() =>
			{
				data = Utils.Download(dl_link, progress);
			});
			if (data == null)
			{
				MessageBox.Show($"Failed to download {dl_link}!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			string output_dir = mod.mod_dir_path;

			txtProgress.Text += $"Reading archive...\n";
			string parent = "";
			bool found_data_dir = false;
			bool foundCBBFile = false;
			bool foundCBBCypher = false;
			bool foundAudioFiles = false;
			using (MemoryStream stream = new MemoryStream(data))
			using (ArchiveFile archiveFile = new ArchiveFile(stream))
			{

				foreach (var entry in archiveFile.Entries)
				{
					// ...

					string fileExtension = System.IO.Path.GetExtension(entry.FileName).ToLower();

					if (fileExtension == ".mp3" || fileExtension == ".wav" || fileExtension == ".flac" || fileExtension == ".ogg" || fileExtension == ".xm")
					{
						foundAudioFiles = true;

						// Prompt the user to choose between different options
						MessageBoxResult result = MessageBox.Show($"Is this a BombRushRadio mod?", $"{fileExtension.ToUpper()} File Detected", MessageBoxButton.YesNo, MessageBoxImage.Question);

						if (result == MessageBoxResult.Yes)
						{

						}

						// Continue processing other entries
						continue;
					}

				}

				if (foundAudioFiles)
				{
					// Add specific actions for audio files
					txtProgress.Text += $"Found an audio file! Handling it separately...\n";
					parent = mod.GetValidFolderName();
					output_dir = System.IO.Path.Combine(output_dir, mod.GetValidFolderName());
					found_data_dir = true;

					// Create the required directory structure inside the mod folder
					string pluginsFolder = System.IO.Path.Combine(output_dir, "plugins");
					string audiodata = System.IO.Path.Combine(pluginsFolder, "Bomb Rush Cyberfunk_Data");
					string streamlikenetflix = System.IO.Path.Combine(audiodata, "StreamingAssets");
					string modslikedniwetamp = System.IO.Path.Combine(streamlikenetflix, "Mods");
					string BombRushRadio = System.IO.Path.Combine(modslikedniwetamp, "BombRushRadio");
					string Songs1 = System.IO.Path.Combine(BombRushRadio, "Songs");

					// Check if the directories exist, and create them if not
					if (!Directory.Exists(pluginsFolder)) Directory.CreateDirectory(pluginsFolder);
					if (!Directory.Exists(audiodata)) Directory.CreateDirectory(audiodata);
					if (!Directory.Exists(streamlikenetflix)) Directory.CreateDirectory(streamlikenetflix);
					if (!Directory.Exists(modslikedniwetamp)) Directory.CreateDirectory(modslikedniwetamp);
					if (!Directory.Exists(BombRushRadio)) Directory.CreateDirectory(BombRushRadio);
					if (!Directory.Exists(Songs1)) Directory.CreateDirectory(Songs1);

					// Move audio files to the Songs folder
					foreach (var fileEntry in archiveFile.Entries)
					{
						string entryFileName = fileEntry.FileName;

						if (entryFileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
							entryFileName.EndsWith(".flac", StringComparison.OrdinalIgnoreCase) ||
							entryFileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
							entryFileName.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) ||
							entryFileName.EndsWith(".xm", StringComparison.OrdinalIgnoreCase))
						{
							// Construct the destination path inside the Songs folder
							string destinationPath = System.IO.Path.Combine(Songs1, System.IO.Path.GetFileName(entryFileName));

							// Extract and move the file
							using (FileStream fs = File.Create(destinationPath))
							{
								fileEntry.Extract(fs);
							}

							// Display a message
							txtProgress.Text += $"Moved {entryFileName} to BombRushRadio's Songs folder.\n";

							string dataFolder = System.IO.Path.Combine(output_dir, mod.GameFolderDataName);
							if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
						}
					}
				}

				foreach (var entry in archiveFile.Entries)
				{
					List<string> split = entry.FileName.Split(new char[] { '\\', '/' }).ToList();
					int idx = split.IndexOf(mod.GameFolderDataName);

					if (entry.FileName.EndsWith(".cbb", StringComparison.OrdinalIgnoreCase))
					{
						// Prompt the user to choose between "no" and "no_cypher"
						MessageBoxResult result = MessageBox.Show("Do you want the mod to go in 'no_cypher'?", "CBB File Detected", MessageBoxButton.YesNo, MessageBoxImage.Question);

						if (result == MessageBoxResult.Yes)
						{
							// User chose "Yes," set foundCBBCypher to true
							foundCBBCypher = true;
						}
						else
						{
							// User chose "No," set foundCBBFile to true
							foundCBBFile = true;
						}

						// Continue
						continue;
					}

					if (foundCBBFile)
					{
						// Add specific actions in here for cbb shit, can copy pasta for other files :adachi_true:
						txtProgress.Text += $"Found a .cbb file! Handling it separately...\n";
						parent = mod.GetValidFolderName();
						output_dir = System.IO.Path.Combine(output_dir, mod.GetValidFolderName());
						found_data_dir = true;

						// Create the required directory structure inside the mod folder
						string pluginsFolder = System.IO.Path.Combine(output_dir, "plugins");
						string bepInExFolder = System.IO.Path.Combine(pluginsFolder, "BepInEx");
						string configFolder = System.IO.Path.Combine(bepInExFolder, "config");
						string crewBoomFolder = System.IO.Path.Combine(configFolder, "CrewBoom");

						// Check if the directories exist, and create them if not
						if (!Directory.Exists(pluginsFolder)) Directory.CreateDirectory(pluginsFolder);
						if (!Directory.Exists(bepInExFolder)) Directory.CreateDirectory(bepInExFolder);
						if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);
						if (!Directory.Exists(crewBoomFolder)) Directory.CreateDirectory(crewBoomFolder);

						// Move .cbb and .json files to the CrewBoom folder
						foreach (var fileEntry in archiveFile.Entries)
						{
							string entryFileName = fileEntry.FileName;

							if (entryFileName.EndsWith(".cbb", StringComparison.OrdinalIgnoreCase) ||
								entryFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
							{
								// Construct the destination path inside the CrewBoom folder
								string destinationPath = System.IO.Path.Combine(crewBoomFolder, System.IO.Path.GetFileName(entryFileName));

								if (entryFileName.EndsWith(".cbb", StringComparison.OrdinalIgnoreCase))
								{
									// For .cbb files, read the content into a MemoryStream and then write to the destination
									using (MemoryStream memoryStream = new MemoryStream())
									{
										fileEntry.Extract(memoryStream);
										File.WriteAllBytes(destinationPath, memoryStream.ToArray());
									}
								}
								else
								{
									// For other files, extract and move as before
									using (FileStream fs = File.Create(destinationPath))
									{
										fileEntry.Extract(fs);
									}
								}

								// Crewboom message
								txtProgress.Text += $"Moved {entryFileName} to CrewBoom folder.\n";

								string dataFolder = System.IO.Path.Combine(output_dir, mod.GameFolderDataName);
								if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
							}
						}
					}
					if (foundCBBCypher)
					{
						txtProgress.Text += $"Found a .cbb file! Handling it separately...\n";
						parent = mod.GetValidFolderName();
						output_dir = System.IO.Path.Combine(output_dir, parent);
						found_data_dir = true;

						// Create the required directory structure inside the mod folder
						string pluginsFolder = System.IO.Path.Combine(output_dir, "plugins");
						string bepInExFolder = System.IO.Path.Combine(pluginsFolder, "BepInEx");
						string configFolder = System.IO.Path.Combine(bepInExFolder, "config");
						string crewBoomFolder = System.IO.Path.Combine(configFolder, "CrewBoom");
						string noCypherFolder = System.IO.Path.Combine(crewBoomFolder, "no_cypher");

						// Check if the directories exist, and create them if not
						if (!Directory.Exists(output_dir)) Directory.CreateDirectory(output_dir);
						if (!Directory.Exists(pluginsFolder)) Directory.CreateDirectory(pluginsFolder);
						if (!Directory.Exists(bepInExFolder)) Directory.CreateDirectory(bepInExFolder);
						if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);
						if (!Directory.Exists(crewBoomFolder)) Directory.CreateDirectory(crewBoomFolder);
						if (!Directory.Exists(noCypherFolder)) Directory.CreateDirectory(noCypherFolder);

						// Move .cbb and .json files to the no_cypher folder
						foreach (var fileEntry in archiveFile.Entries)
						{
							string entryFileName = fileEntry.FileName;

							if (entryFileName.EndsWith(".cbb", StringComparison.OrdinalIgnoreCase) ||
								entryFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
							{
								// Construct the destination path inside the CrewBoom folder no_cupher
								string destinationPath = System.IO.Path.Combine(noCypherFolder, System.IO.Path.GetFileName(entryFileName));

								if (entryFileName.EndsWith(".cbb", StringComparison.OrdinalIgnoreCase))
								{
									// For .cbb files, read the content into a MemoryStream and then write to the destination
									using (MemoryStream memoryStream = new MemoryStream())
									{
										fileEntry.Extract(memoryStream);
										File.WriteAllBytes(destinationPath, memoryStream.ToArray());
									}
								}
								else
								{
									// For other files, extract and move as before
									using (FileStream fs = File.Create(destinationPath))
									{
										fileEntry.Extract(fs);
									}
								}

								// Crewboom message
								txtProgress.Text += $"Moved {entryFileName} to CrewBoom's no_cypher folder.\n";

								string dataFolder = System.IO.Path.Combine(output_dir, mod.GameFolderDataName);
								if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
							}
						}
					}
					if (idx != -1)
					{
						if (idx == 0) // If the data folder is the parent folder in the archive, then make a new folder manually
						{
							txtProgress.Text += $"{mod.GameFolderDataName} does not have a parent! Creating new parent name...\n";
							parent = mod.GetValidFolderName();
							output_dir = System.IO.Path.Combine(output_dir, mod.GetValidFolderName());
							found_data_dir = true;
							break;
						}
						else if (idx == 1)
						{
							txtProgress.Text += $"{mod.GameFolderDataName} has a parent! Storing parent name...\n";
							parent = split[idx - 1];
							found_data_dir = true;
							break;
						}
						else
						{
							MessageBox.Show("Archive has a invalid structure! Aborting the operation.", "Invalid Structure", MessageBoxButton.OK, MessageBoxImage.Error);
							this.Close();
						}
					}
				}

				// Existing code for handling the case when mod.GameFolderDataName is not found
				// Replace it with the following code
				if (!found_data_dir && !foundCBBFile)
				{
					MessageBox.Show("Archive has an invalid structure! Aborting the operation.", "Invalid Structure", MessageBoxButton.OK, MessageBoxImage.Error);
					this.Close();
				}

				archiveFile.Extract(output_dir);
			}

			string config_dir = System.IO.Path.Combine(mod.mod_dir_path, parent);
			if (!File.Exists(System.IO.Path.Combine(config_dir, ModConfig.CONFIG_FILE)))
			{
				txtProgress.Text += $"Config not found! Generating new one based on mod data....\n";
				mod.GetModConfig().SaveToPath(config_dir);
			}
			txtProgress.Text += $"Finished downloading {mod.name}! Closing the window in 5 seconds...\n";
			await Task.Delay(TimeSpan.FromSeconds(5));
			this.Close();
		}
		private void toggleImage_Click(object sender, RoutedEventArgs e)
		{
			if (toggleImage.IsChecked == true)
			{
				image.Visibility = Visibility.Collapsed;
			}
			else
			{
				image.Visibility = Visibility.Visible;
			}
		}


		//public GBModPrompt(string test)
		//{
		//    InitializeComponent();
		//}
	}
}