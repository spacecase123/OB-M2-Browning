using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    ///     Logica di interazione per SplashWindow.xaml
    /// </summary>
    ///
    
    public partial class SplashWindow : Window
    {
        // Local Version Number
        public string CurrentVersion = "1.0.1".Trim();

        // URL for the Changelog
        public static string ChangelogURL = "https://gist.githubusercontent.com/Robaxman/fee5957cf409a5c441d32f85b8aab52e/raw/f6bd4b3e9a5159b287b0d267e729dcfb1dfc703e/Changelog";

        //WebClient for Changelog
        private readonly WebClient ChangelogGet = new WebClient();

        public SplashWindow()
        {
            InitializeComponent();
        }

        private void AgreeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HandleInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            }
            catch
            {
            }
        }

        private void quitImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        
        private void Update(object sender, RoutedEventArgs e)
        {
            //Update Check. Checks the Version Number from a URL and compares it to the Local Version
            WebClient wc = new WebClient();
            try
            {
                string NewestVersion = wc.DownloadString("https://gist.githubusercontent.com/Robaxman/35c7888f13d207d9120c9aa7e7a8c345/raw/453fe6228a999421a4eba2ef60441bc89343455d/VersionNumber");

                string NewVersionTrimmed = NewestVersion.Trim();
                if (CurrentVersion.Equals(NewVersionTrimmed))
                {
                    System.Windows.MessageBox.Show("Tutto Aggiornato!", "OpenBullet Updater");
                }
                else
                {
                    MessageBoxResult dialogResult = System.Windows.MessageBox.Show("Update Trovato! Vuoi Aggiornare?", "OpenBullet Updater", MessageBoxButton.YesNo);
                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        System.Windows.MessageBox.Show("Il Launcher adesso si chiuderà e veràà scaricata l'ultima versine.");
                        _ = Process.Start("https://github.com/Area51Crew/OB-M2-Browning/releases/download/V.1.0.0-Beta/OB.ONE.M2.Browning.zip");
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error Connecting to GitHub Version Number!", "Connection Error");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start("https://t.me/AreA51Crew2021");
        }

        // Loads the text from a URL.
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ChangeLogBox.Text = ChangelogGet.DownloadString(ChangelogURL);
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error Connecting to GitHub Changelog", "Connection Error");
            }
        }
    }
}