using InventorySystem.Model;

namespace InventorySystem.Interface
{
    public interface IDatabaseService
    {
        event Action<DataChangedEventArgs> OnDataChanged;
        event Action BrandCacheUpdated;
        event Action DatabaseReady;
        bool InitializeDatabase();
        List<RamData> GetRamDatas();
        Dictionary<string, int> GetBrandDatas();
        void InsertValuesIntoColumn(string tableName, string columnName, IEnumerable<string> items);
        void InsertValuesIntoColumn(string tableName, string columnName, string item);
        void InsertCollection<T>(string tableName, IEnumerable<T> values, params string[] propertyIgnore);
        void InsertCollection<T>(string tableName, T item, params string[] propertyIgnore);
        void UpdateFromTable<T>(string tableName, string condition, T data, params string[] ignoreColumn);
        void DeleteFromTable(string tableName, string condition, object value);
        List<T> SearchFromTable<T>(string tableName, string search);
        int BrandNameToID(string name);
        string BrandIDtoName(int id);
    }
}
