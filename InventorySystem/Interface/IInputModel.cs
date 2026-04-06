namespace InventorySystem.Interface
{
    public interface IInputModel
    {
        string Key { get; }
        string Value { get; }
        bool IsReady { get; }
    }
}
