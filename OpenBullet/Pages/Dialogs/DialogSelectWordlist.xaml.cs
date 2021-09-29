using OpenBullet.ViewModels;
using RuriLib.Models;
using RuriLib.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogSelectWordlist.xaml
    /// </summary>
    public partial class DialogSelectWordlist : Page
    {
        private ObservableCollection<Wordlist> WordList;
        WordlistManagerViewModel vm = new WordlistManagerViewModel();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        object Caller { get; set; }
        System.Windows.Forms.CheckBox checkBox = new System.Windows.Forms.CheckBox();

        public DialogSelectWordlist(object caller)
        {
            InitializeComponent();
            Caller = caller;
            DataContext = Globals.mainWindow.WordlistManagerPage.DataContext; //vm;

            vm.RefreshList();
        }

        
        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            if (Caller.GetType() == typeof(Runner))
            {
                ((Runner)Caller).SetWordlist((Wordlist)wordlistsListView.SelectedItem);
            }
            ((MainDialog)Parent).Close();
        }

        
        private void listViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                wordlistsListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            wordlistsListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            selectButton_Click(this, null);
        }

        private void ListViewItem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                selectButton_Click(this, null);
        }

        private void importWordlistButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Wordlist file | *.txt";
            ofd.FilterIndex = 1;
            ofd.ShowDialog();
            try
            {
                // Build the wordlist object
                var wordlist = new Wordlist(Path.GetFileNameWithoutExtension(ofd.FileName), ofd.FileName, Globals.environment.WordlistTypes.First().Name, "");

                // Get the first line
                var first = File.ReadLines(wordlist.Path).First();

                // Set the correct wordlist type
                wordlist.Type = Globals.environment.RecognizeWordlistType(first);

                // Add the wordlist to the runner
                ((Runner)Caller).SetWordlist(wordlist);

                ((MainDialog)Parent).Close();
            }
            catch { }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            WordList = new ObservableCollection<Wordlist>(vm.WordlistList.Where(c => c.Name.ToLower().Contains(searchBox.Text.ToLower())));
            wordlistsListView.ItemsSource = WordList;
        }

        private void wordlistsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
