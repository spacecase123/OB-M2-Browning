﻿using RuriLib;
using RuriLib.Functions.Conversions;
using System;
using System.Windows.Controls;
using RuriLib.Functions.Conditions;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per Pagexaml
    /// </summary>
    public partial class PageBlockUtility : Page
    {
        BlockUtility block;

        public PageBlockUtility(BlockUtility block)
        {
            InitializeComponent();
            this.block = block;
            DataContext = this.block;

            foreach (var g in Enum.GetNames(typeof(UtilityGroup)))
                groupCombobox.Items.Add(g);

            groupCombobox.SelectedIndex = (int)block.Group;

            foreach (var a in Enum.GetNames(typeof(ListAction)))
                listActionCombobox.Items.Add(a);

            listActionCombobox.SelectedIndex = (int)block.ListAction;

            foreach (var a in Enum.GetNames(typeof(VarAction)))
                varActionCombobox.Items.Add(a);

            varActionCombobox.SelectedIndex = (int)block.VarAction;

            foreach (var c in Enum.GetNames(typeof(Encoding)))
                conversionFromCombobox.Items.Add(c);

            conversionFromCombobox.SelectedIndex = (int)block.ConversionFrom;

            foreach (var c in Enum.GetNames(typeof(Encoding)))
                conversionToCombobox.Items.Add(c);

            conversionToCombobox.SelectedIndex = (int)block.ConversionTo;

            foreach (var a in Enum.GetNames(typeof(FileAction)))
                fileActionCombobox.Items.Add(a);

            fileActionCombobox.SelectedIndex = (int)block.FileAction;

            foreach (var a in Enum.GetNames(typeof(FolderAction)))
                folderActionCombobox.Items.Add(a);

            folderActionCombobox.SelectedIndex = (int)block.FolderAction;


        }

        private void groupCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            block.Group = (UtilityGroup)((ComboBox)e.OriginalSource).SelectedIndex;

            switch (block.Group)
            {
                default:
                    groupTabControl.SelectedIndex = 0;
                    break;

                case UtilityGroup.List:
                    groupTabControl.SelectedIndex = 1;
                    break;

                case UtilityGroup.Variable:
                    groupTabControl.SelectedIndex = 2;
                    break;

                case UtilityGroup.Conversion:
                    groupTabControl.SelectedIndex = 3;
                    break;

                case UtilityGroup.File:
                    groupTabControl.SelectedIndex = 4;
                    break;
               

                case UtilityGroup.Telegram:
                    groupTabControl.SelectedIndex = 6;
                    break;

                case UtilityGroup.Folder:
                    groupTabControl.SelectedIndex = 5;
                    break;
            }
        }

        private void listActionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            block.ListAction = (ListAction)((ComboBox)e.OriginalSource).SelectedIndex;

            switch (block.ListAction)
            {
                default:
                    listActionTabControl.SelectedIndex = 0;
                    break;

                case ListAction.Join:
                    listActionTabControl.SelectedIndex = 1;
                    break;

                case ListAction.Sort:
                    listActionTabControl.SelectedIndex = 2;
                    break;

                case ListAction.Zip:
                    listActionTabControl.SelectedIndex = 3;
                    break;

                case ListAction.Map:
                    listActionTabControl.SelectedIndex = 3;
                    break;

                case ListAction.Add:
                    listActionTabControl.SelectedIndex = 4;
                    break;

                case ListAction.Remove:
                    listActionTabControl.SelectedIndex = 5;
                    break;
            }
        }

        private void conversionFromCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            block.ConversionFrom = (Encoding)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void conversionToCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            block.ConversionTo = (Encoding)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void fileActionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            block.FileAction = (FileAction)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void folderActionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            block.FolderAction = (int)(FolderAction)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void varActionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            block.VarAction = (VarAction)((ComboBox)e.OriginalSource).SelectedIndex;

            switch (block.VarAction)
            {
                default:
                    varActionTabControl.SelectedIndex = 0;
                    break;

                case VarAction.Split:
                    varActionTabControl.SelectedIndex = 1;
                    break;
            }
        }

        private void TelegramActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null == block)
            {
                return;
            }

            block.TelegramAction = (TelegramAction)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void messageRTB_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            block.SetMessages(messageRTB.Lines());
        }

    }
}
