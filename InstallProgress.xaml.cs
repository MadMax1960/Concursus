using System;
using System.Collections.Generic;
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

namespace Concursus
{
	/// <summary>
	/// Interaction logic for InstallProgress.xaml
	/// </summary>
	public partial class InstallProgress : Window
	{
		public InstallProgress(Game game)
		{
			InitializeComponent();
			checkBox.IsChecked = Properties.Settings.Default.progress_close_status;
			StartInstallation(game);

			Themes.UpdateForm(Themes.CURRENT_THEME, this); // Apply the theme

		}

		public async void StartInstallation(Game game)
		{
			game.textProgress = new Progress<string>(value => {
				txtLog.Text += $"{value}\n";
				txtLog.ScrollToEnd();
			});
			await Task.Run(() => {
				game.Save();
			});
			button.IsEnabled = true;
			if ((bool)checkBox.IsChecked)
				this.Close();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
