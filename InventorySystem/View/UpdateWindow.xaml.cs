using InventorySystem.ViewModel.MainWindowViewModel;
using System.Windows;

namespace InventorySystem.View
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow(UpdateViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.RequestClose += () => this.Close();
        }
    }
}
