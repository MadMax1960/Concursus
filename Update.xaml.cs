using SevenZipExtractor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Concursus
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    public partial class Update : Window
    {
        private JsonObject data;
        public Update(JsonObject releaseData)
        {
            this.data = releaseData;
            InitializeComponent();
            Themes.UpdateForm(Themes.CURRENT_THEME, this);
            this.txtCurrVersion.Text = App.APP_VERSION;
            this.txtLatestVersion.Text = (string)releaseData["tag_name"];
            this.txtUpdateNotes.Text = (string)releaseData["body"];
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnCancel.IsEnabled = false;
            btnUpdate.IsEnabled = false;

            Progress<string> progress = new Progress<string>(e => txtLog.Text += $"{e}\n");

            JsonObject asset = (JsonObject)((JsonArray)this.data["assets"])[0];

            string download_link = (string)asset["browser_download_url"].AsValue();

            byte[] data = null;
            await Task.Run(() => { data = Utils.Download(download_link, progress); });

            string original_folder_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (!Directory.Exists(App.OLD_FOLDER))
                Directory.CreateDirectory(App.OLD_FOLDER);

            using (MemoryStream stream = new MemoryStream(data))
            using (ArchiveFile archiveFile = new ArchiveFile(stream))
            {
                foreach(var entry in archiveFile.Entries)
                {
                    if (!File.Exists(entry.FileName))
                        continue;
                    MoveFile(entry.FileName, System.IO.Path.Combine(App.OLD_FOLDER, entry.FileName));
                }
                archiveFile.Extract("", true);
            }
            Environment.Exit(0);
        }

        private void MoveFile(string input, string output)
        {
            if (output.IndexOfAny(new char[] { '\\', '/' }) != -1)
                new System.IO.FileInfo(output).Directory.Create();
            File.Move(input, output, true);
        }
    }
}
