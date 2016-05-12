using BooksDB.Data;
using BooksDB.Models;
using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Windows;
using System.Windows.Input;

namespace BooksDB
{
    public enum StateEnum
    {
        Cancelled,
        Saved,
        Deleted
    }
    
    internal class EditItemWindowViewModel : ViewModel
    {
        private EditItemWindow window;
        private ItemViewModel item;
        private IDataStore db;

        public Visibility DeleteVisibilty { get; private set; }

        public EditItemWindowViewModel(ItemViewModel item, IDataStore db, EditItemWindow window)
        {
            this.window = window;
            this.db = db;
            State = StateEnum.Cancelled;

            SetItem(item);

        }

        public string Title { get; private set; }

        private string seriesName;
        public string SeriesName
        {
            get
            {
                return seriesName;
            }
            set
            {
                seriesName = value;
                RaisePropertyChanged();
            }
        }

        
        public StateEnum State { get; private set; }

        private Item? updatedItem = null;

        public Item? GetItem()
        {
            if (updatedItem == null)
                return updatedItem;
            else
            {
                Item i = new Item(SeriesName, URL, null, ItemType.Unread);
                return i;
            }
        }

        private string url;
        public string URL
        {
            get
            {
                return url;
            }
            set
            {
                if (value.Contains("amazon.com"))
                {
                    url = CleanAmazon(value);
                }
                else
                    url = value;

                RaisePropertyChanged();
            }
        }

        private string CleanAmazon(string url)
        {
            url = Strip(url, "/ref");

            url = Strip(url, "?");

            return url;    
        }

        private string Strip(string url, string match)
        {
            int end = url.IndexOf(match);
            if (end > 0)
                url = url.Substring(0, end);

            return url;
        }

        public string OKLabel { get; private set; }

        public ICommand CancelCommand
        {
            get
            {
                return new CommandHelper(() => window.Close());
            }
        }
        
        public ICommand DeleteCommand
        {
            get
            {
                return new CommandHelper(Delete);
            }
        }

        private void Delete()
        {
            if (item != null && item.ID != null)
            {
                db.DeleteItem(item.ID.Value);
                State = StateEnum.Deleted;
                window.Close();
            }
        }

        public ICommand OKCommand
        {
            get
            {
                return new CommandHelper(Save);
            }
        }

        private void Save()
        {
            try
            {
                Validate();

                Item i;
                
                if (this.item == null)
                {
                    i = new Item(SeriesName, URL, null, ItemType.Unread);
                    db.AddItem(i);
                }
                else
                {
                    i = new Item(SeriesName, URL, item.ID, item.ItemType, DateTime.Now, item.Rating);
                    db.UpdateItem(item.Item, i);
                }

                updatedItem = i;

                State = StateEnum.Saved;
                window.Close();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

        private void Validate()
        {
            //TODO: Check URL Valid

            if (string.IsNullOrEmpty(URL))
                throw new Exception("URL cannot be empty.");
            if (string.IsNullOrEmpty(SeriesName.Trim()))
                throw new Exception("Name cannot be empty.");

            var matches = db.FindByName(SeriesName, true);

            if (matches.Count > 0)
            {
                var item = matches[0];
                if (this.item == null || this.item.ID != item.ID)
                    throw new Exception("An item by the same name already exists.");                
            }

        }

        private void SetItem(ItemViewModel item)
        {
            string action = item == null ? "Add" : "Edit";
            
            this.item = item;

            if (item != null)
            {
                OKLabel = "Edit";
                this.SeriesName = item.Name;
                this.URL = item.Item.URL;
                
                DeleteVisibilty = Visibility.Visible;
            }
            else
            {
                OKLabel = "Add";
                DeleteVisibilty = Visibility.Collapsed;
            }

            Title = OKLabel + " Item";

        }
    }
}