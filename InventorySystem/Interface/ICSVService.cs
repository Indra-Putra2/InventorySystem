using InventorySystem.Model;
using System.IO;

namespace InventorySystem.Interface
{
    public interface ICSVService
    {
        IEnumerable<dynamic> CSVReader(string path);
        IEnumerable<dynamic> CSVReader(StreamReader reader);
        List<RamData> CSVImport(string path);
    }
}
