using Dapper;
using InventorySystem.Interface;
using InventorySystem.Model;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace InventorySystem.Services
{
    public class DatabaseService : IDatabaseService
    {
        private string _dbPath;
        private string _connectionString;
        private ISqlQueryBuilder _queryBuilder;
        private IStringService _stringService;
        private ICSVService _csvService;
        private static readonly Dictionary<string, string> _allowedTables = new()
        {
            { "brands", "Brands" },
            { "product", "Product" }
        };
        private static readonly Dictionary<string, string> _allowedColumns = new()
        {
            { "name", "Name" },
            { "brandid", "BrandID" },
            { "brand", "Brand" },
            { "memorytype", "MemoryType" },
            { "memoryspeed", "MemorySpeed" },
            { "module", "Module" },
            { "totalcapacity", "TotalCapacity" },
            { "firstwordlatency", "FirstWordLatency" },
            { "caslatency", "CASLatency" },
            { "rating", "Rating" },
            { "reviewcount", "ReviewCount" },
            { "price", "Price" },
            { "pricepergb", "PricePerGB" },
            { "id", "Id" }
        };
        private readonly string appFolder;
        private static Dictionary<string, int> _brandCache = new(StringComparer.OrdinalIgnoreCase);
        public event Action<string, int> OnDataChanged;
        public DatabaseService(ICSVService CSVService, ISqlQueryBuilder sqlQueryBuilder, IStringService stringService)
        {
            _queryBuilder = sqlQueryBuilder;
            _stringService = stringService;
            _csvService = CSVService;
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            appFolder = Path.Combine(folder, "InventorySystem");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _dbPath = Path.Combine(appFolder, "inventory.db");

            _connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = _dbPath,
                Version = 3,
                ForeignKeys = true
            }.ToString();

            OnDataChanged += HandleDatabaseChanged;
        }
        public bool InitializeDatabase()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                string brand = @"CREATE TABLE IF NOT EXISTS Brand (
                    id    INTEGER   PRIMARY KEY
                                    AUTOINCREMENT
                                    NOT NULL,
                    Name  TEXT      UNIQUE
                                    NOT NULL
                )";

                // Create a simple table for your Inventory items
                string product = @"CREATE TABLE IF NOT EXISTS Product (
                    id                   INTEGER    PRIMARY KEY
                                                    AUTOINCREMENT
                                                    NOT NULL,
                    Name                 TEXT       NOT NULL,
                    BrandID              INTEGER    REFERENCES Brand (id)   ON DELETE SET NULL
                                                                            ON UPDATE CASCADE,
                    MemoryType           TEXT       NOT NULL,
                    MemorySpeed          INTEGER    NOT NULL,
                    Module               TEXT       NOT NULL,
                    TotalCapacity        INTEGER    NOT NULL,
                    FirstWordLatency     REAL,
                    CASLatency           REAL,
                    PricePerGB           REAL       DEFAULT (0) 
                                                    NOT NULL,
                    Price                REAL       NOT NULL
                                                    DEFAULT (0),
                    Color                TEXT,
                    ReviewCount          INTEGER    DEFAULT (0),
                    Rating               REAL       DEFAULT (0) 
                );";

                using (var command = new SQLiteCommand(brand, conn))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(product, conn))
                {
                    command.ExecuteNonQuery();
                }
            }

            if (IsTableEmpty("Brand"))
            {
                try
                {
                    ImportBrand();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (InvalidFilterCriteriaException ex)
                {
                    MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            UpdateBrandData();
            return true;
        }
        public List<RamData> GetRamDatas()
        {
            using var conn = new SQLiteConnection(_connectionString);
            string query = @"
                SELECT Product.*, Brand.Name AS Brand
                FROM Product
                LEFT JOIN Brand ON Product.BrandID = Brand.id
            ";
            return conn.Query<RamData>(query).ToList();
        }
        public Dictionary<string, int> GetBrandDatas()
        {
            return _brandCache;
        }
        public void InsertValuesIntoColumn(string tableName, string columnName, IEnumerable<string> items)
        {
            var safeTable = TableValidator(tableName);
            var safeColumn = ColumnValidator(columnName);

            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {

                string sql = _queryBuilder.BuildInsertToColumn(safeTable, safeColumn);

                var affected = conn.Execute(sql, items.Select(i => new { Value = i }), transaction);

                transaction.Commit();

                OnDataChanged?.Invoke(safeTable.ToLower(), affected);
            }
            catch
            {
                transaction.Rollback();
                throw new InvalidOperationException($"Cannot Insert Data to {tableName} with the column Name {columnName}");
            }
        }
        public void InsertValuesIntoColumn(string tableName, string columnName, string item)
        {
            InsertValuesIntoColumn(tableName, columnName, [item]);
        }

        public void InsertCollectionToProduct(IEnumerable<RamData> values)
        {
            foreach (RamData value in values)
            {
                var id = BrandIDtoName(value.BrandID);
                if (id == "Unknown Brand") throw new InvalidOperationException($"BrandID {value.BrandID} Doesn't Exist Can't Insert!");
            }

            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            string sql = _queryBuilder.BuildInsert<RamData>("Product", "id", "Brand");

            var failedItems = new List<string>();
            var affected = 0;

            foreach (var item in values)
            {
                try
                {
                    affected += conn.Execute(sql, item, transaction);
                }
                catch (Exception ex)
                {
                    failedItems.Add(
                        $"FAILED -> {_stringService.ObjectToString(item)}ERROR -> {ex.Message}"
                    );
                }
            }

            if (failedItems.Any())
            {
                transaction.Rollback();

                // 1. Create a safe filename (yyyyMMdd_HHmmss avoids illegal characters)
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                string filePath = Path.Combine(appFolder, $"DatabaseError_{timestamp}.txt");

                File.WriteAllLines(filePath, failedItems);

                throw new InvalidOperationException($"One or more items failed to insert. See error log at {filePath}.");
            }

            transaction.Commit();
            OnDataChanged?.Invoke("Product", affected);
        }
        public void InsertCollectionToProduct(RamData item)
        {
            InsertCollectionToProduct([item]);
        }

        public void DeleteFromTable(string tableName, string condition, object value)
        {
            var safeTable = TableValidator(tableName);
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = _queryBuilder.BuildDelete(safeTable, condition);

            using var transaction = conn.BeginTransaction();
            var affected = 0;
            try
            {
                affected += conn.Execute(sql, value, transaction);
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw new InvalidOperationException($"Can't Delete item where {condition} from {tableName}\n Reason {e.Message}");
            }
            transaction.Commit();
            OnDataChanged?.Invoke(tableName, affected);
        }
        public void UpdateFromTable(string tableName, string condition, RamData ramData)
        {
            var safeTable = TableValidator(tableName);

            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = _queryBuilder.BuildUpdate<RamData>(safeTable, condition, "id", "Brand");

            using var transaction = conn.BeginTransaction();
            var affected = 0;
            try
            {
                affected += conn.Execute(sql, ramData, transaction);
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw new InvalidOperationException($"Cannot Update {safeTable} with condition {condition} \n Reason {e.Message}");
            }

            transaction.Commit();
            OnDataChanged?.Invoke(tableName, affected);
        }
        public void UpdateFromTable(string tableName, string condition, object value)
        {
            var safeTable = TableValidator(tableName);

            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = _queryBuilder.BuildUpdate<RamData>(safeTable, condition, "id", "Brand");

            using var transaction = conn.BeginTransaction();
            var affected = 0;
            try
            {
                affected += conn.Execute(sql, value, transaction);
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw new InvalidOperationException($"Cannot Update {safeTable} with condition {condition} \n Reason {e.Message}");
            }

            transaction.Commit();
            OnDataChanged?.Invoke(tableName, affected);
        }
        public int BrandNameToID(string name)
        {
            // Return 0 if the string is empty or whitespace
            if (string.IsNullOrWhiteSpace(name)) return 0;

            name = name.Trim();

            // TryGetValue returns false if not found, so we return 0 as the fallback
            if (_brandCache.TryGetValue(name, out var id))
                return id;

            return -1;
        }
        public string BrandIDtoName(int id)
        {
            var name = _brandCache
                .Where(p => p.Value == id)
                .Select(p => p.Key)
                .FirstOrDefault();

            // If FirstOrDefault returns null (id not found), return a friendly string
            return name ?? "Unknown Brand";
        }
        private bool IsTableEmpty(string tableName)
        {
            using var conn = new SQLiteConnection(_connectionString);
            int count = conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM {tableName}");
            return count == 0;
        }
        private void ImportBrand()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "InventorySystem.Resources.brands.csv";
            using Stream stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                throw new InvalidOperationException($"Could not find resource: {resourceName}");
            }

            using var reader = new StreamReader(stream);
            var brandNames = _csvService.CSVReader(reader)
                                    .Select(b => b.Brand?.ToString())
                                    .Where(b => !string.IsNullOrWhiteSpace(b))
                                    .ToList().OfType<string>();

            InsertValuesIntoColumn("Brand", "Name", brandNames);
        }
        private void UpdateBrandData()
        {
            using var conn = new SQLiteConnection(_connectionString);
            var data = conn.Query<BrandData>("SELECT id, Name FROM Brand");

            // Use StringComparer.OrdinalIgnoreCase so "nike" matches "Nike"
            _brandCache = data.ToDictionary(
                b => b.Name,
                b => b.Id,
                StringComparer.OrdinalIgnoreCase
            );
        }
        private void HandleDatabaseChanged(string table, int affected)
        {
            MessageBox.Show($"Table {table} is Changed {affected} is Affected", "Database", MessageBoxButton.OK, MessageBoxImage.Information);
            if (table == "brand") { UpdateBrandData(); }
        }
        private string TableValidator(string tableName)
        {
            if (!_allowedTables.TryGetValue(tableName.ToLower(), out var safeTable))
            {
                throw new InvalidFilterCriteriaException("Invalid Table");
            }
            return safeTable;
        }
        private string ColumnValidator(string columnName)
        {
            if (!_allowedColumns.TryGetValue(columnName.ToLower(), out var safeColumn))
            {
                throw new InvalidFilterCriteriaException("Invalid Column");
            }
            return safeColumn;
        }
    }
}
