using Dapper;
using InventorySystem.Interface;
using InventorySystem.Model;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace InventorySystem.Services
{
    public class DatabaseService : IDatabaseService
    {
        private string _dbPath;
        private string _connectionString;
        private ICSVService _csvService;

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
                ImportBrand();
            }
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
        public List<BrandData> GetBrandDatas()
        {
            using var conn = new SQLiteConnection(_connectionString);
            string query = @"SELECT * FROM Brand";
            var result = conn.Query<RamData>(query);
            return conn.Query<BrandData>(query).ToList();
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
                throw new Exception($"Could not find resource: {resourceName}");
            }

            using var reader = new StreamReader(stream);
            var brandNames = _csvService.CSVReader(reader)
                                    .Select(b => (string)b.Brand)
                                    .ToList();

            InsertCollectionToTable("Brand", "Name", brandNames);
        }
        private void InsertCollectionToTable(string tableName, string columnName, IEnumerable<string> items)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                string sql = $"INSERT OR IGNORE INTO {tableName} ({columnName}) VALUES (@Value)";

                conn.Execute(sql, items.Select(i => new { Value = i }), transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
