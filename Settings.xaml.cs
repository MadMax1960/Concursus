using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Windows.Controls;
using Microsoft.VisualBasic;

namespace Concursus
{
	public partial class Settings : Window
	{
		public Settings()
		{
			InitializeComponent();
			Themes.UpdateForm(Themes.CURRENT_THEME, this);
			foreach (Themes.ThemeOption theme_option in Enum.GetValues(typeof(Themes.ThemeOption)))
			{
				if (theme_option == Themes.ThemeOption.Invalid)
					continue;
				cboThemes.Items.Add(Themes.GetStringFromOption(theme_option));
			}
			cboThemes.SelectedItem = Themes.GetStringFromOption(Themes.CURRENT_THEME);
		}

		private void cboThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Themes.CURRENT_THEME = Themes.GetOptionFromString(cboThemes.SelectedItem.ToString());
			Themes.UpdateForm(Themes.CURRENT_THEME, this);
			if (this.Owner != null)
				Themes.UpdateForm(Themes.CURRENT_THEME, this.Owner);
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.App_Theme = Themes.GetStringFromOption(Themes.CURRENT_THEME);
			Properties.Settings.Default.Save();
			this.Close();
		}

		private Dictionary<string, int> knownGameIDs = new Dictionary<string, int>
{
    { "SonicSuperstars.exe", 1965917 },
	{ "SOUL HACKERS2.exe", 45 },
    // Add more entries for other known games as needed
};

		private Dictionary<string, int> GBRSSValues = new Dictionary<string, int>
{
	{ "SonicSuperstars.exe", 18552 },
	{ "SOUL HACKERS2.exe", 17065 },
	{ "Etrian Odyssey.exe", 18479 },
	{ "Etrian Odyssey 2.exe", 18480 },
	{ "Etrian Odyssey 3.exe", 18481 },
    // Add more entries for other known games as needed
};

		private Dictionary<string, string> GBONECLICKValues = new Dictionary<string, string>
{
	{ "SonicSuperstars.exe", "SUPERSTARS" },
	{ "SOUL HACKERS2.exe", "SH2" },
	{ "Etrian Odyssey.exe", "EOHD" },
	{ "Etrian Odyssey 2.exe", "EO2HD" },
	{ "Etrian Odyssey 3.exe", "EO3HD" }
    // Add more entries for other known games as needed
};

		private void btnAddGame_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if (ofd.ShowDialog() != true)
				return;

			string exePath = ofd.FileName;
			string dataDir = "";

			foreach (string dir in Directory.GetDirectories(Path.GetDirectoryName(exePath)))
			{
				if (dir.Contains("_Data"))
				{
					dataDir = dir;
					break;
				}
			}

			string key = Path.GetFileNameWithoutExtension(exePath); // Remove ".exe" extension
			string gameName = Path.GetFileName(Path.GetDirectoryName(exePath)); // Get the parent folder name

			int gameID = 0; // Default to 0

			if (knownGameIDs.ContainsKey(Path.GetFileName(exePath)))
			{
				gameID = knownGameIDs[Path.GetFileName(exePath)]; // Set the GameID if it's a known game
			}

			int GBRSS = 0; // Initialize GBRSS as an int
			string GBONECLICK = ""; // Initialize GBONECLICK as a string

			if (GBRSSValues.ContainsKey(Path.GetFileName(exePath)))
			{
				GBRSS = GBRSSValues[Path.GetFileName(exePath)]; // Set GBRSS as an int
			}

			if (GBONECLICKValues.ContainsKey(Path.GetFileName(exePath)))
			{
				GBONECLICK = GBONECLICKValues[Path.GetFileName(exePath)]; // Set GBONECLICK as a string
			}

			List<Game> games = new List<Game>();
			games.Add(new Game()
			{
				key = key,
				GameName = gameName,
				GameFolderDataName = Path.GetFileName(dataDir),
				GamePath = Path.GetDirectoryName(exePath),
				GameExecutable = exePath,
				GameID = gameID,
				GBRSS = GBRSS, // Set GBRSS as an int
				GBONECLICK = GBONECLICK // Set GBONECLICK as a string
			});

			string gameJson = JsonConvert.SerializeObject(games, Formatting.Indented);

			string gameDataFolderPath = "GameData";
			if (!Directory.Exists(gameDataFolderPath))
			{
				Directory.CreateDirectory(gameDataFolderPath);
			}

			string jsonFileName = Path.Combine(gameDataFolderPath, $"{key}.json");
			File.WriteAllText(jsonFileName, gameJson);

			MessageBox.Show("Game added and JSON data saved!");
		}

		private void btnManualAdd_Click(object sender, RoutedEventArgs e)
		{
			string gamePath = Interaction.InputBox("Enter Game Path:", "Add Game (Manual)", "");
			string gameFolderDataName = Interaction.InputBox("Enter Game Folder Data Name:", "Add Game (Manual)", "");

			if (string.IsNullOrWhiteSpace(gamePath) || string.IsNullOrWhiteSpace(gameFolderDataName))
			{
				MessageBox.Show("Please provide valid input for Game Path and Game Folder Data Name.");
				return;
			}

			string GameFolderDataName = Path.GetFileName(gamePath); // Get the last folder name as Game Folder Data Name

			string key = Path.GetFileName(gamePath); // Get the folder name as key

			int gameID = 0; // Default to 0

			string gameIDInput = Interaction.InputBox("Enter Game ID:", "Add Game (Manual)", "");
			if (!string.IsNullOrWhiteSpace(gameIDInput) && int.TryParse(gameIDInput, out int parsedGameID))
			{
				gameID = parsedGameID;
			}

			List<Game> games = new List<Game>();
			games.Add(new Game()
			{
				key = key,
				GameName = key, // You can use key as the default GameName
				GameFolderDataName = GameFolderDataName,
				GamePath = gamePath,
				GameExecutable = Path.Combine(gamePath, $"{key}.exe"), // Assuming the executable is named after the folder
				GameID = gameID // Set the GameID here with the desired value or user input
			});

			string gameJson = JsonConvert.SerializeObject(games, Formatting.Indented);

			// Create the GameData folder if it doesn't exist
			string gameDataFolderPath = "GameData";
			if (!Directory.Exists(gameDataFolderPath))
			{
				Directory.CreateDirectory(gameDataFolderPath);
			}

			// Save the game's data in a separate JSON file named after the key
			string jsonFileName = Path.Combine(gameDataFolderPath, $"{key}.json");
			File.WriteAllText(jsonFileName, gameJson);

			MessageBox.Show("Game added and JSON data saved!");
		}
	}
}
