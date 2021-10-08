using Extreme.Net;
using LiteDB;
using RuriLib;
using RuriLib.Models;
using RuriLib.Runner;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per Runner.xaml
    /// </summary>
    public partial class Runner : Page
    {
        public RunnerViewModel vm = new RunnerViewModel(Globals.environment, Globals.rlSettings, Globals.random);
        public SoundPlayer hitPlayer;
        public SoundPlayer reloadPlayer;

        public bool validDisplayLock = false;
        public bool soundLock = false;
        public bool noProxiesWarningSent = false;

        public Runner()
        {

            DataContext = vm;

            InitializeComponent();


            vm.MessageArrived += LogRunnerData;
            vm.WorkerStatusChanged += LogWorkerStatus;
            vm.WorkerStatusChanged += ProcessStatusChange;
            vm.FoundHit += PlayHitSound;
            vm.FoundHit += RegisterHit;
            vm.ReloadProxies += PlayReloadSound;
            vm.ReloadProxies += LoadProxiesFromManager;
            vm.DispatchAction += ExecuteAction;
            vm.SaveProgress += SaveProgressToDB;
            vm.AskCustomInputs += InitCustomInputs;

            if (Globals.obSettings.General.ChangeRunnerInterface)
            {
                Globals.LogInfo(Components.About, "Changed the Runner interface");
                Grid.SetColumn(rightGrid, 0);
                Grid.SetRow(rightGrid, 1);
                Grid.SetColumn(bottomLeftGrid, 2);
                Grid.SetRow(bottomLeftGrid, 0);
            }
            
            logBox.AppendText("", Colors.White);
            logBox.AppendText("Runner initialized succesfully!"+ Environment.NewLine, Globals.GetColor("ForegroundMain"));
        }

        #region Events
        private void LogRunnerData(IRunnerMessaging sender, LogLevel level, string message, bool prompt, int timeout)
        {
            Globals.Log(Components.Runner, level, message, prompt, timeout);
        }

        private void LogWorkerStatus(IRunnerMessaging sender)
        {
            var runner = sender as RunnerViewModel;
            switch (runner.Master.Status)
            {
                case WorkerStatus.Running:                    
                    logBox.AppendText($"Avvio esecuzione di Config {runner.ConfigName} con Wordlist {runner.WordlistName} in data {DateTime.Now}.{Environment.NewLine}",
                        Globals.GetColor("ForegroundGood"));
                    break;

                case WorkerStatus.Stopping:
                    logBox.AppendText($"Richiesta interruzione inviata alle {DateTime.Now}.{Environment.NewLine}",
                        Globals.GetColor("ForegroundCustom"));
                    break;

                case WorkerStatus.Idle:
                    logBox.AppendText($"Runner terminato alle {DateTime.Now}.{Environment.NewLine}",
                        Globals.GetColor("ForegroundBad"));
                    break;
            }
        }

        private void ExecuteAction(IRunnerMessaging sender, Action action)
        {
            App.Current.Dispatcher.Invoke(action);
        }

        private void RegisterHit(IRunnerMessaging sender, Hit hit)
        {
            Globals.LogInfo(Components.Runner, $"Aggiungendo {hit.Type} hit " + hit.Data + " al DB");
            try
            {
                using (var db = new LiteDatabase(Globals.dataBaseFile))
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Globals.mainWindow.HitsDBPage.vm.HitsList.Add(hit);
                        Globals.mainWindow.HitsDBPage.AddConfigToFilter(vm.ConfigName);
                    }));
                    db.GetCollection<Hit>("hits").Insert(hit);
                }
            }
            catch (Exception ex) { Globals.LogError(Components.Runner, $"Aggiunta fallita {hit.Type} hit " + hit.Data + $" al DB - {ex.Message}"); }
        }

        private void PlayHitSound(IRunnerMessaging sender, Hit hit)
        {
            if (Globals.obSettings.Sounds.EnableSounds && hit.Type == "SUCCESS")
            {
                try
                {
                    while (soundLock) { Thread.Sleep(10); }
                    soundLock = true;
                    hitPlayer.Play();
                    soundLock = false;
                }
                catch { }
                finally { soundLock = false; }
            }
        }

        private void PlayReloadSound(IRunnerMessaging sender)
        {
            if (Globals.obSettings.Sounds.EnableSounds)
            {
                try
                {
                    while (soundLock) { Thread.Sleep(10); }
                    soundLock = true;
                    reloadPlayer.Play();
                    soundLock = false;
                }
                catch { }
                finally { soundLock = false; }
            }
        }

        private void LoadProxiesFromManager(IRunnerMessaging sender)
        {
            var proxies = Globals.mainWindow.ProxyManagerPage.vm.ProxyList.ToList();
            List<CProxy> toAdd;
            if (vm.Config.Settings.OnlySocks) toAdd = proxies.Where(x => x.Type != ProxyType.Http).ToList();
            else if (vm.Config.Settings.OnlySsl) toAdd = proxies.Where(x => x.Type == ProxyType.Http).ToList();
            else toAdd = proxies;

            vm.ProxyPool = new ProxyPool(toAdd, Globals.rlSettings.Proxies.ShuffleOnStart);
        }

        private void ProcessStatusChange(IRunnerMessaging sender)
        {
            switch (vm.Master.Status)
            {
                case WorkerStatus.Idle:
                    SaveRecord();
                    startButton.Content = "START";
                    break;
            }
        }

        private void SaveProgressToDB(IRunnerMessaging sender)
        {
            SaveRecord();
        }

        private void InitCustomInputs(IRunnerMessaging sender)
        {
            // Ask for Custom User Input
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                vm.CustomInputs = new List<KeyValuePair<string, string>>();
                foreach (var input in vm.Config.Settings.CustomInputs)
                {
                    Globals.LogInfo(Components.Runner, $"Richiesta di input {input.Description}");
                    (new MainDialog(new DialogCustomInput(this, input.VariableName, input.Description), "Custom Input")).ShowDialog();
                }
                vm.CustomInputsInitialized = true;
            }));
        }
        #endregion

        #region Start
        public void OnStartRunner(object sender, EventArgs e)
        {
            startButton_Click(this, new RoutedEventArgs());
        }

        public void startButton_Click(object sender, RoutedEventArgs e)
        {            
            switch (vm.Master.Status)
            {
                case WorkerStatus.Idle:
                    SetupSoundPlayers();
                    ThreadPool.SetMinThreads(vm.BotsNumber * 2 + 1, vm.BotsNumber * 2 + 1);
                    ServicePointManager.DefaultConnectionLimit = 10000; // This sets the default connection limit for requests on the whole application
                    startButton.Content = "STOP";
                    vm.Start();
                    break;

                case WorkerStatus.Running:
                    vm.Stop();                    
                    startButton.Content = "HARD ABORT";
                    break;

                case WorkerStatus.Stopping:
                    vm.ForceStop();
                    startButton.Content = "START";
                    SaveRecord();
                    break;
            }
        }
        #endregion

        #region Private Setup Methods
        private void SetupSoundPlayers()
        {
            var hitSound = $"Sounds/{Globals.obSettings.Sounds.OnHitSound}";
            var reloadSound = $"Sounds/{Globals.obSettings.Sounds.OnReloadSound}";
            if (File.Exists(hitSound))
            {
                hitPlayer = new SoundPlayer(hitSound);
            }

            if (File.Exists(reloadSound))
            {
                reloadPlayer = new SoundPlayer(reloadSound);
            }

            Globals.LogInfo(Components.Runner, "Configurare i lettori audio");
        }
        #endregion

        #region External Set Methods
        public void SetConfig(Config config)
        {
            vm.SetConfig(config, Globals.obSettings.General.RecommendedBots);
            RetrieveRecord();
        }

        public void SetWordlist(Wordlist wordlist)
        {
            vm.SetWordlist(wordlist);
            RetrieveRecord();
        }
        #endregion

        #region UI Elements
        private void botsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            vm.BotsNumber = (int)e.NewValue;
        }

        private void startingPointSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            vm.StartingPoint = (int)e.NewValue;
        }

        private void selectConfigButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogSelectConfig(this), "Seleziona Config")).ShowDialog();
        }

        private void selectWordlistButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogSelectWordlist(this), "Seleziona Wordlist")).ShowDialog();
        }

        private void hitsFilterButton_Click(object sender, RoutedEventArgs e)
        {
            vm.ResultsFilter = BotStatus.SUCCESS;
            Globals.LogInfo(Components.Runner, $"Filtro valido modificato in {vm.ResultsFilter}");
            RefreshListView();
        }

        private void customFilterButton_Click(object sender, RoutedEventArgs e)
        {
            vm.ResultsFilter = BotStatus.CUSTOM;
            Globals.LogInfo(Components.Runner, $"Filtro valido modificato in {vm.ResultsFilter}");
            RefreshListView();
        }

        private void toCheckFilterButton_Click(object sender, RoutedEventArgs e)
        {
            vm.ResultsFilter = BotStatus.NONE;
            Globals.LogInfo(Components.Runner, $"Filtro valido modificato in {vm.ResultsFilter}");
            RefreshListView();
        }

        private void RefreshListView()
        {
            validListView.ItemsSource = vm.EmptyList;
            switch (vm.ResultsFilter)
            {
                case BotStatus.SUCCESS:
                    validListView.ItemsSource = vm.HitsList;
                    break;

                case BotStatus.CUSTOM:
                    validListView.ItemsSource = vm.CustomList;
                    break;

                case BotStatus.NONE:
                    validListView.ItemsSource = vm.ToCheckList;
                    break;
            }
        }

        private ListView GetCurrentListView()
        {
            return validListView;
        }

        private void showManagerButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.mainWindow.ShowRunnerManager();
        }
        #endregion

        #region Records
        private void RetrieveRecord()
        {
            if (vm.Wordlist == null || vm.Config == null)
                return;

            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                var record = db.GetCollection<Record>("records").FindOne(r => r.ConfigName == vm.ConfigName && r.WordlistLocation == vm.Wordlist.Path);
                if (record != null)
                {
                    vm.StartingPoint = record.Checkpoint;
                    Globals.LogInfo(Components.Runner, "Record recuperato dal DB per wordlist '" + vm.Wordlist.Name + "' and config '" + vm.ConfigName + $"' (set {vm.StartingPoint} as starting point)");
                }
                else
                {
                    vm.StartingPoint = 1;
                    Globals.LogInfo(Components.Runner, "Nessun record trovato nel DB per wordlist '" + vm.Wordlist.Name + "' and config '" + vm.ConfigName + "'");
                }
            }
        }

        private void SaveRecord()
        {
            if (vm.Config == null || vm.Wordlist == null) return;
            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                var coll = db.GetCollection<Record>("records");
                var record = new Record(vm.ConfigName, vm.Wordlist.Path, vm.TestedCount + vm.StartingPoint);

                coll.Delete(r => r.ConfigName == vm.ConfigName && r.WordlistLocation == vm.Wordlist.Path);
                coll.Insert(record);
            }
        }
        #endregion

        #region Rightclick options
        private void ListViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void showHTML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText("source.html", ((ValidData)GetCurrentListView().SelectedItem).Source);
                System.Diagnostics.Process.Start("source.html");
                Globals.LogInfo(Components.Runner, "Salvato l'html nel source.html e aperto con il visualizzatore predefinito");
            }
            catch (Exception ex) { Globals.LogError(Components.Runner, $"Impossibile visualizzare il codice HTML - {ex.Message}", true); }
        }

        private void showLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (new MainDialog(new DialogShowLog(((ValidData)GetCurrentListView().SelectedItem).Log), "Complete Log")).Show();
                Globals.LogInfo(Components.Runner, "Aperto il registro per il colpo " + ((ValidData)GetCurrentListView().SelectedItem).Data);
            }
            catch { MessageBox.Show("FALLITO"); }
        }

        private void copySelectedData_Click(object sender, RoutedEventArgs e)
        {
            var clipboardText = "";
            try
            {
                foreach (ValidData selected in GetCurrentListView().SelectedItems)
                    clipboardText += selected.Data + Environment.NewLine;

                Globals.LogInfo(Components.Runner, $"Copiato {GetCurrentListView().SelectedItems.Count} data");
                Clipboard.SetText(clipboardText);
            }
            catch (Exception ex) { Globals.LogError(Components.Runner, $"Eccezione durante la copia dei dati - {ex.Message}"); }
        }

        private void copySelectedCapture_Click(object sender, RoutedEventArgs e)
        {
            var clipboardText = "";
            try
            {
                foreach (ValidData selected in GetCurrentListView().SelectedItems)
                    clipboardText += selected.Data + " | " + selected.CapturedData + Environment.NewLine;

                Globals.LogInfo(Components.Runner, $"Copiato {GetCurrentListView().SelectedItems.Count} data");
                Clipboard.SetText(clipboardText);
            }
            catch (Exception ex) { Globals.LogError(Components.Runner, $"Eccezione durante la copia dei dati - {ex.Message}"); }
        }

        private void selectAll_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentListView().SelectAll();
        }

        private void copySelectedProxy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(((ValidData)GetCurrentListView().SelectedItem).Proxy);
                Globals.LogInfo(Components.Runner, "Proxy copiati " + ((ValidData)GetCurrentListView().SelectedItem).Proxy);
            }
            catch (Exception ex) { Globals.LogError(Components.Runner, $"Impossibile copiare il proxy per hit selezionato - {ex.Message}"); }
        }

        private void sendToDebugger_Click(object sender, RoutedEventArgs e)
        {
            try // Try because StackerPage can be null if not initialized yet
            {
                var stacker = Globals.mainWindow.ConfigsPage.StackerPage.vm;
                var current = GetCurrentListView().SelectedItem as ValidData;

                stacker.TestData = current.Data;

                stacker.TestProxy = current.Proxy;
                stacker.ProxyType = current.ProxyType;
                
                Globals.LogInfo(Components.Runner, $"Invia al debugger");
            }
            catch (Exception ex) { Globals.LogError(Components.Runner, $"Impossibile inviare dati e proxy al debugger - {ex.Message}"); }
        }
        #endregion

        private void botsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    #region RTB Extensions
    public static class RichTextBoxExtensions
    {

        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
        }

        public static string[] Lines(this RichTextBox box)
        {
            var textRange = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);
            return textRange.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string GetText(this RichTextBox box)
        {
            var textRange = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);
            return textRange.Text;
        }

        public static string GetTextFromLines(this RichTextBox box)
        {
            return box.Lines().Aggregate((current, next) => current + next);
        }

        public static string Select(this RichTextBox rtb, int offset, int length, Color color)
        {
            // Get text selection:
            TextSelection textRange = rtb.Selection;

            // Get text starting point:
            TextPointer start = rtb.Document.ContentStart;

            // Get begin and end requested:
            TextPointer startPos = GetTextPointAt(start, offset);
            TextPointer endPos = GetTextPointAt(start, offset + length);

            // New selection of text:
            textRange.Select(startPos, endPos);

            // Apply property to the selection:
            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(color));

            // Return selection text:
            return rtb.Selection.Text;
        }

        public static TextPointer GetTextPointAt(TextPointer from, int pos)
        {
            TextPointer ret = from;
            int i = 0;

            while ((i < pos) && (ret != null))
            {
                if ((ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text) || (ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.None))
                    i++;

                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    return ret;

                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
            }

            return ret;
        }
    }
    #endregion
}
