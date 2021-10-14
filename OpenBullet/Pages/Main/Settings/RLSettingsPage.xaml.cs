using RuriLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet.Pages.Main.Settings
{
    /// <summary>
    /// Logica di interazione per RLSettingsPage.xaml
    /// </summary>
    public partial class RLSettingsPage : Page
    {
        RLSettingsGeneral GeneralPage = new RLSettingsGeneral();
        RLSettingsProxies ProxiesPage = new RLSettingsProxies();
        RLSettingsCaptchas CaptchasPage = new RLSettingsCaptchas();
        RLSettingsSelenium SeleniumPage = new RLSettingsSelenium();

        public RLSettingsPage()
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

        private void menuOptionProxies_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ProxiesPage;
            menuOptionSelected(menuOptionProxies);
        }

        private void menuOptionCaptchas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = CaptchasPage;
            menuOptionSelected(menuOptionCaptchas);
        }

        private void menuOptionSelenium_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = SeleniumPage;
            menuOptionSelected(menuOptionSelenium);
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
            IOManager.SaveSettings(Globals.rlSettingsFile, Globals.rlSettings);
            _ = MessageBox.Show("Opzioni Salvate", "OB ONE M2");
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Sei sicuro di voler ripristinare tutte le impostazioni di RuriLib?", "OB ONE M2", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Globals.rlSettings.Reset();
            }
        }
    }
}
