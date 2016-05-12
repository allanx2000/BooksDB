using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksDB.Models
{
    public class ItemViewModel : ObservableClass
    {
        private Item item;

        public string Name
        {
            get
            {
                return item.Name;
            }
        }

        public int? ID
        {
            get
            {
                return item.ID;
            }
        }

        public Item Item
        {
            get
            {
                return item;
            }
            set
            {
                item = value;

                RaisePropertyChanged("Item");
                RaisePropertyChanged("Id");
                RaisePropertyChanged("Name");
                RaisePropertyChanged("ItemType");
                RaisePropertyChanged("Rating");
                RaisePropertyChanged("RatingText");
            }
        }


        public ItemViewModel(Item item)
        {
            this.item = item;
        }

        public ItemType ItemType { 
            get
            {
                return item.Type;
            }
        }

        public int Rating
        {
            get
            {
                return item.Rating;
            }
            set
            {
                item.Rating = value;
                RaisePropertyChanged("Rating");
                RaisePropertyChanged("RatingText");
            }
        }

        public string RatingText
        {
            get
            {
                return Rating == 0 ? "NA" : Rating.ToString();
            }
        }

    }
}
