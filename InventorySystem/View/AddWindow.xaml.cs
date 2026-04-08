using InventorySystem.ViewModel.MainWindowViewModel;
using System.Windows;

namespace InventorySystem.View
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {

        public AddWindow(AddViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;

            vm.AddRamVM.RequestClose += () => this.Close();
            vm.AddBrandVM.RequestClose += () => this.Close();
        }
    }
}
