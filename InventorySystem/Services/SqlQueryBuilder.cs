using InventorySystem.Interface;

namespace InventorySystem.Services
{
    public class SqlQueryBuilder : ISqlQueryBuilder
    {
        public string BuildInsertToColumn(string table, string column)
        {
            return $"INSERT OR IGNORE INTO {table} ({column}) VALUES (@Value)";
        }
        public string BuildInsert<T>(string table, params string[] propertyIgnore)
        {
            propertyIgnore ??= Array.Empty<string>();
            var props = typeof(T).GetProperties()
                                    .Where(p => !propertyIgnore
                                    .Any(i => string.Equals(i, p.Name, StringComparison.OrdinalIgnoreCase)));

            var columns = string.Join(", ", props.Select(p => p.Name));
            var paramList = string.Join(", ", props.Select(p => "@" + p.Name));

            return $"INSERT OR IGNORE INTO {table} ({columns}) VALUES ({paramList})";
        }
        public string BuildDelete(string table, string condition)
        {
            return $"DELETE FROM {table} WHERE {condition}";
        }
        public string BuildUpdate(string table, string condition, params (string column, object value)[] updates)
        {
            var setClause = string.Join(", ", updates.Select(u => $"{u.column} = @{u.column}"));
            return $"UPDATE {table} SET {setClause} WHERE {condition}";
        }
        public string BuildUpdate<T>(string table, string condition, params string[] propertyIgnore)
        {
            var props = typeof(T).GetProperties()
                                    .Where(p => !propertyIgnore
                                    .Any(i => string.Equals(i, p.Name, StringComparison.OrdinalIgnoreCase)));

            var setClause = string.Join(", ", props.Select(p => $"{p.Name} = @{p.Name}"));

            return $"UPDATE {table} SET {setClause} WHERE {condition}";
        }
    }
}
