using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Concursus
{
    /// <summary>
    /// Interaction logic for CreateMod.xaml
    /// </summary>
    public partial class CreateMod : Window
    {

        List<string> current_mod_names = MainWindow.current_mods.Select(e => e.folder_name).ToList();
        public CreateMod()
        {
            InitializeComponent();
            Themes.UpdateForm(Themes.CURRENT_THEME, this);
            this.Title = $"Create Mod for {MainWindow.selected_game.GameName}";
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string new_mod_name = txtName.Text.Trim();
            if (new_mod_name == String.Empty)
            {
                MessageBox.Show("Mod Name cannot be empty!", "Can't create mod", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (new_mod_name.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                MessageBox.Show($"Mod Name contains illegal characters!\n\n{String.Join(',', Path.GetInvalidPathChars())}", "Can't create mod", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (current_mod_names.Contains(new_mod_name))
            {
                MessageBox.Show("A mod folder with that name already exists!", "Can't create mod", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            MainWindow.selected_game.CreateNewMod(new_mod_name, new ModConfig()
            {
                name = new_mod_name,
                author = txtAuthor.Text.Trim(),
                version = txtVersion.Text.Trim(),
                description = txtDescription.Text.Trim()
            });
            this.Close();
        }
    }
}
