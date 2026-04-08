using InventorySystem.Interface;

namespace InventorySystem.Services
{
    public class StringService : IStringService
    {
        public string ObjectToString<T>(T obj)
        {
            var props = typeof(T).GetProperties();

            return string.Join(", ", props.Select(p =>
            {
                var value = p.GetValue(obj) ?? "NULL";
                return $"{p.Name}: {value} type:{p.PropertyType}\n";
            }));
        }
    }
}
