using System.Windows;
using InventorySystem.ViewModel.MainWindowViewModel;

namespace InventorySystem.View
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow(SettingViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            vm.RequestClose += () => this.Close();
        }
    }
}
