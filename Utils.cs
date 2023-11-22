using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Concursus
{
    public class Utils
    {
        public const string MM_PROTOCOL = "concursus";
        public const string MM_PROTOCOL_LINK = $"{MM_PROTOCOL}://";
        public static void RegisterProtocol(string protocol, string title) {
            if(protocol == String.Empty)
                return;
            string exe_path = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
            RegistryKey key = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{protocol}");

            key.SetValue(String.Empty, $"URL: {title}");
            key.SetValue("URL Protocol", String.Empty);

            RegistryKey appTitle = key.CreateSubKey("Application");
            appTitle.SetValue("ApplicationName", title);

            key = key.CreateSubKey(@"shell\open\command");
            key.SetValue(String.Empty, $"\"{exe_path}\" %1");

            key.Close();
        }

        public static string GetTextFromURL(string url)
        {
            var options = new RestClientOptions(url)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("", Method.Get);
            RestResponse response = client.Execute(request);
            return response.Content;
        }

        public static byte[] Download(string link, IProgress<string> progress = null)
        {
            if (progress == null)
                progress = new Progress<string>();
            var options = new RestClientOptions(link)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("", Method.Get);
            progress.Report($"Downloading {link}");
            byte[] response = client.DownloadData(request);
            return response == null ? null : response;
        }

        public static void CheckForUpdate()
        {
            try
            {
                string json = Utils.GetTextFromURL(App.APP_UPDATE_ENDPOINT);
                JsonArray jsonArr = JsonSerializer.Deserialize<JsonArray>(json);
                JsonObject latestRelease = (JsonObject)jsonArr[0];
                string latest_release_tag = (string)latestRelease["tag_name"].AsValue();

                var current_version = Semver.SemVersion.Parse(App.APP_VERSION);
                var latest_version = Semver.SemVersion.Parse(latest_release_tag);
                if (current_version.ComparePrecedenceTo(latest_version) == -1) // -1 => outdated
                    new Update(latestRelease).ShowDialog();
            }
            catch (Exception)
            {

            }
        }
    }
}
