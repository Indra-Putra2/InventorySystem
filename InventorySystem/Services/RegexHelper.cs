using System.Text.RegularExpressions;

namespace InventorySystem.Services
{
    public static class RegexHelper
    {
        public static string SplitName(string name)
        {
            return Regex.Replace(name, @"(\b[a-z]+|(?<=[a-z])[A-Z]|(?<=[A-Z])[A-Z](?=[a-z]))", " $1").Trim();
        }
    }
}
