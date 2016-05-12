using BooksDB.Models;
using Innouvous.Utils.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksDB.Data
{
    class SQLiteDataStore : IDataStore
    {
        private const string ScriptsPath = @"Data\SQL";

        private SQLiteClient client;

        private const string SeriesTable = "tbl_series";
        private const string RatingsTable = "tbl_rating";


        private Properties.Settings Config = Properties.Settings.Default;

        public SQLiteDataStore()
        {
            string file = Config.DBPath;

            if (File.Exists(file))
            {
                client = new SQLiteClient(file, false);
                Upgrade();
            }
            else
            {
                client = new SQLiteClient(file, true);
                CreateTables();
            }
        }

        private void Upgrade()
        {
            //Check Ratings table exists

            if (!TableExists(RatingsTable))
            {
                CreateRatingsTable();
            }
        }

        private bool TableExists(string tableName)
        {
            string command = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='" + tableName + "' COLLATE NOCASE";
            int result = Convert.ToInt32(client.ExecuteScalar(command));

            return result > 0;
        }

        private void CreateRatingsTable()
        {
            string command = LoadCommandFT("CreateRatingsTable", RatingsTable, SeriesTable);
            client.ExecuteNonQuery(command);
        }

        private void CreateTables()
        {
            CreateSeriesTable();

            CreateRatingsTable();
        }

        private void CreateSeriesTable()
        {
            string command = LoadCommandFT("CreateSeriesTable", SeriesTable);
            client.ExecuteNonQuery(command);
        }

        private string LoadCommandFT(string fileName, params object[] args)
        {
            var result = SQLUtils.LoadCommandFromText(ScriptsPath, fileName, "sql", args);
            return result;
        }

        /*public ObservableCollection<Item> Items
        {
            get {
                if (allItems == null)
                    LoadItems();

                return allItems;
            }
        }*/

        private readonly string FullSelect = 
            String.Format("select ID, Type, Name, URL, ModifiedDate, Rating from {0} series",SeriesTable) +
            String.Format(" LEFT JOIN {0} rating on series.ID = rating.SeriesId", RatingsTable);


        public List<Item> GetAllItems()
        {

            string query = FullSelect;

            List<Item> allItems = GetItems(query);

            return allItems;
        }

        private List<Item> GetItems(string query)
        {
            List<Item> allItems = new List<Item>();

            DataTable dt = client.ExecuteSelect(query);

            foreach (DataRow r in dt.Rows)
            {
                int id = Convert.ToInt32(r["ID"]);
                ItemType type = (ItemType)Convert.ToInt32(r["Type"]);
                string name = r["Name"].ToString();
                string url = r["URL"].ToString();
                DateTime date = Convert.ToDateTime(r["ModifiedDate"]);

                object rating = r["Rating"];

                Item item = new Item(name, url, id, type, date,
                    rating == DBNull.Value ? 0 : Convert.ToInt32(rating));

                allItems.Add(item);
            }

            return allItems;
        }

        public List<Item> AddItems(IEnumerable<Item> items)
        {
            List<Item> addedItems = new List<Item>();

            foreach (var i in items)
            {
                if (!Exists(i))
                {
                    var newItem = SQLInsertItem(i);
                    
                    addedItems.Add(newItem);
                }
            }

            return addedItems;
        }

        private Item SQLInsertItem(Item i)
        {
            string command = LoadCommandFT("InsertSeriesTable", SeriesTable, (int) i.Type, i.Name, i.URL, SQLUtils.ToSQLDateTime(i.ModifiedDate));
            client.ExecuteNonQuery(command);

            int id = SQLUtils.GetLastInsertRow(client);

            return new Item(i.Name, i.URL, id, i.Type, rating: i.Rating);

        }

        private bool Exists(Item i)
        {
            string command = LoadCommandFT("CheckSeriesExists", SeriesTable, i.Name, i.URL);

            int count = Convert.ToInt32(client.ExecuteScalar(command));

            return count > 0;  
        }




        public void UpdateItem(Item i, Item newItem)
        {
            if (i.ID != newItem.ID)
                throw new Exception("ID does not match");

            string command = LoadCommandFT("UpdateSeriesTable", SeriesTable,
                (int)newItem.Type,
                newItem.Name, newItem.URL, 
                SQLUtils.ToSQLDateTime(newItem.ModifiedDate),
                newItem.ID);

            client.ExecuteNonQuery(command);

            //Update Rating
            command = String.Format("DELETE from {0} WHERE SeriesID={1}", RatingsTable, i.ID.Value);
            client.ExecuteNonQuery(command);

            command = String.Format("INSERT INTO {0} VALUES({1},{2})", RatingsTable, newItem.ID.Value, newItem.Rating);
            client.ExecuteNonQuery(command);
        }

        public void DeleteItem(int id)
        {
            string command = String.Format("DELETE from {0} WHERE ID={1}", SeriesTable, id);
            client.ExecuteNonQuery(command);
        }

        public List<Item> FindByName(string name, bool exact)
        {
            string command = FullSelect;
            command += " WHERE Name ";

            if (exact)
                command += " = '" + name + "'";
            else
                command += " LIKE '%" + name +"%'";

            var results = GetItems(command);
            return results;
        }

        public Item AddItem(Item item)
        {
            if (!Exists(item))
            {
                var newItem = SQLInsertItem(item);
                return newItem;
            }
            else throw new Exception("Item already exists.");
        }

        
    }
}
