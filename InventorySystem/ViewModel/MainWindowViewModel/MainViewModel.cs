using InventorySystem.Interface;

namespace InventorySystem.ViewModel.MainWindowViewModel
{
    public class MainViewModel
    {
        public SpreadSheetViewModel SpreadSheetVM { get; set; }
        public DashboardViewModel DashboardVM { get; set; }
        private IDatabaseService _service;

        public MainViewModel(IDatabaseService service, SpreadSheetViewModel spreadSheet, DashboardViewModel dashboard)
        {
            _service = service;
            SpreadSheetVM = spreadSheet;
            DashboardVM = dashboard;
            if (_service.InitializeDatabase())
            {
                spreadSheet.Initialize();
            }
        }

        
    }
}
