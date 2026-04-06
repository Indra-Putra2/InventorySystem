using InventorySystem.BaseClass;
using InventorySystem.Interface;
using InventorySystem.Model;
using InventorySystem.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace InventorySystem.ViewModel
{
    public class AddViewModel : ViewModelBase
    {
        public RelayCommand AddCommand => new RelayCommand(execute => Add(), canExecute => UpdateStatus());
        public RelayCommand CancelCommand => new RelayCommand(execute => Cancel());
        public Action? RequestClose;
        private IDatabaseService _databaseService;

        public ObservableCollection<object> ItemInput { get; set; }
        private RamData RamData = new();
        public AddViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            ItemInput = new();

            foreach (var (prop, value) in RamData.GetProperties())
            {
                if (prop.Name == "Brand") continue;

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

        public bool UpdateStatus()
        {
            return ItemInput.Cast<IInputModel>().All(i => i.IsReady);
        }

        private void Child_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IInputModel.IsReady))
            {
                UpdateStatus();
            }
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
                _databaseService.InsertCollectionToProduct(ram);

                RequestClose?.Invoke();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Cancel() => RequestClose?.Invoke();
    }
}
