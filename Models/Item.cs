using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksDB.Models
{
    public struct Item
    {
        public int? ID { get; private set; }
        public ItemType Type { get; private set; }
        public string Name { get; private set; }
        
        public string URL { get; private set; }

        public int Rating { get; set; }

        public DateTime ModifiedDate {get; private set;}
        public Item(string name, string url,
             int? id = null, ItemType type = ItemType.Unread, DateTime? modifiedDate = null, int rating = 0)
            : this()
        {
            this.Name = name;
            this.URL = url;
            this.ID = id;
            this.Type = type;
            this.Rating = rating;

            if (modifiedDate.HasValue)
                this.ModifiedDate = modifiedDate.Value;
            else ModifiedDate = DateTime.Now;
        }


        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
                return false;
            else
                return this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public Item SetType(ItemType itemType)
        {
            return new Item()
            {
                Name = this.Name,
                URL = this.URL,
                ID = this.ID,
                Type = itemType,
                ModifiedDate = DateTime.Now,
                Rating = this.Rating,
            };
        }
    }
}
