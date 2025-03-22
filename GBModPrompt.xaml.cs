using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

		private void Log(string message)
		{
			try
			{
				File.AppendAllText("modmanager.log", $"{DateTime.Now}: {message}\n");
			}
			catch (Exception ex)
			{
				// Optional: Handle logging errors, maybe write to a different file or console
			}
		}

		public GBModPrompt(string game_id = null, string mod_id = null)
		{
			Log("Constructor started");
			this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			Log("WindowStartupLocation set");

			if (game_id == null && mod_id == null)
			{
				Log("game_id and mod_id are null, parsing command line arguments");
				string[] args = Environment.GetCommandLineArgs();
				Log($"Command line arguments count: {args.Length}");
				if (args.Length != 2)
					return;

				string arg = args[1];
				Log($"Argument extracted: {arg}");
				if (arg.StartsWith(Utils.MM_PROTOCOL_LINK))
					arg = arg.Substring((Utils.MM_PROTOCOL_LINK).Length);

				Log($"Processed argument: {arg}");
				string[] res = arg.Trim().Replace("\\", "/").Trim('/').Split('_');
				Log($"Argument split into: {String.Join(", ", res)}");
				if (res.Length != 2)
					return;
				game_id = res[0];
				mod_id = res[1];
				Log($"game_id: {game_id}, mod_id: {mod_id}");
			}

			mod = GamebananaMod.GetModInfoFromID(game_id, mod_id);
			Log($"Mod fetched: {mod?.name ?? "null"}");

			InitializeComponent();
			Log("InitializeComponent called");
			Themes.UpdateForm(Themes.CURRENT_THEME, this);
			Log("Themes updated");

			if (mod == null)
			{
				Log($"Failed getting mod with id {mod_id}");
				MessageBox.Show($"Failed getting mod with id {mod_id}!", "Failed getting mod", MessageBoxButton.OK, MessageBoxImage.Error);
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
			// Disable the button and allow its content to stretch.
			btnDownload.IsEnabled = false;
			btnDownload.HorizontalContentAlignment = HorizontalAlignment.Stretch;

			// Create a determinate ProgressBar that fills the button's width.
			ProgressBar pb = new ProgressBar
			{
				IsIndeterminate = false,
				Minimum = 0,
				Maximum = 100,
				Value = 0,
				Height = btnDownload.Height,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(0)
			};
			btnDownload.Content = pb;

			// Create a progress handler that updates the ProgressBar.
			var progressHandler = new Progress<double>(value =>
			{
				pb.Value = value;
				txtProgress.Text = $"Downloading: {value:0.00}% completed";
			});

			string dl_link = mod.files[cboFiles.SelectedValue.ToString()].download_link;
			byte[] data = null;

			try
			{
				// Download the file with progress reporting.
				data = await DownloadDataAsync(dl_link, progressHandler);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed to download {dl_link}!\n{ex.Message}",
								"Failed", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (data == null || data.Length == 0)
			{
				MessageBox.Show($"Failed to download {dl_link}!",
								"Failed", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Base directory where mods are stored.
			string baseOutputDir = mod.mod_dir_path;
			// This will hold the final extraction folder (a new mod folder if needed).
			string output_dir = baseOutputDir;
			// This variable will store the name of the new folder (if created).
			string parentFolder = "";

			// List to store the names of files manually moved.
			List<string> movedFiles = new List<string>();

			txtProgress.Text += "Reading archive...\n";

			using (MemoryStream stream = new MemoryStream(data))
			using (ArchiveFile archiveFile = new ArchiveFile(stream))
			{
				// Check if the archive contains any audio files.
				bool isAudioMod = archiveFile.Entries.Any(e =>
				{
					string ext = System.IO.Path.GetExtension(e.FileName).ToLower();
					return ext == ".mp3" || ext == ".wav" || ext == ".flac" ||
						   ext == ".ogg" || ext == ".xm";
				});

				// Check if the archive contains any .cbb files.
				bool isCbbMod = archiveFile.Entries.Any(e =>
					e.FileName.EndsWith(".cbb", StringComparison.OrdinalIgnoreCase));

				bool structureFound = false;

				if (isAudioMod)
				{
					// Audio mods: Create a new mod folder and build the BombRushRadio structure.
					parentFolder = mod.GetValidFolderName();
					output_dir = System.IO.Path.Combine(baseOutputDir, parentFolder);

					string pluginsFolder = System.IO.Path.Combine(output_dir, "plugins");
					string audioData = System.IO.Path.Combine(pluginsFolder, "Bomb Rush Cyberfunk_Data");
					string streamingAssets = System.IO.Path.Combine(audioData, "StreamingAssets");
					string modsFolder = System.IO.Path.Combine(streamingAssets, "Mods");
					string bombRushRadioFolder = System.IO.Path.Combine(modsFolder, "BombRushRadio");
					string songsFolder = System.IO.Path.Combine(bombRushRadioFolder, "Songs");

					Directory.CreateDirectory(pluginsFolder);
					Directory.CreateDirectory(audioData);
					Directory.CreateDirectory(streamingAssets);
					Directory.CreateDirectory(modsFolder);
					Directory.CreateDirectory(bombRushRadioFolder);
					Directory.CreateDirectory(songsFolder);

					// Move the audio files into the Songs folder.
					foreach (var entry in archiveFile.Entries)
					{
						string ext = System.IO.Path.GetExtension(entry.FileName).ToLower();
						if (ext == ".mp3" || ext == ".wav" || ext == ".flac" ||
							ext == ".ogg" || ext == ".xm")
						{
							string destPath = System.IO.Path.Combine(songsFolder, System.IO.Path.GetFileName(entry.FileName));
							using (FileStream fs = File.Create(destPath))
							{
								entry.Extract(fs);
							}
							txtProgress.Text += $"Moved {entry.FileName} to BombRushRadio's Songs folder.\n";
						}
					}
					structureFound = true;
				}
				else if (isCbbMod)
				{
					// For mods with .cbb files, ask the user once whether to use "no_cypher".
					MessageBoxResult result = MessageBox.Show("Do you want the mod to go in 'no_cypher'?",
															  "CBB File Detected",
															  MessageBoxButton.YesNo,
															  MessageBoxImage.Question);
					bool useNoCypher = (result == MessageBoxResult.Yes);

					parentFolder = mod.GetValidFolderName();
					output_dir = System.IO.Path.Combine(baseOutputDir, parentFolder);

					string pluginsFolder = System.IO.Path.Combine(output_dir, "plugins");
					string bepInExFolder = System.IO.Path.Combine(pluginsFolder, "BepInEx");
					string configFolder = System.IO.Path.Combine(bepInExFolder, "config");
					string crewBoomFolder = System.IO.Path.Combine(configFolder, "CrewBoom");
					// Target folder depends on the user response.
					string targetFolder = useNoCypher ? System.IO.Path.Combine(crewBoomFolder, "no_cypher") : crewBoomFolder;
					string bombRushDataFolder = System.IO.Path.Combine(output_dir, "Bomb Rush Cyberfunk_Data");

					Directory.CreateDirectory(pluginsFolder);
					Directory.CreateDirectory(bepInExFolder);
					Directory.CreateDirectory(configFolder);
					Directory.CreateDirectory(crewBoomFolder);
					Directory.CreateDirectory(targetFolder);
					Directory.CreateDirectory(bombRushDataFolder);

					// Move .cbb and .json files into the target folder.
					foreach (var entry in archiveFile.Entries)
					{
						if (entry.FileName.EndsWith(".cbb", StringComparison.OrdinalIgnoreCase) ||
							entry.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
						{
							string fileName = System.IO.Path.GetFileName(entry.FileName);
							movedFiles.Add(fileName);
							string destPath = System.IO.Path.Combine(targetFolder, fileName);
							using (FileStream fs = File.Create(destPath))
							{
								entry.Extract(fs);
							}
							txtProgress.Text += $"Moved {entry.FileName} to {(useNoCypher ? "CrewBoom's no_cypher" : "CrewBoom")} folder.\n";
						}
					}
					structureFound = true;
				}
				else
				{
					// Normal mod archive:
					// Try to find the game data folder (mod.GameFolderDataName) in the archive.
					foreach (var entry in archiveFile.Entries)
					{
						List<string> parts = entry.FileName.Split(new char[] { '\\', '/' },
												  StringSplitOptions.RemoveEmptyEntries).ToList();
						int idx = parts.IndexOf(mod.GameFolderDataName);
						if (idx != -1)
						{
							if (idx == 0)
							{
								// Game data folder is at the root: create a new mod folder and also create the data folder.
								parentFolder = mod.GetValidFolderName();
								output_dir = System.IO.Path.Combine(baseOutputDir, parentFolder);
								Directory.CreateDirectory(output_dir);
								string dataFolder = System.IO.Path.Combine(output_dir, mod.GameFolderDataName);
								Directory.CreateDirectory(dataFolder);
								structureFound = true;
								break;
							}
							else if (idx == 1)
							{
								// Use the parent folder already provided by the archive.
								parentFolder = parts[idx - 1];
								output_dir = System.IO.Path.Combine(baseOutputDir, parentFolder);
								structureFound = true;
								break;
							}
							else
							{
								MessageBox.Show("Archive has an invalid structure! Aborting the operation.",
												"Invalid Structure",
												MessageBoxButton.OK,
												MessageBoxImage.Error);
								this.Close();
								return;
							}
						}
					}
					// If no specific game data folder was found, assume a flat archive and create a new mod folder.
					if (!structureFound)
					{
						parentFolder = mod.GetValidFolderName();
						output_dir = System.IO.Path.Combine(baseOutputDir, parentFolder);
						Directory.CreateDirectory(output_dir);
						// Optionally create the game data folder inside if your mod system expects it.
						string dataFolder = System.IO.Path.Combine(output_dir, mod.GameFolderDataName);
						if (!Directory.Exists(dataFolder))
						{
							Directory.CreateDirectory(dataFolder);
						}
						structureFound = true;
					}
				}

				if (!structureFound)
				{
					MessageBox.Show("Archive has an invalid structure! Aborting the operation.",
									"Invalid Structure",
									MessageBoxButton.OK,
									MessageBoxImage.Error);
					this.Close();
					return;
				}

				// Attempt extraction; catch errors from invalid archives.
				try
				{
					archiveFile.Extract(output_dir);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Extraction failed: " + ex.Message, "Extraction Error",
									MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				// If it's a CBB mod, delete duplicate .cbb and .json files from the root mod folder.
				if (movedFiles.Count > 0)
				{
					foreach (string fileName in movedFiles)
					{
						string duplicatePath = System.IO.Path.Combine(output_dir, fileName);
						if (File.Exists(duplicatePath))
						{
							File.Delete(duplicatePath);
							txtProgress.Text += $"Deleted duplicate {fileName} from root mod folder.\n";
						}
					}
				}
			}

			// Generate configuration if it doesn't exist.
			string config_dir = System.IO.Path.Combine(mod.mod_dir_path, parentFolder);
			if (!File.Exists(System.IO.Path.Combine(config_dir, ModConfig.CONFIG_FILE)))
			{
				txtProgress.Text += "Config not found! Generating new one based on mod data....\n";
				mod.GetModConfig().SaveToPath(config_dir);
			}
			txtProgress.Text += $"\nFinished downloading {mod.name}! Closing the window in 5 seconds...\n";
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

		private async Task<byte[]> DownloadDataAsync(string url, IProgress<double> progress)
		{
			using (var client = new HttpClient())
			{
				// Request the content without buffering it entirely in memory.
				using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
				{
					response.EnsureSuccessStatusCode();
					long total = response.Content.Headers.ContentLength ?? -1L;
					using (var stream = await response.Content.ReadAsStreamAsync())
					{
						using (var ms = new MemoryStream())
						{
							var buffer = new byte[81920];
							long totalRead = 0;
							int read;
							while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
							{
								ms.Write(buffer, 0, read);
								totalRead += read;
								if (total != -1)
								{
									double percentage = (double)totalRead / total * 100;
									progress.Report(percentage);
								}
							}
							return ms.ToArray();
						}
					}
				}
			}
		}
	}
}
