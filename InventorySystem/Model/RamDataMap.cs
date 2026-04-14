using CsvHelper.Configuration;
using System.Text.RegularExpressions;

namespace InventorySystem.Model
{
    internal class RamDataMap : ClassMap<RamData>
    {
        public RamDataMap()
        {
            Map(m => m.Name).Name("Name");
            Map(m => m.Brand).Name("Brand");
            Map(m => m.MemoryType).Name("Memory Type");
            Map(m => m.MemorySpeed).Name("Memory Speed");
            Map(m => m.Module).Name("Module");
            Map(m => m.TotalCapacity).Name("Total Capacity");
            Map(m => m.FirstWordLatency).Name("First Word Latency");
            Map(m => m.CASLatency).Name("CAS Latency");
            Map(m => m.Price).Name("Price");
            Map(m => m.PricePerGB).Name("Price Per GB");
            Map(m => m.Color).Convert(row =>
            {
                string raw = row.Row.GetField("Color") ?? "";
                return Regex.Replace(raw, @"[\[\]'\s+]", "");
            });
            Map(m => m.Rating).Name("Rating");
            Map(m => m.ReviewCount).Name("Review Count");

            // optional / missing fields
            Map(m => m.BrandID).Ignore();
            Map(m => m.id).Ignore();
        }
    }
}
