using InventorySystem.ViewModel.AddWindowViewModel;

namespace InventorySystem.ViewModel.MainWindowViewModel
{
    public class AddViewModel
    {
        public AddRamViewModel AddRamVM { get; set; }
        public AddBrandViewModel AddBrandVM { get; set; }

        public AddViewModel(AddRamViewModel addRam, AddBrandViewModel addBrand)
        {
            AddRamVM = addRam;
            AddBrandVM = addBrand;
        }
    }

}
