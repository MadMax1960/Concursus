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

namespace YourNamespace
{
	public partial class RSSWindow : Window
	{
		private string rssFeedLink;

		public RSSWindow(string rssFeedLink)
		{
			this.rssFeedLink = rssFeedLink;
			InitializeComponent();
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
					
					string downloadLink = $"eohdmm://{rssItem.ModId}";

					try
					{
						
						ProcessStartInfo psi = new ProcessStartInfo
						{
							FileName = downloadLink,
							UseShellExecute = true
						};
						Process.Start(psi);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Failed to open link: {ex.Message}");
					}
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
