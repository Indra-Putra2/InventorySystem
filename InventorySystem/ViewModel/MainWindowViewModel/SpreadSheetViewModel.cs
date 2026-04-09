using InventorySystem.BaseClass;
using InventorySystem.Interface;
using InventorySystem.Model;
using InventorySystem.Services;
using InventorySystem.View;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;

namespace InventorySystem.ViewModel.MainWindowViewModel
{
    public class SpreadSheetViewModel : ViewModelBase
    {
        private readonly ICSVService _csvService;
        private readonly ISelectionService _selectionService;
        private readonly IDatabaseService _databaseService;
        private readonly IWindowFactory _WindowFactory;

        public ObservableCollection<RamData> RamDatas { get; set; }
        public RelayCommand InsertCommand => new RelayCommand(execute => InsertItem());
        public RelayCommand AddCommand => new RelayCommand(execute => AddItem());
        public RelayCommand UpdateCommand => new RelayCommand(execute => UpdateItem(), canExecute => CanUpdate());
        public RelayCommand DeleteCommand => new RelayCommand(execute => DeleteItem(), canExecute => CanDelete());
        public RelayCommand SearchCommand => new RelayCommand(execute => Search());
        public RelayCommand SettingCommand => new RelayCommand(execute => Setting());
        public string ItemName => SelectedItem?.Name ?? "Select an Item";
        public string SearchValue { get; set; } = string.Empty;
        private RamData _selectedItem;
        public RamData SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                _selectionService.SelectedRam = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemName));
                OnPropertyChanged(nameof(ItemDetails));
            }
        }
        private List<DetailItem> _itemDetails;
        public List<DetailItem> ItemDetails
        {
            get
            {
                if (SelectedItem == null) return new List<DetailItem>();

                var data = new List<DetailItem>();
                foreach (var (prop, value) in SelectedItem.GetProperties())
                {
                    var detail = new DetailItem();
                    if (prop.Name == "Name" || prop.Name == "id" || prop.Name == "BrandID") continue;
                    else
                    {
                        detail.Label = RegexHelper.SplitName(prop.Name);
                        detail.Value = $"{value}";
                    }
                    data.Add(detail);
                }
                return data;
            }
        }

        public SpreadSheetViewModel(IDatabaseService service, ISelectionService selectionService, IWindowFactory windowFactory, ICSVService cSVService)
        {
            _csvService = cSVService;
            _selectionService = selectionService;
            _databaseService = service;
            _databaseService.OnDataChanged += HandleDataChanged;
            _WindowFactory = windowFactory;

            RamDatas = new ObservableCollection<RamData>();
        }
        public void Initialize() => LoadData();
        private void HandleDataChanged(DataChangedEventArgs args)
        {
            LoadData();

            OnPropertyChanged(nameof(RamDatas));
            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(ItemName));
            OnPropertyChanged(nameof(ItemDetails));
        }

        private void LoadData()
        {
            var datas = _databaseService.GetRamDatas();
            RamDatas.Clear();
            foreach (var data in datas)
            {
                RamDatas.Add(data);
            }
        }
        private void InsertItem()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a file",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Multiselect = false
            };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filePath = dialog.FileName;
                var items = _csvService.CSVImport(filePath);
                try
                {
                    foreach (var item in items)
                    {
                        item.BrandID = _databaseService.BrandNameToID(item.Brand);
                    }
                    _databaseService.InsertCollectionToProduct("Products",items, "id", "Brand");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void AddItem()
        {
            var window = _WindowFactory.Create<AddWindow>();
            window.ShowDialog();
        }

        private void UpdateItem()
        {
            var window = _WindowFactory.Create<UpdateWindow>();
            window.ShowDialog();
        }
        private bool CanUpdate()
        {
            return SelectedItem != null;
        }

        private void DeleteItem()
        {
            try
            {
                _databaseService.DeleteFromTable("Products", "id = @id", new { id = SelectedItem.id });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDelete()
        {
            return SelectedItem != null;
        }

        private void Search()
        {
            try
            {
                var result = _databaseService.SearchFromTable<RamData>("Products", SearchValue);
                RamDatas.Clear();
                foreach (var item in result)
                {
                    RamDatas.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Setting()
        {
            var window = _WindowFactory.Create<SettingWindow>();
            window.ShowDialog();
        }
    }
}