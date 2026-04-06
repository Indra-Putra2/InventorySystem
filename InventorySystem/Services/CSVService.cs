using CsvHelper;
using InventorySystem.Interface;
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
            return csv.GetRecords<dynamic>();
        }
    }
}
