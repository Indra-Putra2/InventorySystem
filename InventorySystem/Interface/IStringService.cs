using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySystem.Interface
{
    public interface IStringService
    {
        string ObjectToString<T>(T obj);
    }
}
