using InventorySystem.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.Services
{
    public class WindowFactory : IWindowFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Create<T>() where T : class
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
