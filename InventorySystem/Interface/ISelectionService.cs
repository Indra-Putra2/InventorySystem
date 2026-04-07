using InventorySystem.Model;

namespace InventorySystem.Interface
{
    public interface ISelectionService
    {
        RamData SelectedRam { get; set; }
        event Action OnSelectionChanged;
    }
}
