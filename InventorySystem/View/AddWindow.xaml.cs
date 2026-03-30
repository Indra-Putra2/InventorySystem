using InventorySystem.ViewModel;
using System.Windows;

namespace InventorySystem.View
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        public AddWindow(AddViewModel addViewModel)
        {
            InitializeComponent();
            DataContext = addViewModel;
        }
    }
}
