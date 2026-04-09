using Dapper;
using InventorySystem.Interface;
using System.Text.RegularExpressions;

namespace InventorySystem.Services
{
    public class SqlQueryBuilder : ISqlQueryBuilder
    {
        private static readonly Dictionary<string, string> operators = new()
        {
            {":", "LIKE" },
            {"=", "=" },
            {">", ">" },
            {"<", "<" },
            {">=", ">=" },
            {"<=", "<=" },
            {"!=", "<>" }
        };
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
        public (string sql, DynamicParameters param) BuildSearch(string table, string search)
        {
            const string pattern = @"(\w+)([=><:!]+)(\w+)(\s*)";
            var matches = Regex.Matches(search, pattern);

            var queryParts = new List<string>();
            var parameters = new DynamicParameters();

            int i = 0;

            foreach (Match match in matches)
            {
                string columnName = match.Groups[1].Value;
                string sqlOp = GetOperator(match.Groups[2].Value);
                string value = match.Groups[3].Value;

                string paramName = $"@p{i++}";

                if (sqlOp == "LIKE")
                {
                    parameters.Add(paramName, $"%{value}%");
                }
                else
                {
                    parameters.Add(paramName, value);
                }

                queryParts.Add($"{columnName} {sqlOp} {paramName}");
            }
            if (queryParts.Count == 0)
            {
                parameters.Add("@p1", $"%{search}%");
                return ($"SELECT * FROM {table} WHERE Name LIKE @p1", parameters);
            }

            string finalSQL = string.Join(" AND ", queryParts);
            string sql = $"SELECT * FROM {table} WHERE {finalSQL}";

            return (sql, parameters);
        }

        private string GetOperator(string op) => operators.GetValueOrDefault(op) ?? "";
    }
}
