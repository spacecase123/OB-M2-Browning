using System.Diagnostics;
using System.Windows.Controls;

namespace OpenBullet.Views.Main
{
    /// <summary>
    /// Logica di interazione per About.xaml
    /// </summary>
    public partial class About : Page
    {
        public About()
        {
            InitializeComponent();
        }

        private void repoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = Process.Start("https://github.com/openbullet/openbullet");
        }

        private void docuButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = Process.Start("https://openbullet.github.io/");
        }

        private void telegramButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = Process.Start("https://t.me/AreA51Crew2021");
        }

        private void discordButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = Process.Start("https://discord.gg/eDa7ktyFFB");
        }

        private void forumButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = Process.Start("https://lacantinadeibin.altervista.org/forum-del-la-cantina-dei-bin/");
        }

        private void siteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = Process.Start("https://lacantinadeibin.altervista.org/");
        }

        private void softwareButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = Process.Start("https://mega.nz/#P!AgAauiQNhAoMC47PWn2NuLZT9QCcQG0ii_SWvTgQqbQPtQQFLLtMRJpdZVTHZQUZu6WFMRceXsaSCmgGUZKsz7x7FPA1n9ftKUh-CgYlvqMxJ3RGIjMECg");
        }

        private void platformButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = Process.Start("http://www.area51crew.flazio.com/");

        }

        private void donateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = Process.Start("https://bit.ly/3i1726l");
        }

        private void genButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = Process.Start("http://generatorgoldevolution.ddns.net");
        }

        private void crackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = Process.Start("https://t.me/joinchat/FRu9vjFLRd5yM5fn");
        }
    }
}
