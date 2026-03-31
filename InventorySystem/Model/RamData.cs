using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace InventorySystem.Model
{
    public class RamData : IEnumerable<(PropertyInfo Prop, object Value)>
    {
        public string Name { get; set; }
        public int BrandID { get; set; }
        public string Brand { get; set; }
        public string MemoryType { get; set; }
        public int MemorySpeed { get; set; }
        public string Module { get; set; }
        public int TotalCapacity { get; set; }

        // Use doubles for these
        public double FirstWordLatency { get; set; }
        public double CASLatency { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }

        // Use decimal for financial data
        public decimal Price { get; set; }
        public decimal PricePerGB { get; set; }

        public IEnumerator<(PropertyInfo Prop, object Value)> GetEnumerator()
        {
            var properties = typeof(RamData).GetProperties();

            foreach (var prop in properties)
            {
                yield return (prop, prop.GetValue(this));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
