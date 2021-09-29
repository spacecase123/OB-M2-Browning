using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace OpenBullet.Pages.Main.Tools
{
    /// <summary>
    /// Interaction logic for TessDataDownloads.xaml
    /// </summary>
    public partial class TessDataDownloads : Page
    {
        //TessDataDownloads c = new TessDataDownloads();

        private WebClient loadSite = new WebClient();
        public string url = @"https://github.com/tesseract-ocr/tessdata/tree/3.04.00";
        public string siteResponse = null;
        public string language = null;
        private Regex lang = new Regex("title=\"(.*).traineddata\" id=\"");

        public TessDataDownloads()
        {
            InitializeComponent();
        }

        private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            LogsText.Clear();

            int i = 0;
            LogsText.Text += "Downloading tessdata files..." + Environment.NewLine;

            foreach (string item in DownloadList.Items)
            {
                Directory.CreateDirectory(@".\tessdata");
                i++;
                LogsText.Text += String.Format("{0}/{1} | Downloading: {2}..", i, DownloadList.Items.Count, item);// + Environment.NewLine;
                System.Windows.Forms.Application.DoEvents();
                try
                {
                    DownloadLanguage(i, item.ToString());
                }
                catch {
                    System.Windows.Forms.MessageBox.Show("Please create a folder named 'tessdata' in your Openbullet directory", "FOLDER MISSING", MessageBoxButtons.OK);
                }
            }

            //foreach (Match line in lang.Matches(siteResponse))
            //{
            //    //LogsText.Text += line.Groups[1].Value + Environment.NewLine;
            //    LogsText.Text += String.Format("{0}/{1} | Downloading: {2}", i, lang.Matches(siteResponse).Count, line.Groups[1].Value) + Environment.NewLine;
            //    DownloadLanguage(line.Groups[1].Value);
            //    i++;
            //}
            LogsText.Text += "Your chosen languages have been downloaded";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            siteResponse = loadSite.DownloadString(url);
            foreach (Match line in lang.Matches(siteResponse))
            {
                LanguageList.Items.Add(line.Groups[1].Value);
            }
        }

        public void DownloadLanguage(int i, string language)
        {
            //LogsText.Text += String.Format("{0}/{1} | Downloading: {2}", i, DownloadList.Items.Count, language) + Environment.NewLine;

            language += ".traineddata";
            loadSite.DownloadFile("https://github.com/tesseract-ocr/tessdata/raw/3.04.00/" + language, AppDomain.CurrentDomain.BaseDirectory + "/tessdata/" + language);
            LogsText.Text += "\t\t\t\t| Finished!" + Environment.NewLine;
            System.Windows.Forms.Application.DoEvents();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DownloadList.Items.Add(LanguageList.SelectedItem);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DownloadList.Items.Remove(DownloadList.SelectedItem);
        }

        private void Button_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("This button will add ALL languages to the download list", "ALERT", MessageBoxButtons.OKCancel) == DialogResult.OK)
                foreach (string item in LanguageList.Items)
                    DownloadList.Items.Add(item);
        }

        private void Button_MouseRightButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("This button will remove ALL languages to the download list", "ALERT", MessageBoxButtons.OKCancel) == DialogResult.OK)
                DownloadList.Items.Clear();
        }
    }
}