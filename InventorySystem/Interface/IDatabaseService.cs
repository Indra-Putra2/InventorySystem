using InventorySystem.Model;
using System.Data.SQLite;
using System.Reflection;

namespace InventorySystem.Interface
{
    public interface IDatabaseService
    {
        event Action<string> OnDataChanged;
        void InitializeDatabase();
        List<RamData> GetRamDatas();
        Dictionary<string, int> GetBrandDatas();
        void InsertValuesIntoColumn(string tableName, string columnName, IEnumerable<string> items);
        void InsertValuesIntoColumn(string tableName, string columnName, string item);
        void InsertCollectionToProduct(IEnumerable<RamData> values);
        void InsertCollectionToProduct(RamData values);
        int? BrandNameToID(string name);
        string? BrandIDtoName(int id);
    }
}
