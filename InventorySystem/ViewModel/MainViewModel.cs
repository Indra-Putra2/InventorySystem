using InventorySystem.Interface;

namespace InventorySystem.ViewModel
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

            _service.InitializeDatabase();
        }
    }
}
