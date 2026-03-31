namespace InventorySystem.Interface
{
    public interface IWindowFactory
    {
        T Create<T>() where T : class;
    }
}
