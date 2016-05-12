using BooksDB.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksDB.Data
{
    public interface IDataStore
    {
        List<Item> GetAllItems();
        List<Item> AddItems(IEnumerable<Item> items);
        Item AddItem(Item item);

        void UpdateItem(Item i, Item newItem);
        List<Item> FindByName(string name, bool exact);
        void DeleteItem(int id);
    }
}
