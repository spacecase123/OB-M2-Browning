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
        public string CurrentVersion = "1.0.2".Trim();
    

        // URL for the Changelog
        public static string ChangelogURL = "https://raw.githubusercontent.com/Area51Crew/OB-M2-Browning/main/Changelog";

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
                {
                    DragMove();
                }
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
                string NewestVersion = wc.DownloadString("https://raw.githubusercontent.com/Area51Crew/OB-M2-Browning/main/Version");

                string NewVersionTrimmed = NewestVersion.Trim();
                if (CurrentVersion.Equals(NewVersionTrimmed))
                {
                    System.Windows.MessageBox.Show("Tutto Aggiornato!", "OB ONE M2 Updater");
                }
                else
                {
                    MessageBoxResult dialogResult = System.Windows.MessageBox.Show("Update Trovato! Vuoi Aggiornare?", "OB ONE M2 Updater", MessageBoxButton.YesNo);
                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        System.Windows.MessageBox.Show("Il Launcher si chiuderà e sarai indirizzato l'ultima versione.", "OB ONE M2 Updater");
                        _ = Process.Start("https://github.com/Area51Crew/OB-M2-Browning/releases/");
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