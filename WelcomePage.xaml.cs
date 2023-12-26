using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace Concursus
{
	public partial class WelcomePage : Window
	{
		public WelcomePage()
		{
			InitializeComponent();
			Themes.UpdateForm(Themes.CURRENT_THEME, this);
			GitHubLink.NavigateUri = new System.Uri("https://github.com/MadMax1960/Concursus/blob/master/README.md");
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			// Open the GitHub link in the default browser
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
			e.Handled = true;
		}

		private void ContinueButton_Click(object sender, RoutedEventArgs e)
		{
			// Close the welcome page
			Close();
		}
	}
}
