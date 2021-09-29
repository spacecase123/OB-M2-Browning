using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace OpenBullet.Pages.Main.Tools
{
    /// <summary>
    /// Interaction logic for ComboSuite.xaml
    /// </summary>
    public partial class ComboSuite : Page
    {
        public ComboSuite()
        {
            InitializeComponent();
            FileName = "";
        }

        public static string FileName;

        private void LoadFromManagerButton_Click(object sender, RoutedEventArgs e)
        {
            new OpenFileDialog();
        }

        private void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Opens the File Prompt
            OpenFileDialog Wordlist = new OpenFileDialog();
            if (Wordlist.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string strfilename = Wordlist.FileName;
                FileName = Wordlist.FileName;
                PathName.Text = FileName;
                int LineCount = File.ReadAllLines(FileName).Length;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (FileName == "")
            {
                System.Windows.MessageBox.Show("Please Choose a file first!");
            }
            else
            {
                // Checks the List for Duplicates.
                int total = 0;
                var sr = new StreamReader(File.OpenRead(FileName));
                // Creates the File and writes Contents to it as it checks.
                var sw = new StreamWriter(File.OpenWrite(Globals.Blank + "DeDuped.txt"));
                var lines = new HashSet<int>();
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    int hc = line.GetHashCode();
                    if (lines.Contains(hc))

                        continue;
                    total = total + 1;
                    lines.Add(hc);
                    sw.WriteLine(line);
                }
                sw.Flush();
                sw.Close();
                sr.Close();
                int LineCount = File.ReadAllLines(FileName).Length;
                LineCount = LineCount - total;
                DupesRemoved.Text = ("Duplicates Removed: " + LineCount.ToString());
                try { System.Windows.MessageBox.Show("Saved File DeDuped.txt to OpenBullet Root Folder!", "OpenBullet Duplicate Remover"); }
                catch { };
            }
        }

        // File Splitter Code
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (FileName == "")
            {
                System.Windows.MessageBox.Show("Please Choose a file first!");
            }
            else

            if (SplitCount.Text == "0")
            {
                System.Windows.MessageBox.Show("Please use something higher than 0", "Just saved your pc");
            }
            else
            {
                var reader = File.OpenText(FileName);
                string outFileName = FileName + "{0}.txt";
                //var SaveFolder = Directory.CreateDirectory(outFileName + "Splits");
                int outFileNumber = 1;
                int MAX_LINES = Convert.ToInt16(SplitCount.Text.Trim());
                //var SplitFolder = SaveFolder.FullName;
                while (!reader.EndOfStream)
                {
                    var writer = File.CreateText(string.Format(outFileName, outFileNumber++));
                    for (int idx = 0; idx < MAX_LINES; idx++)
                    {
                        writer.WriteLine(reader.ReadLine());
                        if (reader.EndOfStream) break;
                    }
                    writer.Close();
                }
                reader.Close();
                System.Windows.MessageBox.Show("Save Files Next to original Folder", "OpenBullet Splitter");
            }
        }

        //Code that reads and changes the Seprator for a list
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (FileName == "")
            {
                System.Windows.MessageBox.Show("Please Select a File First!");
            }
            else
            {
                StreamReader reader = new StreamReader(FileName);
                string content = reader.ReadToEnd();
                reader.Close();

                content = Regex.Replace(content, OriginalSep.Text.Trim(), NewSep.Text.Trim());

                StreamWriter writer = new StreamWriter(Globals.Blank + FileName + "SepChange.txt");
                writer.Write(content);
                writer.Close();
                try { System.Windows.MessageBox.Show("Saved File by Original File!", "OpenBullet Sep Changer"); }
                catch { };
            }
        }

        private void Merge_Lists_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}