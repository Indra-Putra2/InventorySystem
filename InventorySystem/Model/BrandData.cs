namespace InventorySystem.Model
{
    public class BrandData
    {
        public int id { get; set; }
        public string Name { get; set; }

        public void BrandataBuilder(string propertyName, string value)
        {
            var property = typeof(BrandData).GetProperty(propertyName);
            if (property != null) return;

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
