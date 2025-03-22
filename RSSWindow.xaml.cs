using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.IO;
using Concursus;

namespace YourNamespace
{
	public partial class RSSWindow : Window
	{
		private string rssFeedLink;
		private string game_id;

		public RSSWindow(string rssFeedLink)
		{
			this.rssFeedLink = rssFeedLink;
			int end_len = rssFeedLink.IndexOf('&') - (rssFeedLink.IndexOf('=') + 1);
			this.game_id = rssFeedLink.Substring(rssFeedLink.IndexOf('=') + 1, end_len);
			InitializeComponent();

			// Apply the theme
			Themes.UpdateForm(Themes.CURRENT_THEME, this);

			// Begin asynchronous loading of RSS feed
			LoadRSSFeedAsync();
		}

		private async void LoadRSSFeedAsync()
		{
			try
			{
				using (WebClient webClient = new WebClient())
				{
					// Asynchronously download the RSS feed content
					string rss = await webClient.DownloadStringTaskAsync(new Uri(rssFeedLink));
					XDocument xdoc = XDocument.Parse(rss);
					var items = new List<RssItem>();

					// Parse the RSS items
					foreach (XElement item in xdoc.Descendants("item"))
					{
						var rssItem = new RssItem
						{
							Title = item.Element("title").Value,
							Link = item.Element("link").Value,
							ModId = GetModIdFromLink(item.Element("link").Value)
							// Note: Image will be loaded asynchronously below
						};

						items.Add(rssItem);
					}

					// Bind the list to the ListView immediately for fast display
					feedListView.ItemsSource = items;

					// Now load images asynchronously so the UI isn’t blocked
					foreach (XElement item in xdoc.Descendants("item"))
					{
						string imageUrl = item.Element("image").Value;
						// Find the corresponding RssItem (assuming Link is unique)
						var correspondingItem = items.FirstOrDefault(i => i.Link == item.Element("link").Value);
						if (correspondingItem != null)
						{
							LoadImageAsync(correspondingItem, imageUrl);
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading RSS feed: " + ex.Message);
			}
		}

		private async void LoadImageAsync(RssItem rssItem, string imageUrl)
		{
			try
			{
				using (WebClient client = new WebClient())
				{
					byte[] bytes = await client.DownloadDataTaskAsync(imageUrl);
					BitmapImage bitmapImage = new BitmapImage();
					bitmapImage.BeginInit();
					bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
					bitmapImage.StreamSource = new MemoryStream(bytes);
					bitmapImage.EndInit();
					bitmapImage.Freeze(); // Freeze to make it cross-thread accessible
					rssItem.Image = bitmapImage;
				}
			}
			catch (Exception)
			{
				rssItem.Image = null; // Optionally set a default or error image
			}
		}

		private int GetModIdFromLink(string link)
		{
			int startIndex = link.LastIndexOf("/") + 1;
			string modIdStr = link.Substring(startIndex);
			return int.Parse(modIdStr);
		}

		private void DownloadButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button && button.DataContext is RssItem rssItem)
			{
				// Open the GBModPrompt window
				GBModPrompt modPrompt = new GBModPrompt(game_id, rssItem.ModId.ToString());
				Themes.UpdateForm(Themes.CURRENT_THEME, modPrompt);
				modPrompt.ShowDialog();
			}
		}

		public class RssItem : INotifyPropertyChanged
		{
			public string Title { get; set; }
			public string Link { get; set; }

			private ImageSource image;
			public ImageSource Image
			{
				get { return image; }
				set
				{
					if (image != value)
					{
						image = value;
						OnPropertyChanged("Image");
					}
				}
			}
			public int ModId { get; set; }

			public event PropertyChangedEventHandler PropertyChanged;
			protected void OnPropertyChanged(string propertyName)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
