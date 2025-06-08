using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MimeTypeCore;

#if !MODERN
internal static partial class Extensions
{
    public static int IndexOf(this string source, char value, StringComparison comparisonType)
    {
        return source.IndexOf(value.ToString(), comparisonType);
    }
    
    public static int LastIndexOf(this string source, char value, StringComparison comparisonType)
    {
        return source.LastIndexOf(value.ToString(), comparisonType);
    }
    
    public static bool StartsWith(this string str, char value)
    {
        return str.Length > 0 && str[0] == value;
    }
    
    public static bool StartsWith(this string str, char value, StringComparison comparisonType)
    {
        if (str.Length == 0)
            return false;
            
        return string.Compare(str, 0, value.ToString(), 0, 1, comparisonType) == 0;
    }
    
    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
            return false;

        dictionary.Add(key, value);
        return true;
    }

    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
            return false;

        dictionary.Add(key, value);
        return true;
    }
    
    public static TValue? GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out TValue value) ? value : default;
    }
    
    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
    }
    
    public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out TValue value) ? value : default;
    }
    
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
    }
    
    public static double Clamp(this double value, double min, double max)
    {
        if (value < min)
            return min;
        return value > max ? max : value;
    }
    
    public static float Clamp(this float value, float min, float max)
    {
        if (value < min)
            return min;
        return value > max ? max : value;
    }
    
    public static int Clamp(this int value, int min, int max)
    {
        if (value < min)
            return min;
        return value > max ? max : value;
    }
    
    public static long Clamp(this long value, long min, long max)
    {
        if (value < min)
            return min;
        return value > max ? max : value;
    }
}
#endif