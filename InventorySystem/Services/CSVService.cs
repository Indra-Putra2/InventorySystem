using CsvHelper;
using InventorySystem.Interface;
using InventorySystem.Model;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace InventorySystem.Services
{
    public class CSVService : ICSVService
    {
        public IEnumerable<dynamic> CSVReader(string path)
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return csv.GetRecords<dynamic>().ToList();
        }

        public IEnumerable<dynamic> CSVReader(StreamReader reader)
        {
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return csv.GetRecords<dynamic>().ToList();
        }

        public IEnumerable<RamData> CSVImport(string path)
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<RamDataMap>();
            IEnumerable<RamData> items;
            try
            {
                items = csv.GetRecords<RamData>().ToList() ?? new List<RamData>();
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
            
            Debug.WriteLine($"Count: {items.Count()}");
            foreach (var item in items.Take(10))
            {
                Debug.WriteLine($"{item.Brand} - {item.Name} - {item.TotalCapacity}");
            }
            return items;
        }
    }
}
