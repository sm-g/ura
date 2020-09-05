using System.Collections.Generic;

namespace Ura
{
    public static class DictionaryExtensions
    {
        public static bool GetBoolValueOrFalse<T>(this IDictionary<T, bool> dict, T key)
        {
            bool result;
            if (dict.TryGetValue(key, out result))
                return result;
            else
                return false;
        }
    }
}