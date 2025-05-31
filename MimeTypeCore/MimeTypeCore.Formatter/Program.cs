namespace MimeTypeCore.Formatter;

class Program
{
    static void Main(string[] args)
    {
        string path = "../../../../MimeTypeCore/MimeTypeMap.cs";
        
        if (File.Exists(path))
        {
            DictionaryFormatter.SortDictionaryEntries(path);   
        }
    }
}