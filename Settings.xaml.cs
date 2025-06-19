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
	{ "Persona 5 Tactica.exe", 10 },
	{ "Bomb Rush Cyberfunk.exe", 11 },
	{ "K12_Data", 12 },
    { "Raidou TMOTSA.exe", 14 }
    // Add more entries for other known games as needed
};

		private Dictionary<string, int> GBRSSValues = new Dictionary<string, int>
{
	{ "SonicSuperstars.exe", 18552 },
	{ "SOUL HACKERS2.exe", 17065 },
	{ "Etrian Odyssey.exe", 18479 },
	{ "Etrian Odyssey 2.exe", 18480 },
	{ "Etrian Odyssey 3.exe", 18481 },
	{ "Persona 5 Tactica.exe", 18918 },
	{ "Bomb Rush Cyberfunk.exe", 18955 },
	{ "GK12.exe", 21360 },
    { "Raidou TMOTSA.exe", 22440 }
    // Add more entries for other known games as needed
};

		private Dictionary<string, string> GBONECLICKValues = new Dictionary<string, string>
{
	{ "SonicSuperstars.exe", "SUPERSTARS" },
	{ "SOUL HACKERS2.exe", "SH2" },
	{ "Etrian Odyssey.exe", "EOHD" },
	{ "Etrian Odyssey 2.exe", "EO2HD" },
	{ "Etrian Odyssey 3.exe", "EO3HD" },
	{ "Persona 5 Tactica.exe", "P5T" },
	{ "Bomb Rush Cyberfunk.exe", "BRC" },
	{ "GK12.exe", "AAIC" },
    { "Raidou TMOTSA.exe", "RRMoSA" }
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
			else
			{
				// GBRSS not found, prompt user for manual input
				GBRSS = PromptForGBRSS();
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

		private int PromptForGBRSS()
		{
			string input = Interaction.InputBox("Enter GB Game ID (0 if you don't know):", "Add Game (Manual)", "");

			if (int.TryParse(input, out int result))
			{
				return result;
			}
			else
			{
				MessageBox.Show("Invalid GBRSS value. Please enter a valid integer.");
				return PromptForGBRSS(); // Recursive call until a valid input is provided
			}
		}


		private void btnManualAdd_Click(object sender, RoutedEventArgs e)
		{
			string gamePath = Interaction.InputBox("Enter Game Path (the folder your mods will install into):", "Add Game (Manual)", "");
			string gameFolderDataName = Interaction.InputBox("Enter Game Folder Data Name (if you don't know what this is type something random):", "Add Game (Manual)", "");

			if (string.IsNullOrWhiteSpace(gamePath) || string.IsNullOrWhiteSpace(gameFolderDataName))
			{
				MessageBox.Show("Please provide valid input for Game Path and Game Folder Data Name.");
				return;
			}

			string gameName = Interaction.InputBox("Enter Game Name:", "Add Game (Manual)", "");

			if (string.IsNullOrWhiteSpace(gameName))
			{
				MessageBox.Show("Please provide a valid input for Game Name.");
				return;
			}

			string GameFolderDataName = Path.GetFileName(gamePath); // Get the last folder name as Game Folder Data Name

			string key = Path.GetFileName(gamePath); // Get the folder name as key

			int gameID = 0; // Default to 0

			string gameIDInput = Interaction.InputBox("Enter Game ID (0 if you don't know):", "Add Game (Manual)", "");
			if (!string.IsNullOrWhiteSpace(gameIDInput) && int.TryParse(gameIDInput, out int parsedGameID))
			{
				gameID = parsedGameID;
			}

			// Prompt the user for GBRSSFEED ID
			string gbrssFeedInput = Interaction.InputBox("Enter GB Game ID (0 if you don't know):", "Add Game (Manual)", "");

			if (int.TryParse(gbrssFeedInput, out int gbrssFeed))
			{
				// Continue with game creation
				List<Game> games = new List<Game>();
				games.Add(new Game()
				{
					key = key,
					GameName = gameName,
					GameFolderDataName = GameFolderDataName,
					GamePath = gamePath,
					GameExecutable = Path.Combine(gamePath, $"{key}.exe"), // Assuming the executable is named after the folder
					GameID = gameID, // Set the GameID here with the desired value or user input
					GBRSS = gbrssFeed // Set GBRSSFEED as an int
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
			else
			{
				MessageBox.Show("Invalid GBRSSFEED ID. Please enter a valid integer.");
			}
		}
	}
}