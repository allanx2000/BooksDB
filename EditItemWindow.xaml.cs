using BooksDB.Data;
using BooksDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BooksDB
{
    /// <summary>
    /// Interaction logic for EditItemWindow.xaml
    /// </summary>
    public partial class EditItemWindow : Window
    {
        private readonly EditItemWindowViewModel vm;

        public EditItemWindow(IDataStore db, ItemViewModel item = null)
        {
            this.vm = new EditItemWindowViewModel(item, db, this);
            this.DataContext = vm;

            InitializeComponent();
        }

        public StateEnum State
        {
            get
            {
                return vm.State;
            }
        }

        public ItemViewModel GetNewItem()
        {
            Item? i = vm.GetItem();

            if (i.HasValue)
                return new ItemViewModel(i.Value);
            else
                return null;
        }
    }


}
