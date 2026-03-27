using InventorySystem.Interface;
using InventorySystem.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InventorySystem.ViewModel
{
    public class SpreadSheetViewModel : INotifyPropertyChanged
    {
        private readonly IDatabaseService _service;

        public ObservableCollection<RamData> RamDatas { get; set; }
        public List<DetailItem> ItemDetails { get; set; }

        private RamData _selectedItem;
        public RamData SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        private string _itemName;
        public string ItemName
        {
            get => _itemName;
            set
            {
                _itemName = value;
                OnPropertyChanged();
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}