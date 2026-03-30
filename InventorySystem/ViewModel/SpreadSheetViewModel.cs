using InventorySystem.BaseClass;
using InventorySystem.Interface;
using InventorySystem.Model;
using System.Collections.ObjectModel;

namespace InventorySystem.ViewModel
{
    public class SpreadSheetViewModel : ViewModelBase
    {
        private readonly IDatabaseService _service;

        public ObservableCollection<RamData> RamDatas { get; set; }

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
                foreach (var item in SelectedItem)
                {
                    var originalKey = item.Key.Replace(" ", "");
                    if (originalKey == "Name" || originalKey == "id" || originalKey == "BrandID") continue;
                    var detail = new DetailItem()
                    {
                        Label = item.Key,
                        Value = $"{item.Value}",
                    };

                    data.Add(detail);
                }
                return data;
            }
        }

        public SpreadSheetViewModel(IDatabaseService service)
        {
            _service = service;

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
    }
}