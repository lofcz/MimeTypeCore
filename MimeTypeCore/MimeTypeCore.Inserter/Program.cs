using System.Text;
using Newtonsoft.Json;

namespace MimeTypeCore.Inserter;

class Program
{
    static void Main(string[] args)
    {
        Dictionary<string, string> source = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("input.txt"))!;
        Dictionary<string, string> compareTo = MimeTypeMapMapping.Mappings;

        List<KeyValuePair<string, string>> add = [];
        
        foreach (KeyValuePair<string, string> str in source)
        {
            string key = $".{str.Key}";

            if (!compareTo.TryGetValue(key, out _))
            {
                add.Add(str);
            }
        }

        if (File.Exists("output.txt"))
        {
            File.Delete("output.txt");   
        }

        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<string, string> x in add)
        {
            sb.AppendLine($"{{ \".{x.Key}\", \"{x.Value}\" }},");
        }
        
        File.WriteAllText("output.txt", sb.ToString().Trim());
    }
}