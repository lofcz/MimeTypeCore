namespace MimeTypeCore;

#if MODERN
internal static partial class Extensions
{
    public static int LastIndexOf(this string source, char value, StringComparison comparisonType)
    {
        return source.LastIndexOf(value.ToString(), comparisonType);
    }
}
#endif