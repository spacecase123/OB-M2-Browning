﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JR.Utils.GUI.Forms;

namespace OpenBullet.Pages.Main.Settings
{
    /// <summary>
    /// Logica di interazione per OBSettingsPage.xaml
    /// </summary>
    public partial class OBSettingsPage : Page
    {
        OBSettingsGeneral GeneralPage = new OBSettingsGeneral();
        OBSettingsSounds SoundsPage = new OBSettingsSounds();
        OBSettingsSources SourcesPage = new OBSettingsSources();
        OBSettingsThemes ThemesPage = new OBSettingsThemes();

        public OBSettingsPage()
        {
            InitializeComponent();
            menuOptionGeneral_MouseDown(this, null);
        }

        #region Menu Options
        private void menuOptionGeneral_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = GeneralPage;
            menuOptionSelected(menuOptionGeneral);
        }

        private void menuOptionSounds_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = SoundsPage;
            menuOptionSelected(menuOptionSounds);
        }

        private void menuOptionSources_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = SourcesPage;
            menuOptionSelected(menuOptionSources);
        }

        private void menuOptionThemes_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ThemesPage;
            menuOptionSelected(menuOptionThemes);
        }

        private void menuOptionSelected(object sender)
        {
            foreach (var child in topMenu.Children)
            {
                try
                {
                    var c = (Label)child;
                    c.Foreground = Globals.GetBrush("ForegroundMain");
                }
                catch { }
            }
            ((Label)sender).Foreground = Globals.GetBrush("ForegroundGood");
        }
#endregion

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            OBIOManager.SaveSettings(Globals.obSettingsFile, Globals.obSettings);
            _ = FlexibleMessageBox.Show("Impostazioni salvate correttamente", "OB ONE M2");
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Sei sicuro di voler ripristinare tutte le impostazioni di OpenBullet? Richiede Riavvio", "OB ONE M2", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Globals.obSettings.Reset();
            }
        }
    }
}
