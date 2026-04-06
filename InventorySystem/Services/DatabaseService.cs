using Dapper;
using InventorySystem.Interface;
using InventorySystem.Model;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.Pkcs;
using System.Windows;

namespace InventorySystem.Services
{
    public class DatabaseService : IDatabaseService
    {
        private string _dbPath;
        private string _connectionString;
        private ICSVService _csvService;
        private static readonly Dictionary<string, string> _allowedTables = new()
        {
            { "brands", "Brands" },
            { "products", "Products" }
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
        private static Dictionary<string, int> _brandCache = new(StringComparer.OrdinalIgnoreCase);
        public event Action<string> OnDataChanged;
        public DatabaseService(ICSVService CSVService)
        {
            _csvService = CSVService;
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(folder, "InventorySystem");

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
        public void InitializeDatabase()
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
                    Name                 TEXT       UNIQUE
                                                    NOT NULL,
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
            if (!_allowedTables.TryGetValue(tableName.ToLower(), out var safeTable))
            {
                throw new InvalidFilterCriteriaException("Invalid Table");
            }

            if (!_allowedColumns.TryGetValue(columnName.ToLower(), out var safeColumn))
            {
                throw new InvalidFilterCriteriaException("Invalid Column");
            }

            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                string sql = $"INSERT OR IGNORE INTO {safeTable} ({safeColumn}) VALUES (@Value)";

                conn.Execute(sql, items.Select(i => new { Value = i }), transaction);

                transaction.Commit();

                OnDataChanged?.Invoke(safeTable.ToLower());
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
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                string sql = @"
                INSERT OR IGNORE INTO Product
                (Name, BrandID, MemoryType, MemorySpeed, Module, TotalCapacity, 
                 FirstWordLatency, CASLatency, PricePerGB, Price, Color, ReviewCount, Rating)
                VALUES
                (@Name, @BrandID, @MemoryType, @MemorySpeed, @Module, @TotalCapacity,
                 @FirstWordLatency, @CASLatency, @PricePerGB, @Price, @Color, @ReviewCount, @Rating)";

                foreach (var item in values)
                {
                    try
                    {
                        conn.Execute(sql, item, transaction);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to insert product:\n" +
                            $"Name: {item.Name}, BrandID: {item.BrandID}, Price: {item.Price}\n" +
                            $"Reason: {ex.Message}",
                            ex
                        );
                    }
                }

                transaction.Commit();

                OnDataChanged?.Invoke("product");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public void InsertCollectionToProduct(RamData item)
        {
            InsertCollectionToProduct([item]);
        }

        public int? BrandNameToID(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            name = name.Trim();

            if (_brandCache.TryGetValue(name, out var id))
                return id;

            return null;
        }
        public string? BrandIDtoName(int id)
        {
            return _brandCache.FirstOrDefault(x => x.Value == id).Key;
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
            Debug.WriteLine("Updating Brand Data");
            using var conn = new SQLiteConnection(_connectionString);
            var data = conn.Query<BrandData>("SELECT id, Name FROM Brand");

            // Use StringComparer.OrdinalIgnoreCase so "nike" matches "Nike"
            _brandCache = data.ToDictionary(
                b => b.Name,
                b => b.Id,
                StringComparer.OrdinalIgnoreCase
            );
        }
        private void HandleDatabaseChanged(string table)
        {
            if (table == "brand") { UpdateBrandData(); }
        }
    }
}
