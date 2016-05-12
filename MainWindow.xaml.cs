using BooksDB.Data;
using BooksDB.Models;
using Innouvous.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BooksDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel;

        private IDataStore datastore; //Need for some local commands, how to move to VM?

        internal static readonly Properties.Settings Configs = Properties.Settings.Default; 

        public MainWindow()
        {
            try
            {
                if (String.IsNullOrEmpty(Configs.DBPath) || !File.Exists(Configs.DBPath))
                {
                    OptionsWindow window = new OptionsWindow();
                    window.ShowDialog();

                    if (String.IsNullOrEmpty(Configs.DBPath))
                        throw new Exception("Database path not set.");
                }

                LoadViewModel();

            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
                App.Current.Shutdown();
            }

            InitializeComponent();

            //var o = viewModel.ItemView;
            //NewRadioButton.IsChecked = true;

            
        }

        private void LoadViewModel()
        {
            datastore = new SQLiteDataStore();
            viewModel = new MainWindowViewModel(datastore);
            
            this.DataContext = viewModel;
            
        }
        
        #region Set Item Type
            
        private void SetUnreadButton_Click(object sender, RoutedEventArgs e)
        {
            SetType(ItemType.Unread);
        }

        private void SetReadButton_Click(object sender, RoutedEventArgs e)
        {

            SetType(ItemType.Read);
        }
        
        private void SetType(ItemType itemType)
        {
            ItemViewModel ivm = ItemsListBox.SelectedItem as ItemViewModel;
            viewModel.SetItemType(ivm, itemType);
        }

        #endregion

        #region UI Helpers
        //Cannot turn this into MVVM

        private void ItemsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenPage();
        }
        
        private void ItemsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    OpenPage();
                    break;
                case Key.U:
                    SetType(ItemType.Unread);
                    break;
                case Key.R:
                    SetType(ItemType.Read);
                    break;
                case Key.Left:
                    var s = SelectedModel();
                    if (s != null && s.Rating > 0)
                    {
                        SetRating(s, s.Rating - 1);
                    }
                    break;
                case Key.Right:
                    var s2 = SelectedModel();
                    if (s2 != null && s2.Rating < 5)
                    {
                        SetRating(s2, s2.Rating + 1);
                    }
                    break;
                
            }
            
        }

        private ItemViewModel SelectedModel()
        {
            return ItemsListBox.SelectedItem as ItemViewModel;
        }

        private void OpenPage()
        {
            ItemViewModel ivm = ItemsListBox.SelectedItem as ItemViewModel;

            if (ivm != null)
            {
                Process.Start(ivm.Item.URL);
            }

        }

        //Needed here as the entire app needs to be reloaded. 
        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new OptionsWindow();

            try
            {
                window.ShowDialog();

                if (!window.Cancelled)
                {
                    LoadViewModel();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SetRating(ItemViewModel model, int newRating)
        {
            var newItem = new Item(model.Item.Name, model.Item.URL, model.ID, model.Item.Type, DateTime.Now, newRating);
            datastore.UpdateItem(model.Item, newItem);
            model.Item = newItem;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s = (Slider)sender;

            var ivm = s.DataContext as ItemViewModel;
            if (ivm != null)
            {

                int rating;

                if (Int32.TryParse(s.Value.ToString(), out rating))
                {
                    SetRating(ivm, ivm.Rating);
                }
            }
        }

        #endregion
    }
}
