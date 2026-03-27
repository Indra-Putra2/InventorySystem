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
                    id                   INTEGER PRIMARY KEY
                                                  AUTOINCREMENT
                                                  NOT NULL,
                    Name                 TEXT    UNIQUE
                                                    NOT NULL,
                    Brand                INTEGER REFERENCES Brand (id) ON DELETE SET NULL
                                                                        ON UPDATE CASCADE,
                    MemoryType           TEXT    NOT NULL,
                    MemorySpeed          INTEGER NOT NULL,
                    Module               TEXT    NOT NULL,
                    TotalCapacity        INTEGER NOT NULL,
                    FirstWordLatency     REAL,
                    CASLatency           REAL,
                    PricePerGB           REAL    DEFAULT (0) 
                                                    NOT NULL,
                    Price                REAL    NOT NULL
                                                    DEFAULT (0),
                    Color                TEXT,
                    ReviewCount          INTEGER DEFAULT (0),
                    Rating               REAL    DEFAULT (0) 
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
            string sql = $"SELECT * FROM Product";
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            using var cmd = new SQLiteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            var list = new List<RamData>();

            while (reader.Read())
            {
                var data = new RamData
                {
                    Name = reader["Name"].ToString(),
                    Brand = Convert.ToInt32(reader["Brand"]),
                    MemoryType = reader["MemoryType"].ToString(),
                    MemorySpeed = Convert.ToDouble(reader["MemorySpeed"]),
                    Module = reader["Module"].ToString(),
                    TotalCapacity = Convert.ToInt32(reader["TotalCapacity"]),
                    FirstWordLatency = Convert.ToDouble(reader["FirstWordLatency"]),
                    CASLatency = Convert.ToDouble(reader["CASLatency"]),
                    PricePerGB = Convert.ToDouble(reader["PricePerGB"]),
                    Price = Convert.ToDouble(reader["Price"]),
                    Color = reader["Color"].ToString(),
                    ReviewCount = Convert.ToInt32(reader["TotalCapacity"]),
                    Rating = Convert.ToDouble(reader["Rating"])
                };

                list.Add(data);
            }
            return list;
        }
        private bool IsTableEmpty(string tableName)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = new SQLiteCommand($"SELECT COUNT(*) FROM {tableName}", conn);
            return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
        }
        private void ImportBrand()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "InventorySystem.brands.csv";
            using Stream stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                throw new Exception($"Could not find resource: {resourceName}");
            }

            using var reader = new StreamReader(stream);
            var brandNames = _csvService.CSVReader(reader)
                                    .Select(r => (string)r.Brand)
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
                string sql = $"INSERT OR IGNORE INTO {tableName} ({columnName}) VALUES (@value)";

                foreach (var item in items)
                {
                    using var cmd = new SQLiteCommand(sql, conn, transaction);
                    cmd.Parameters.AddWithValue("@value", item);
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
