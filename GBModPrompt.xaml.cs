using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length != 2)
                return;

            string arg = args[1];
            if (arg.StartsWith(Utils.MM_PROTOCOL + ':'))
                arg = arg.Substring((Utils.MM_PROTOCOL + ':').Length);
            string mod_id = String.Join(null, arg.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries));

            InitializeComponent();
            Themes.UpdateForm(Themes.CURRENT_THEME, this);

            mod = GamebananaMod.GetModInfoFromID(mod_id);

            if (mod == null)
            {
                MessageBox.Show($"Failed getting mod with id {mod_id}!", "Failed getting mod", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
            }

            if(mod.mod_dir_path == "mods")
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
            using (MemoryStream stream = new MemoryStream(data))
            using(ArchiveFile archiveFile = new ArchiveFile(stream))
            {
                foreach(var entry in archiveFile.Entries)
                {
                    List<string> split = entry.FileName.Split(new char[] { '\\', '/' }).ToList();
                    int idx = split.IndexOf(mod.GameFolderDataName);
                    if (idx != -1)
                    {
                        if(idx == 0) // If the data folder is the parent folder in the archive, then make a new folder manually
                        {
                            txtProgress.Text += $"{mod.GameFolderDataName} does not have a parent! Creating new parent name...\n";
                            parent = mod.GetValidFolderName();
                            output_dir = System.IO.Path.Combine(output_dir, mod.GetValidFolderName());
                            found_data_dir = true;
                            break;
                        } else if(idx == 1)
                        {
                            txtProgress.Text += $"{mod.GameFolderDataName} has a parent! Storing parent name...\n";
                            parent = split[idx - 1];
                            found_data_dir = true;
                            break;
                        } else
                        {
                            MessageBox.Show("Archive has a invalid structure! Aborting the operation.", "Invalid Structure", MessageBoxButton.OK, MessageBoxImage.Error);
                            Environment.Exit(-1);
                        }
                    }
                }

                if (!found_data_dir)
                {
                    MessageBox.Show("Archive has a invalid structure! Aborting the operation.", "Invalid Structure", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
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
            Environment.Exit(0);
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