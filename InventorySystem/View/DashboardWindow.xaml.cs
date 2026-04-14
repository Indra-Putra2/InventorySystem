using InventorySystem.ViewModel.MainWindowViewModel;
using System.Windows;

namespace InventorySystem.View
{
    /// <summary>
    /// Interaction logic for DashboardWindow.xaml
    /// </summary>
    public partial class DashboardWindow : Window
    {
        private DashboardViewModel _vm;

        public DashboardWindow(DashboardViewModel vm)
        {
            _vm = vm;
            InitializeComponent();
            DataContext = vm;

            this.StateChanged += HandleStateChanged;
        }

        private void HandleStateChanged(object? sender, EventArgs e)
        {
            _vm.IsMaximized = this.WindowState == WindowState.Maximized;
        }
    }
}
