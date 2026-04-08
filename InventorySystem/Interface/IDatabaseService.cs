using InventorySystem.Model;
using System.Data.SQLite;
using System.Reflection;

namespace InventorySystem.Interface
{
    public interface IDatabaseService
    {
        event Action<string, int> OnDataChanged;
        bool InitializeDatabase();
        List<RamData> GetRamDatas();
        Dictionary<string, int> GetBrandDatas();
        void InsertValuesIntoColumn(string tableName, string columnName, IEnumerable<string> items);
        void InsertValuesIntoColumn(string tableName, string columnName, string item);
        void InsertCollectionToProduct(IEnumerable<RamData> values);
        void InsertCollectionToProduct(RamData values);
        public void UpdateFromTable(string tableName, string condition, RamData ramData);
        public void UpdateFromTable(string tableName, string condition, object value);
        public void DeleteFromTable(string tableName, string condition, object value);
        int BrandNameToID(string name);
        public string BrandIDtoName(int id);
    }
}
