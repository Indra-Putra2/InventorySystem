using InventorySystem.Interface;
using InventorySystem.Model;
using InventorySystem.Services;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;

namespace InventorySystem.ViewModel.MainWindowViewModel
{
    public class UpdateViewModel
    {
        private IDatabaseService _databaseService;
        private ISelectionService _selection;
        public RelayCommand UpdateCommand => new RelayCommand(execute => Update(), canExecute => CanUpdate());
        public RelayCommand CancelCommand => new RelayCommand(execute => Cancel());
        public Action? RequestClose;
        public ObservableCollection<object> ItemInput { get; set; }
        public UpdateViewModel(ISelectionService selectionService, IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            _selection = selectionService;
            ItemInput = new();

            foreach (var (prop, value) in _selection.SelectedRam.GetProperties())
            {
                if (prop.Name == "Brand" || prop.Name == "id") continue;

                string label = RegexHelper.SplitName(prop.Name);

                object input = CreateInput(prop, label, value);

                ItemInput.Add(input);
            }
        }
        private object CreateInput(PropertyInfo prop, string label, object value)
        {
            var type = prop.PropertyType;
            string newVal = value?.ToString() ?? "";
            int id = int.TryParse(value?.ToString(), out var result) ? result : 0;

            if (prop.Name == "BrandID")
            {
                var brands = _databaseService
                    .GetBrandDatas()
                    .OrderBy(b => b.Key)
                    .Select(b => b.Key)
                    .ToList();

                var brandName = _databaseService.BrandIDtoName(id);
                return new InputComboModel(prop.Name, label, brands, brandName);
            }

            if (type == typeof(string))
            {
                return new InputTextModel(prop.Name, label, newVal);
            }

            if (type == typeof(int) || type == typeof(double) || type == typeof(decimal))
            {
                return new InputNumericModel(prop.Name, label, type, newVal);
            }
            return new InputTextModel(prop.Name, label, newVal); // fallback
        }

        private void Update()
        {
            RamData ram = new RamData();
            ram.id = _selection.SelectedRam.id;
            try
            {
                foreach (IInputModel item in ItemInput)
                {
                    if (item.Key == "BrandID")
                    {
                        var id = _databaseService.BrandNameToID(item.Value);
                        ram.RamBuilder(item.Key, id);
                    }
                    else
                    {
                        ram.RamBuilder(item.Key, item.Value);
                    }
                }
                _databaseService.UpdateFromTable("Product", "id = @id", ram);
                RequestClose?.Invoke();
            }
            catch (InvalidFilterCriteriaException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanUpdate()
        {
            return ItemInput.Cast<IInputModel>().All(i => i.IsReady);
        }
        private void Cancel()
        {
            RequestClose?.Invoke();
        }
    }
}
