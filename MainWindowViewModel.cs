using BooksDB.Data;
using BooksDB.Models;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace BooksDB
{
    public class MainWindowViewModel : ObservableClass
    {
        private ObservableCollection<ItemViewModel> items = new ObservableCollection<ItemViewModel>();
        private IDataStore datastore;
        
        public ReadOnlyCollection<string> Ratings { get; private set; }
        public ReadOnlyCollection<string> Categories { get; private set; }

        private CollectionViewSource itemView;

        public ItemViewModel SelectedItem { get; set; }

        private ItemType viewType;

        public const string AllRating = "All";
        public const string NARating = "NA";

        #region Properties

        private int? showRating;
        public string ShowRating
        {
            get
            {
                if (showRating == null)
                    return AllRating;
                else if (showRating == 0)
                    return NARating;
                else return showRating.Value.ToString();
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    value = AllRating;

                if (value == AllRating)
                    showRating = null;
                else if (value == NARating)
                    showRating = 0;
                else
                    showRating = Convert.ToInt32(value);

                RefreshItemsView();

            }
        }


        public ObservableCollection<ItemViewModel> Items
        {
            get
            {
                return items;
            }
        }

        public int ItemViewTotal
        {
            get
            {
                return itemView.View.Cast<ItemViewModel>().Count();
            }
        }

        public ICollectionView ItemView
        {
            get
            {
                return itemView.View;
            }
        }

        #endregion


        #region Commands


        public void SetItemType(ItemViewModel ivm, ItemType type)
        {
            if (ivm == null)
                return;

            Item i = SelectedItem.Item;
            Item newItem = i.SetType((ItemType)type);

            datastore.UpdateItem(i, newItem);

            SelectedItem.Item = newItem;

            RefreshItemsView();
        }
        
        public ICommand AddCommand
        {
            get
            {
                return new CommandHelper(AddSeries);
            }
        }

        private void AddSeries()
        {
            EditItemWindow edit = new EditItemWindow(datastore);
            edit.ShowDialog();

            if (edit.State == StateEnum.Saved)
                ReloadModels();
        }


        public ICommand EditCommand
        {
            get
            {
                return new CommandHelper(EditSeries);
            }
        }

        private void EditSeries()
        {
            if (SelectedItem == null)
                return;

            EditItemWindow edit = new EditItemWindow(datastore, SelectedItem);
            edit.ShowDialog();

            switch (edit.State)
            {
                case StateEnum.Saved:
                    UpdateItem(SelectedItem, edit.GetNewItem());
                    break;
                case StateEnum.Deleted:
                    ReloadModels();
                    break;
            }
        }

        private void UpdateItem(ItemViewModel oldItem, ItemViewModel newItem)
        {
            items.Remove(SelectedItem);
            items.Add(newItem);
        }
        #endregion

        #region Search
        private bool isSearching = false;
        public bool IsSearching
        {
            get
            {
                return isSearching;
            }
        }

        public bool NotSearching
        {
            get
            {
                return !isSearching;
            }
        }

        private string searchText;
        public string SearchText
        {
            get
            {
                return searchText;
            }
            set
            {
                searchText = value;
                RaisePropertyChanged("SearchText");
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return new CommandHelper(Search);
            }
        }


        private void Search()
        {
            if (String.IsNullOrEmpty(searchText))
                return;

            SetSearching(true);
            itemView.View.Refresh();
            RaisePropertyChanged("ItemViewTotal");
        }


        //TODO: Add Clear
        public ICommand ClearSearchCommand
        {
            get
            {
                return new CommandHelper(ClearSearch);
            }
        }


        private void ClearSearch()
        {
            if (!isSearching)
                return;

            SearchText = null;
            SetSearching(false);
            SetMode(viewType);
        }



        private void SetSearching(bool state)
        {
            isSearching = state;
            RaisePropertyChanged("NotSearching");
            RaisePropertyChanged("IsSearching");

            //itemView.View.Refresh();
        }

        #endregion

        public MainWindowViewModel(IDataStore ds)
        {
            datastore = ds;

            itemView = new CollectionViewSource();
            itemView.Source = items;
            itemView.Filter += itemView_Filter;
            ItemView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            InitializeRatings();
            InitializeCategories();

            ReloadModels();


            /* Batch edit urls
            List<Item> notModified = new List<Item>();
            foreach (Item i in datastore.GetAllItems())
            {
                if (i.URL.Contains(Match))
                {
                    string url = i.URL.Replace(Match, "goodanime.co");
                    try
                    {
                        datastore.UpdateItem(i, new Item(i.Name, url, i.ID, i.Type, i.Description, i.ModifiedDate, i.Rating));
                    }
                    catch (Exception e)
                    {
                        notModified.Add(i);
                    }
                }
            }
            */
        }


        private void ReloadModels()
        {
            ClearItems();

            foreach (Item i in datastore.GetAllItems())
            {
                AddItem(i);
            }

            RefreshItemsView();
        }

        private void InitializeCategories()
        {
            var list = new List<string>();
            list.AddRange(Enum.GetNames(typeof(ItemType))); //.OrderBy((a) => a));

            Categories = list.AsReadOnly();

            SelectedCategory = ItemType.Unread.ToString();
        }

        private void InitializeRatings()
        {
            var list = new List<string>();
            list.Add(AllRating);

            for (int i = 5; i > 0; i--)
            {
                list.Add(i.ToString());
            }

            list.Add(NARating);

            Ratings = list.AsReadOnly();
        }

        private void itemView_Filter(object sender, FilterEventArgs e)
        {
            var i = e.Item as ItemViewModel;

            if (i == null)
                e.Accepted = false;
            else if (!isSearching)
            {
                //Match Type
                if (i.ItemType != viewType)
                {
                    e.Accepted = false;
                }
                else if (showRating.HasValue
                    && showRating.Value != i.Rating)
                {
                    e.Accepted = false;
                }
                else
                    e.Accepted = true;
            }
            else
                e.Accepted = i.Name.ToUpper().Contains(SearchText.ToUpper());
            //true;
        }

        private string selectedCategory;
        public string SelectedCategory
        {
            get
            {
                return selectedCategory;
            }
            set
            {
                selectedCategory = value;
                SetMode(value);
                RaisePropertyChanged("SelectedCategory");
            }
        }

        private void SetMode(string value)
        {
            SetMode((ItemType)Enum.Parse(typeof(ItemType), value));
        }

        private void SetMode(ItemType type)
        {
            this.viewType = type;

            itemView.View.Refresh();

            RaisePropertyChanged("ItemViewTotal");
        }

        //For custom?
        private void AddItem(object item)
        {
            if (item is Item)
            {
                items.Add(new ItemViewModel((Item)item));
                RaisePropertyChanged("ItemViewTotal");
            }
        }

        private void AddItem(Item item)
        {
            items.Add(new ItemViewModel(item));
            RaisePropertyChanged("ItemViewTotal");
        }

        private void RemoveItem(Item item)
        {
            var vm = items.First(x => x.Item.Equals(item));

            if (vm != null)
            {
                RemoveItem(vm);
                RaisePropertyChanged("ItemViewTotal");
            }
        }

        private void RemoveItem(ItemViewModel viewModel)
        {
            items.Remove(viewModel);
        }

        private void ClearItems()
        {
            items.Clear();
        }

        private void RefreshItemsView()
        {
            ItemView.Refresh();
            RaisePropertyChanged("ItemViewTotal");
        }
    }
}
