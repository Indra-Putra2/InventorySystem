using InventorySystem.Interface;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace InventorySystem.Model
{
    public class RamData
    {
        public string Brand { get; set; }
        public string Name { get; set; }
        public int BrandID { get; set; }
        public string MemoryType { get; set; }
        public int MemorySpeed { get; set; }
        public string Module { get; set; }
        public int TotalCapacity { get; set; }

        public double FirstWordLatency { get; set; }
        public double CASLatency { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }

        public string Color { get; set; }
        public decimal Price { get; set; }
        public decimal PricePerGB { get; set; }

        public IEnumerable<(PropertyInfo Name, object? Value)> GetProperties()
        {
            foreach (var prop in typeof(RamData).GetProperties())
            {
                yield return (prop, prop.GetValue(this));
            }
        }

        public void RamBuilder(string propertyName, object value)
        {
            var property = typeof(RamData).GetProperty(propertyName);
            if (property == null) return;

            try
            {
                var convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(this, convertedValue);
            }
            catch
            {
                throw new InvalidOperationException($"Can't Set Value '{value}' to '{property.Name}' with type '{property.PropertyType}'");
            }
        }
    }
}
