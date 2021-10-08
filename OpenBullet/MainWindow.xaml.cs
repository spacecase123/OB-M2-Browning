using OpenBullet.ViewModels;
using OpenBullet.Views.Main;
using RuriLib;
using RuriLib.ViewModels;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Diagnostics;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random rand = new Random();
        private int snowBuffer = 0;

        public RunnerManager RunnerManagerPage { get; set; }
        public Runner CurrentRunnerPage { get; set; }
        public ProxyManager ProxyManagerPage { get; set; }
        public WordlistManager WordlistManagerPage { get; set; }
        public Configs ConfigsPage { get; set; }
        public HitsDB HitsDBPage { get; set; }
        public Settings OBSettingsPage { get; set; }
        public Tools ToolsPage { get; set; }
        public About AboutPage { get; set; }
        public System.Drawing.Rectangle Bounds { get; private set; }

        private string title = $"OB ONE M2 BrowninG {Globals.obVersion}";
        private bool maximized = false;
        System.Windows.Point _startPosition;
        bool _isResizing = false;

        private BackgroundWorker ruriGuard = new BackgroundWorker();
        private BackgroundWorker ruriKiller = new BackgroundWorker();
        private BackgroundWorker ruriScout = new BackgroundWorker();

        public MainWindow()
        {
            // Clean or create log file
            File.WriteAllText(Globals.logFile, "");

            InitializeComponent();

            Title = title;
            titleLabel.Content = title;

            // Set global reference to this window
            Globals.mainWindow = this;

            // Make sure all folders are there or recreate them
            var folders = new string[] { "Captchas", "ChromeExtensions", "Configs", "DB", "Screenshots", "Settings", "Sounds", "Wordlists" };
            foreach (var folder in folders.Select(f => System.IO.Path.Combine(Directory.GetCurrentDirectory(), f)))
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }

            // Initialize Environment Settings
            try
            {
                Globals.environment = IOManager.ParseEnvironmentSettings(Globals.envFile);
            }
            catch
            {
                MessageBox.Show("Impossibile trovare / analizzare il file delle impostazioni di ambiente. Risolvi il problema e riprova.");
                Environment.Exit(0);
            }

            if (Globals.environment.WordlistTypes.Count == 0 || Globals.environment.CustomKeychains.Count == 0)
            {
                MessageBox.Show("Almeno un WordlistType e una CustomKeychain devono essere definiti nel file Impostazioni ambiente.");
                Environment.Exit(0);
            }

            // Initialize Settings
            Globals.rlSettings = new RLSettingsViewModel();
            Globals.obSettings = new OBSettingsViewModel();

            // Create / Load Settings
            if (!File.Exists(Globals.rlSettingsFile))
            {
                MessageBox.Show("File delle impostazioni RuriLib non trovato, genera uno predefinito");
                Globals.LogWarning(Components.Main, "File delle impostazioni RuriLib non trovato, genera uno predefinito");
                IOManager.SaveSettings(Globals.rlSettingsFile, Globals.rlSettings);
                Globals.LogInfo(Components.Main, $"Creato il file predefinito RuriLib Settings {Globals.rlSettingsFile}");
            }
            else
            {
                Globals.rlSettings = IOManager.LoadSettings(Globals.rlSettingsFile);
                Globals.LogInfo(Components.Main, "Caricato il file delle impostazioni RuriLib esistente");
            }

            if (!File.Exists(Globals.obSettingsFile))
            {
                MessageBox.Show("File delle impostazioni OpenBullet non trovato, genera uno predefinito");
                Globals.LogWarning(Components.Main, "File delle impostazioni OpenBullet non trovato, genera uno predefinito");
                OBIOManager.SaveSettings(Globals.obSettingsFile, Globals.obSettings);
                Globals.LogInfo(Components.Main, $"Creato il file predefinito OpenBullet Settings {Globals.obSettingsFile}");
            }
            else
            {
                Globals.obSettings = OBIOManager.LoadSettings(Globals.obSettingsFile);
                Globals.LogInfo(Components.Main, "Caricato il file delle impostazioni OpenBullet esistente");
            }

            // If there is no DB backup or if it's more than 1 day old, back up the DB
            try
            {
                if (Globals.obSettings.General.BackupDB &&
                    (!File.Exists(Globals.dataBaseBackupFile) || 
                    (File.Exists(Globals.dataBaseBackupFile) && ((DateTime.Now - File.GetCreationTime(Globals.dataBaseBackupFile)).TotalDays > 1))))
                {
                    // Check that the DB is not corrupted by accessing a random collection. If this fails, an exception will be thrown.
                    using (var db = new LiteDB.LiteDatabase(Globals.dataBaseFile))
                    {
                        var coll = db.GetCollection<RuriLib.Models.CProxy>("proxies");
                    }

                    // Delete the old file and copy over the new one
                    File.Delete(Globals.dataBaseBackupFile);
                    File.Copy(Globals.dataBaseFile, Globals.dataBaseBackupFile);
                    Globals.LogInfo(Components.Main, "DB Backup salvato");
                }
            }

            catch (Exception ex)
            {
                Globals.LogError(Components.Main, $"Non riesco a salvare il DB: {ex.Message}");
            }

            
            Topmost = Globals.obSettings.General.AlwaysOnTop;

            _ = (new SplashWindow()).ShowDialog();
            menuOptionRunner_MouseDown(this, null);

            RunnerManagerPage = new RunnerManager(Globals.obSettings.General.AutoCreateRunner);
            if (Globals.obSettings.General.AutoCreateRunner)
            {
                CurrentRunnerPage = RunnerManagerPage.vm.Runners.FirstOrDefault().Page;
            }

            Globals.LogInfo(Components.Main, "Initialized RunnerManager");
            ProxyManagerPage = new ProxyManager();
            Globals.LogInfo(Components.Main, "Initialized ProxyManager");
            WordlistManagerPage = new WordlistManager();
            Globals.LogInfo(Components.Main, "Initialized WordlistManager");
            ConfigsPage = new Configs();
            Globals.LogInfo(Components.Main, "Initialized ConfigManager");
            HitsDBPage = new HitsDB();
            Globals.LogInfo(Components.Main, "Initialized HitsDB");
            OBSettingsPage = new Settings();
            Globals.LogInfo(Components.Main, "Initialized Settings");
            ToolsPage = new Tools();
            Globals.LogInfo(Components.Main, "Initialized Tools");
            AboutPage = new About();

            menuOptionRunner_MouseDown(this, null);

            var width = Globals.obSettings.General.StartingWidth;
            var height = Globals.obSettings.General.StartingHeight;
            if (width > 800) Width = width;
            if (height > 600)
            {
                Height = height;
            }

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (Globals.obSettings.Themes.EnableSnow)
                Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10000 / Globals.obSettings.Themes.SnowAmount) };
            t.Tick += (s, ea) => Snow();
            t.Start();
        }

        private void Snow()
        {
            if (snowBuffer >= 100)
            {
                int i = 0;
                while (i < Root.Children.Count)
                {
                    // Remove first snowflake you find (oldest) before putting another one so there are max 100 snowflakes on screen
                    if (Root.Children[i].GetType() == typeof(Snowflake)) { Root.Children.RemoveAt(i); break; }
                    i++;
                }
            }

            var x = rand.Next(-500, (int)Root.ActualWidth - 100);
            var y = -100;
            var s = rand.Next(5, 15);

            Snowflake flake = new Snowflake
            {
                Width = s,
                Height = s,
                RenderTransform = new TranslateTransform
                {
                    X = x,
                    Y = y
                },
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                IsHitTestVisible = false
            };

            Grid.SetColumn(flake, 1);
            Grid.SetRow(flake, 2);
            Root.Children.Add(flake);

            var d = TimeSpan.FromSeconds(rand.Next(1, 4));

            x += rand.Next(100, 500);
            var ax = new DoubleAnimation { To = x, Duration = d };
            flake.RenderTransform.BeginAnimation(TranslateTransform.XProperty, ax);

            y = (int)Root.ActualHeight + 200;
            var ay = new DoubleAnimation { To = y, Duration = d };
            flake.RenderTransform.BeginAnimation(TranslateTransform.YProperty, ay);

            snowBuffer++;
        }

        public void ShowRunnerManager()
        {
            CurrentRunnerPage = null;
            Main.Content = RunnerManagerPage;
        }

        public void ShowRunner(Runner page)
        {
            CurrentRunnerPage = page;
            Main.Content = page;
        }

        #region Menu Options MouseDown Events
        public void menuOptionRunner_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentRunnerPage == null) Main.Content = RunnerManagerPage;
            else Main.Content = CurrentRunnerPage;
            menuOptionSelected(menuOptionRunner);
        }

        private void menuOptionProxyManager_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ProxyManagerPage;
            menuOptionSelected(menuOptionProxyManager);
        }

        private void menuOptionWordlistManager_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = WordlistManagerPage;
            menuOptionSelected(menuOptionWordlistManager);
        }

        private void menuOptionConfigCreator_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ConfigsPage;
            menuOptionSelected(menuOptionConfigCreator);
        }

        private void menuOptionHitsDatabase_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = HitsDBPage;
            menuOptionSelected(menuOptionHitsDatabase);
        }

        private void menuOptionTools_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ToolsPage;
            menuOptionSelected(menuOptionTools);
        }

        private void menuOptionSettings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = OBSettingsPage;
            menuOptionSelected(menuOptionSettings);
        }

        private void menuOptionAbout_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = AboutPage;
            menuOptionSelected(menuOptionAbout);
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
            ((Label)sender).Foreground = Globals.GetBrush("ForegroundMenuSelected");
        }
        #endregion

        private void quitPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            quitPanel.Background = new SolidColorBrush(Colors.DarkRed);
        }

        private void quitPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            quitPanel.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void quitPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CheckOnQuit())
            {
                Environment.Exit(0);
            }
        }

        private bool CheckOnQuit()
        {
            int active = RunnerManagerPage.vm.Runners.Count(r => r.Runner.Busy);
            if (!Globals.obSettings.General.DisableQuitWarning || active > 0)
            {
                Globals.LogWarning(Components.Main, "Prompting quit confirmation");

                if (active == 0)
                {
                    if (MessageBox.Show($"Sicuro di chiudere l'applicazione?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        return false;
                }
                else
                {
                    if (MessageBox.Show($"Ci sono {active} lavori in esecuzione. Sicuro di voler chiudere l'applicazione?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        return false;
                    }
                }
            }

            if (!Globals.obSettings.General.DisableNotSavedWarning && !Globals.mainWindow.ConfigsPage.ConfigManagerPage.CheckSaved())
            {
                Globals.LogWarning(Components.Main, "Config not saved, prompting quit confirmation");
                if (MessageBox.Show("La configurazione in Stacker non è stata salvata.\nSei sicuro di voler uscire?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return false;
                }
            }
            Globals.LogInfo(Components.Main, "Quit sequence initiated");
            return true;
        }

        private void maximizePanel_MouseEnter(object sender, MouseEventArgs e)
        {
            maximizePanel.Background = new SolidColorBrush(Colors.DimGray);
        }

        private void maximizePanel_MouseLeave(object sender, MouseEventArgs e)
        {
            maximizePanel.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void maximizePanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (maximized)
            {
                Rect workArea = SystemParameters.WorkArea;
                this.Width = Globals.obSettings.General.StartingWidth;
                this.Height = Globals.obSettings.General.StartingHeight;
                Left = (workArea.Width - this.Width) / 2 + workArea.Left;
                Top = (workArea.Height - this.Height) / 2 + workArea.Top;
                maximized = false;
                WindowState = WindowState.Normal;
            }
            else
            {
                this.Width = SystemParameters.WorkArea.Width;
                this.Height = SystemParameters.WorkArea.Height;
                Left = 0;
                Top = 0;
                maximized = true;
                WindowState = WindowState.Normal;
            }
        }

        private void minimizePanel_MouseEnter(object sender, MouseEventArgs e)
        {
            minimizePanel.Background = new SolidColorBrush(Colors.DimGray);
        }

        private void minimizePanel_MouseLeave(object sender, MouseEventArgs e)
        {
            minimizePanel.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void minimizePanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try { WindowState = WindowState.Minimized; } catch { }
        }

        private void titleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {

                if (maximized)
                {
                    Rect workArea = SystemParameters.WorkArea;
                    this.Width = Globals.obSettings.General.StartingWidth;
                    this.Height = Globals.obSettings.General.StartingHeight;
                    Left = (workArea.Width - this.Width) / 2 + workArea.Left;
                    Top = (workArea.Height - this.Height) / 2 + workArea.Top;
                    maximized = false;
                    WindowState = WindowState.Normal;
                }
                else
                {
                    this.Width = SystemParameters.WorkArea.Width;
                    this.Height = SystemParameters.WorkArea.Height;
                    Left = 0;
                    Top = 0;
                    maximized = true;
                    WindowState = WindowState.Normal;
                }
            }
        }

        private void dragPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void gripImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Capture(gripImage))
            {
                _isResizing = true;
                _startPosition = Mouse.GetPosition(this);
            }
        }

        private void gripImage_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                System.Windows.Point currentPosition = Mouse.GetPosition(this);
                double diffX = currentPosition.X - _startPosition.X;
                double diffY = currentPosition.Y - _startPosition.Y;
                Width = Width + diffX;
                Height = Height + diffY;
                _startPosition = currentPosition;
            }
        }

        private void gripImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing == true)
            {
                _isResizing = false;
                Mouse.Capture(null);
            }
        }

        private void screenshotImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var bitmap = CopyScreen((int)Width, (int)Height, (int)Top, (int)Left);
            Clipboard.SetImage(bitmap);
            GetBitmap(bitmap).Save("screenshot.jpg", ImageFormat.Jpeg);
            Globals.LogInfo(Components.Main, "screenshot Acquisito");
        }

        private static BitmapSource CopyScreen(int width, int height, int top, int left)
        {
            using (var screenBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen(left, top, 0, 0, screenBmp.Size);
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        private static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        private void logImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Globals.logWindow == null)
            {
                Globals.logWindow = new LogWindow();
                Globals.logWindow.Show();
            }
            else
            {
                Globals.logWindow.Show();
            }
        }

        public void SetStyle()
        {
            try
            {
                var brush = Globals.GetBrush("BackgroundMain");

                if (!Globals.obSettings.Themes.UseImage)
                {
                    Background = brush;
                    Main.Background = brush;
                }
                else
                {
                    // BACKGROUND
                    if (File.Exists(Globals.obSettings.Themes.BackgroundImage))
                    {
                        var bbrush = new ImageBrush(new BitmapImage(new Uri(Globals.obSettings.Themes.BackgroundImage)));
                        bbrush.Opacity = (double)((double)Globals.obSettings.Themes.BackgroundImageOpacity / (double)100);
                        Background = bbrush;
                    }
                    else
                    {
                        Background = brush;
                    }

                    // LOGO
                    if (File.Exists(Globals.obSettings.Themes.BackgroundLogo))
                    {
                        var lbrush = new ImageBrush(new BitmapImage(new Uri(Globals.obSettings.Themes.BackgroundLogo)));
                        lbrush.AlignmentX = AlignmentX.Center;
                        lbrush.AlignmentY = AlignmentY.Center;
                        lbrush.Stretch = Stretch.None;
                        lbrush.Opacity = (double)((double)Globals.obSettings.Themes.BackgroundImageOpacity / (double)100);
                        Main.Background = lbrush;
                    }
                    else
                    {
                        Main.Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/"
                            + Assembly.GetExecutingAssembly().GetName().Name
                            + ";component/"
                            + "Images/Themes/empty.png", UriKind.Absolute)));
                    }
                }
            }
            catch { }
        }


        private void OpenGuide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _ = System.Diagnostics.Process.Start("https://lacantinadeibin.altervista.org/wp-content/uploads/2021/09/LOLI-CODE-DOC.pdf");
        }
    }

}
