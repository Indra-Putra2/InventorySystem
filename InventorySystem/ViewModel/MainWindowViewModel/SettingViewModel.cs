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
        private string _brandold;
        public string BrandOld
        {
            get { return _brandold; }
            set
            {
                _brandold = value;
                BrandNew = value;
                OnPropertyChanged();
            }
        }

        private string _brandNew;

        public string BrandNew
        {
            get { return _brandNew; }
            set
            {
                _brandNew = value;
                IsReady = !string.IsNullOrEmpty(_brandNew);
                OnPropertyChanged();
                CanUpdateBrand();
            }
        }


        public SettingViewModel(IDatabaseService service)
        {
            _databaseService = service;
            _databaseService.BrandCacheUpdated += HandleBrandUpdate;
            Options = GetOptions();
        }
        private bool CanUpdateBrand()
        {
            return !string.IsNullOrEmpty(BrandNew);
        }

        private void UpdateBrand()
        {
            BrandData brand = new BrandData() { id = _databaseService.BrandNameToID(BrandOld), Name = BrandNew };
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
            return !string.IsNullOrEmpty(BrandOld);
        }

        private void DeleteBrand()
        {
            try
            {
                var brandId = _databaseService.BrandNameToID(BrandOld);
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
            var previousBrand = BrandNew;

            Options = GetOptions();

            if (Options.Contains(previousBrand, StringComparer.OrdinalIgnoreCase))
            {
                BrandOld = previousBrand;
            }
            else if (Options.Count > 0)
            {
                BrandOld = Options[0];
                BrandNew = Options[0];
            }
            else
            {
                BrandOld = null;
                BrandNew = null;
            }
        }
    }
}
