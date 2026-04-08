using InventorySystem.Interface;
using InventorySystem.Model;
using InventorySystem.Services;
using System.Reflection;
using System.Windows;

namespace InventorySystem.ViewModel.AddWindowViewModel
{
    public class AddBrandViewModel
    {
        public RelayCommand AddCommand => new RelayCommand(execute => Add(), canExecute => CanAdd());
        public RelayCommand CloseCommand => new RelayCommand(execute => Close());
        public event Action? RequestClose;

        private IDatabaseService _databaseService;

        public List<object> ItemInput { get; set; }
        public AddBrandViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            ItemInput = new();

            var props = typeof(BrandData).GetProperties();
            foreach (var prop in props)
            {
                if (prop.Name == "id") { continue; }
                var input = CreateInput(prop, prop.Name);

                ItemInput.Add(input);
            }
        }


        private object CreateInput(PropertyInfo prop, string label)
        {
            var type = prop.PropertyType;

            if (type == typeof(string))
            {
                return new InputTextModel(prop.Name, label);
            }

            if (type == typeof(int) || type == typeof(double) || type == typeof(decimal))
            {
                return new InputNumericModel(prop.Name, label, type);
            }

            return new InputTextModel(prop.Name, label); // fallback
        }
        private bool CanAdd()
        {
            return ItemInput.Cast<IInputModel>().All(i => i.IsReady);
        }
        private void Add()
        {
            foreach (IInputModel item in ItemInput)
            {
                try
                {
                    _databaseService.InsertValuesIntoColumn("Brands", "Name", item.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,"Error",MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public void Close() => RequestClose?.Invoke();
    }
}
