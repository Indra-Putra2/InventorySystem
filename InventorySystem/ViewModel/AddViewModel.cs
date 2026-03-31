using InventorySystem.BaseClass;
using InventorySystem.Interface;
using InventorySystem.Model;
using InventorySystem.Services;
using System.Collections.ObjectModel;
using System.Reflection;

namespace InventorySystem.ViewModel
{
    public class AddViewModel : ViewModelBase
    {
        private IDatabaseService _databaseService;

        public ObservableCollection<object> ItemInput { get; set; }
        private RamData RamData = new();
        public AddViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            ItemInput = new();

            foreach (var (prop, value) in RamData)
            {
                if (prop.Name == "Brand") continue;

                string label = RegexHelper.SplitName(prop.Name);

                object input = CreateInput(prop, label);

                ItemInput.Add(input);
            }
        }

        private object CreateInput(PropertyInfo prop, string label)
        {
            var type = prop.PropertyType;
            var brands = _databaseService
                                .GetBrandDatas()
                                .OrderBy(b => b.Name)
                                .Select(b => b.Name)
                                .ToList();
            if (prop.Name == "BrandID")
            {
                return new InputComboModel(label, brands);
            }

            if (type == typeof(string))
            {
                return new InputTextModel(label);
            }

            if (type == typeof(int) || type == typeof(double) || type == typeof(decimal))
            {
                return new InputTextModel(label); // later you can make numeric input
            }

            return new InputTextModel(label); // fallback
        }
    }
}
