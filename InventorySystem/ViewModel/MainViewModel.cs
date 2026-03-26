namespace InventorySystem.ViewModel
{
    internal class MainViewModel
    {
        public SpreadSheetViewModel SpreadSheetVM { get; set; } = new SpreadSheetViewModel();
        public DashboardViewModel DashboardVM { get; set; } = new DashboardViewModel();
    }
}
