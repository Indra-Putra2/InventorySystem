using InventorySystem.BaseClass;
using InventorySystem.Interface;
using InventorySystem.Model;
using InventorySystem.Services;
using InventorySystem.View;
using System.Collections.ObjectModel;

namespace InventorySystem.ViewModel
{
    public class SpreadSheetViewModel : ViewModelBase
    {
        private readonly IDatabaseService _service;
        private readonly IWindowFactory _WindowFactory;

        public ObservableCollection<RamData> RamDatas { get; set; }
        public RelayCommand AddCommand => new RelayCommand(execute => AddItem());
        public string ItemName => SelectedItem?.Name ?? "Select an Item";

        private RamData _selectedItem;
        public RamData SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
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
                foreach (var (prop, value) in SelectedItem)
                {
                    if (prop.Name == "Name" || prop.Name == "id" || prop.Name == "BrandID") continue;
                    var detail = new DetailItem()
                    {
                        Label = RegexHelper.SplitName(prop.Name),
                        Value = $"{value}",
                    };

                    data.Add(detail);
                }
                return data;
            }
        }

        public SpreadSheetViewModel(IDatabaseService service, IWindowFactory windowFactory)
        {
            _service = service;
            _WindowFactory = windowFactory;

            RamDatas = new ObservableCollection<RamData>();
            LoadData();
        }

        private void LoadData()
        {
            var datas = _service.GetRamDatas();

            foreach (var data in datas)
            {
                RamDatas.Add(data);
            }
        }

        private void AddItem()
        {
            var window = _WindowFactory.Create<AddWindow>();
            window.ShowDialog();
        }
    }
}