using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Diagnostics;
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

			LoadRSSFeed();
		}

		private void LoadRSSFeed()
		{
			try
			{
				WebClient webClient = new WebClient();
				webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);
				webClient.DownloadStringAsync(new Uri(rssFeedLink));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading RSS feed: " + ex.Message);
			}
		}

		private void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			try
			{
				if (e.Error == null && !e.Cancelled)
				{
					XDocument xdoc = XDocument.Parse(e.Result);
					List<RssItem> items = new List<RssItem>();

					foreach (XElement item in xdoc.Descendants("item"))
					{
						RssItem rssItem = new RssItem
						{
							Title = item.Element("title").Value,
							Link = item.Element("link").Value,
							Image = GetImageFromUrl(item.Element("image").Value),
							ModId = GetModIdFromLink(item.Element("link").Value) 
						};

						items.Add(rssItem);
					}

					feedListView.ItemsSource = items;
				}
				else
				{
					MessageBox.Show("Error loading RSS feed: " + e.Error?.Message);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error parsing RSS feed: " + ex.Message);
			}
		}

		private ImageSource GetImageFromUrl(string imageUrl)
		{
			try
			{
				WebClient webClient = new WebClient();
				byte[] bytes = webClient.DownloadData(imageUrl);
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = new MemoryStream(bytes);
				bitmapImage.EndInit();
				return bitmapImage;
			}
			catch (Exception)
			{
				
				return null;
			}
		}

		private int GetModIdFromLink(string link)
		{
			// Fuck you
			int startIndex = link.LastIndexOf("/") + 1;
			int endIndex = link.Length;
			string modIdStr = link.Substring(startIndex, endIndex - startIndex);
			return int.Parse(modIdStr);
		}

		private void DownloadButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button)
			{
				if (button.DataContext is RssItem rssItem)
				{
					// Apply the theme to the GBModPrompt window
					GBModPrompt modPrompt = new GBModPrompt(game_id, rssItem.ModId.ToString());
					Themes.UpdateForm(Themes.CURRENT_THEME, modPrompt);
					modPrompt.ShowDialog();
				}
			}
		}

		public class RssItem
		{
			public string Title { get; set; }
			public string Link { get; set; }
			public ImageSource Image { get; set; }
			public int ModId { get; set; } 
		}
	}
}
