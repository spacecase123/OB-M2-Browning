using RuriLib.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per Configs.xaml
    /// </summary>
    public partial class Configs : Page
    {
        public ConfigManager ConfigManagerPage;
        public Stacker StackerPage;
        public ConfigOtherOptions OtherOptionsPage;
        public ConfigOCRSettings OCRSettingsPage;
        public ConfigViewModel CurrentConfig { get; set; }

        public Configs()
        {
            InitializeComponent();

            ConfigManagerPage = new ConfigManager();
            Globals.LogInfo(Components.ConfigManager, "Initialized Manager Page");

            menuOptionManager_MouseDown(this, null);
        }

        #region Menu Options
        private void menuOptionManager_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ConfigManagerPage;
            menuOptionSelected(menuOptionManager);
        }

        public void menuOptionStacker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(CurrentConfig != null && StackerPage != null)
            {
                Main.Content = StackerPage;
                menuOptionSelected(menuOptionStacker);
            }
            else
            {
                Globals.LogError(Components.ConfigManager, "Impossibile passare allo stacker poiché non viene caricata alcuna configurazione o la configurazione caricata non è pubblica");
            }
        }

        private void menuOptionOtherOptions_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(CurrentConfig != null)
            {
                if (OtherOptionsPage == null)
                    OtherOptionsPage = new ConfigOtherOptions();    
                
                Main.Content = OtherOptionsPage;
                menuOptionSelected(menuOptionOtherOptions);
            }
            else
            {
                Globals.LogError(Components.ConfigManager, "Impossibile passare allo stacker poiché non viene caricata alcuna configurazione");
            }
        }

        private void menuOptionOCRSettings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentConfig != null)
            {
                OCRSettingsPage = new ConfigOCRSettings();

                Main.Content = OCRSettingsPage;
                menuOptionSelected(menuOptionOCRSettings);
            }
            else
            {
                Globals.LogError(Components.ConfigManager, "Impossibile passare allo stacker poiché non viene caricata alcuna configurazione");
            }
        }

        private void menuOptionSelected(object sender)
        {
            foreach (var child in topMenu.Children)
            {
                try
                {
                    //var a = "";
                    var c = (Label)child;
                    c.Foreground = Globals.GetBrush("ForegroundMain");
                }
                catch { }
            }
            ((Label)sender).Foreground = Globals.GetBrush("ForegroundCustom");
        }
        #endregion

    }
}
