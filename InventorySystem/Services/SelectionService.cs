using InventorySystem.Model;
using InventorySystem.Interface;

namespace InventorySystem.Services
{
    public class SelectionService : ISelectionService
    {
        private RamData _selectedRam;
        public RamData SelectedRam
        {
            get => _selectedRam;
            set
            {
                _selectedRam = value;
                OnSelectionChanged?.Invoke();
            }
        }
        public event Action? OnSelectionChanged;
    }
}
