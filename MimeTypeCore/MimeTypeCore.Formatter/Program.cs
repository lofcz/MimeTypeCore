namespace MimeTypeCore.Formatter;

class Program
{
    static void Main(string[] args)
    {
        string path = "../../../../MimeTypeCore/MimeTypeMapMapping.cs";
        
        if (File.Exists(path))
        {
            DictionaryFormatter.SortDictionaryEntries(path);   
        }
    }
}