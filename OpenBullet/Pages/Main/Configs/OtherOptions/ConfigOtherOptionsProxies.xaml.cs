﻿using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per ConfigOtherOptionsProxies.xaml
    /// </summary>
    public partial class ConfigOtherOptionsProxies : Page
    {
        public ConfigOtherOptionsProxies()
        {
            InitializeComponent();
            DataContext = Globals.mainWindow.ConfigsPage.CurrentConfig.Config.Settings;
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
