using InventorySystem.Interface;
using InventorySystem.Model;
using InventorySystem.Services;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace InventorySystem.ViewModel.AddWindowViewModel
{
    public class AddRamViewModel
    {
        public RelayCommand AddCommand => new RelayCommand(execute => Add(), canExecute => CanAdd());
        public RelayCommand CloseCommand => new RelayCommand(execute => Close());
        public Action? RequestClose;
        private IDatabaseService _databaseService;

        public List<object> ItemInput { get; set; }
        private RamData RamData = new();
        public AddRamViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            _databaseService.BrandCacheUpdated += HandleBrandDataUpdate;
            ItemInput = new();

            foreach (var (prop, value) in RamData.GetProperties())
            {
                if (prop.Name == "Brand" || prop.Name == "id") continue;

                string label = RegexHelper.SplitName(prop.Name);

                object input = CreateInput(prop, label);

                if (input is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged += Child_PropertyChanged;
                }

                ItemInput.Add(input);
            }
        }

        private object CreateInput(PropertyInfo prop, string label)
        {
            var type = prop.PropertyType;

            if (prop.Name == "BrandID")
            {
                var brands = _databaseService
                    .GetBrandDatas()
                    .OrderBy(b => b.Key)
                    .Select(b => b.Key)
                    .ToList();

                return new InputComboModel(prop.Name, label, brands);
            }

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
        private void Child_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IInputModel.IsReady))
            {
                CanAdd();
            }
        }
        private bool CanAdd()
        {
            return ItemInput.Cast<IInputModel>().All(i => i.IsReady);
        }
        private void Add()
        {
            RamData ram = new RamData();

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
                _databaseService.InsertCollectionToProduct("Products", ram, "id", "Brand");

                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void Close() => RequestClose?.Invoke();
        private void HandleBrandDataUpdate()
        {
            var brands = _databaseService
                            .GetBrandDatas()
                            .OrderBy(b => b.Key)
                            .Select(b => b.Key)
                            .ToList();

            foreach (var i in ItemInput.OfType<InputComboModel>().Where(i => i.Key == "BrandID"))
            {
                i.Options = brands;
            }
        }
    }
}
