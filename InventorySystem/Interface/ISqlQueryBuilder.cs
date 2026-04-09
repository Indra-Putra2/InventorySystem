using Dapper;

namespace InventorySystem.Interface
{
    public interface ISqlQueryBuilder
    {
        string BuildInsertToColumn(string table, string column);
        string BuildInsert<T>(string table, params string[] propertyIgnore);
        string BuildDelete(string table, string condition);
        string BuildUpdate(string table, string condition, params (string column, object value)[] updates);
        string BuildUpdate<T>(string table, string condition, params string[] propertyIgnore);
        (string sql, DynamicParameters param) BuildSearch(string table, string search);
    }
}
