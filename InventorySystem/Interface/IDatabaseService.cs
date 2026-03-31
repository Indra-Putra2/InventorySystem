using InventorySystem.Model;

namespace InventorySystem.Interface
{
    public interface IDatabaseService
    {
        void InitializeDatabase();
        List<RamData> GetRamDatas();
        List<BrandData> GetBrandDatas();
    }
}
