using InventorySystem.BaseClass;
using InventorySystem.Interface;
using InventorySystem.Model;
using InventorySystem.Services;
using System.Windows;

namespace InventorySystem.ViewModel.MainWindowViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        public RelayCommand UpdateBrandCommand => new RelayCommand(execute => UpdateBrand(), canExecute => CanUpdateBrand());
        public RelayCommand DeleteBrandCommand => new RelayCommand(execute => DeleteBrand(), canExecute => CanDeleteBrand());

        private bool _isReady;
        private readonly IDatabaseService _databaseService;
        public event Action? RequestClose;
        private List<string> _options;
        public List<string> Options
        {
            get { return _options; }
            set
            {
                _options = value;
                OnPropertyChanged();
            }
        }
        public bool IsReady
        {
            get { return _isReady; }
            set
            {
                _isReady = value;
                OnPropertyChanged();
            }
        }
        private string _selectedValue;
        public string SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                _selectedValue = value;
                NewValue = value;
                OnPropertyChanged();
            }
        }

        private string _newValue;

        public string NewValue
        {
            get { return _newValue; }
            set
            {
                _newValue = value;
                IsReady = !string.IsNullOrEmpty(_newValue);
                OnPropertyChanged();
                CanUpdateBrand();
            }
        }


        public SettingViewModel(IDatabaseService service)
        {
            _databaseService = service;
            _databaseService.BrandCacheUpdated += HandleBrandUpdate;
            Options = GetOptions();
            SelectedValue = Options[0];
        }
        private bool CanUpdateBrand()
        {
            return !string.IsNullOrEmpty(NewValue);
        }

        private void UpdateBrand()
        {
            BrandData brand = new BrandData() { id = _databaseService.BrandNameToID(SelectedValue), Name = NewValue };
            try
            {
                _databaseService.UpdateFromTable("Brands", "id = @id", brand, "id");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanDeleteBrand()
        {
            return !string.IsNullOrEmpty(SelectedValue);
        }

        private void DeleteBrand()
        {
            try
            {
                var brandId = _databaseService.BrandNameToID(SelectedValue);
                _databaseService.DeleteFromTable("Brands", "id = @id", new { id = brandId });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private List<string> GetOptions()
        {
            return _databaseService
                     .GetBrandDatas()
                     .OrderBy(b => b.Key)
                     .Select(b => b.Key)
                     .ToList();
        }
        private void HandleBrandUpdate()
        {
            var previousBrand = NewValue;

            Options = GetOptions();

            if (Options.Contains(previousBrand, StringComparer.OrdinalIgnoreCase))
            {
                SelectedValue = previousBrand;
            }
            else if (Options.Count > 0)
            {
                SelectedValue = Options[0];
                NewValue = Options[0];
            }
            else
            {
                SelectedValue = null;
                NewValue = null;
            }
        }
    }
}
