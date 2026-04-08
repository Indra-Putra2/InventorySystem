using InventorySystem.ViewModel.AddWindowViewModel;

namespace InventorySystem.ViewModel.MainWindowViewModel
{
    public class AddViewModel
    {
        public AddRamViewModel AddRamVM { get; set; }
        
        public AddViewModel(AddRamViewModel addRam)
        {
            AddRamVM = addRam;
        }
    }

}
