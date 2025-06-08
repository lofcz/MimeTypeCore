namespace MimeTypeCore.Formatter;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class DictionaryFormatter
{
    // Interface for items within the dictionary content block (entries or comments)
    public interface IDictionaryContentItem
    {
        IEnumerable<string> GetLines();
        int OriginalIndex { get; set; } // To preserve original relative order of comments
    }

    // Represents a single dictionary entry
    public class DictionaryEntryItem : IDictionaryContentItem
    {
        public string Key { get; set; }
        public string FullLine { get; set; } // The original line including any same-line comments
        public string Indentation { get; set; } // The leading whitespace of the entry line
        public int OriginalIndex { get; set; }

        public IEnumerable<string> GetLines() => new[] { FullLine };
    }

    // Represents a block of comments or blank lines between dictionary entries
    public class CommentBlockItem : IDictionaryContentItem
    {
        public List<string> Lines { get; set; } = new List<string>();
        public int OriginalIndex { get; set; }

        public IEnumerable<string> GetLines() => Lines;
    }

    /// <summary>
    /// Reads a C# file, sorts dictionary entries within a specific Dictionary<string, string>
    /// declaration alphabetically by key, and overwrites the original file.
    /// Preserves all comments and their relative positions.
    /// </summary>
    /// <param name="filePath">The path to the C# file to format.</param>
    public static void SortDictionaryEntries(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File not found at '{filePath}'");
            return;
        }

        List<string> allLines = File.ReadAllLines(filePath).ToList();

        int dictionaryDeclarationLineIndex = -1;
        int dictionaryEndLineIndex = -1;
        int dictionaryContentStartLineIndex = -1; // The line with the opening '{'

        // 1. Find the start of the dictionary declaration and the opening brace '{'.
        for (int i = 0; i < allLines.Count; i++)
        {
            // Find the start of the declaration, which is consistent.
            if (allLines[i].Contains("public static readonly"))
            {
                dictionaryDeclarationLineIndex = i;

                // Now, find the opening brace '{' that follows the declaration.
                // It might be several lines down due to preprocessor directives.
                for (int j = i; j < allLines.Count; j++)
                {
                    if (allLines[j].Trim().StartsWith("{"))
                    {
                        dictionaryContentStartLineIndex = j;
                        break;
                    }
                }
                break; // Stop searching once the declaration is found.
            }
        }

        if (dictionaryDeclarationLineIndex == -1 || dictionaryContentStartLineIndex == -1)
        {
            Console.WriteLine("Error: Dictionary declaration or opening brace '{' not found.");
            return;
        }

        // 2. Find the end of the dictionary block (the line with '}' or ';').
        for (int i = dictionaryContentStartLineIndex; i < allLines.Count; i++)
        {
            string trimmedLine = allLines[i].Trim();
            // The closing line could be '};', '.ToFrozenDictionary();', or just ';'.
            // The most reliable signal is the semicolon ';'.
            if (trimmedLine.Contains(";"))
            {
                dictionaryEndLineIndex = i;
                break;
            }
        }

        if (dictionaryEndLineIndex == -1)
        {
            Console.WriteLine("Error: Dictionary end ';' not found after declaration.");
            return;
        }

        // 3. Extract lines before the dictionary content.
        List<string> linesBeforeDictionary = allLines.Take(dictionaryContentStartLineIndex + 1).ToList();

        // 4. Extract lines after the dictionary content.
        List<string> linesAfterDictionary = allLines.Skip(dictionaryEndLineIndex).ToList();

        // 5. Extract the actual content lines of the dictionary.
        List<string> dictionaryContentLines = allLines
            .Skip(dictionaryContentStartLineIndex + 1)
            .Take(dictionaryEndLineIndex - (dictionaryContentStartLineIndex + 1))
            .ToList();

        List<IDictionaryContentItem> parsedContentItems = new List<IDictionaryContentItem>();
        List<string> currentCommentLines = new List<string>();
        int itemCounter = 0; // Used to preserve the original relative order of comment blocks

        // 6. Parse the dictionary content lines into DictionaryEntryItem and CommentBlockItem objects
        foreach (string line in dictionaryContentLines)
        {
            string trimmedLine = line.TrimStart();

            // Check if the line is a dictionary entry:
            // - Starts with '{'
            // - Contains a quoted string (the key)
            // - Ends with '},' or '}' (indicating a complete entry line)
            if (trimmedLine.StartsWith("{") && trimmedLine.Contains("\"") && (trimmedLine.Contains("},") || trimmedLine.EndsWith("}")))
            {
                // If we were collecting comments, add them as a CommentBlockItem before the new entry
                if (currentCommentLines.Any())
                {
                    parsedContentItems.Add(new CommentBlockItem { Lines = new List<string>(currentCommentLines), OriginalIndex = itemCounter++ });
                    currentCommentLines.Clear();
                }

                // Extract the key using a regular expression
                // Matches '{', optional whitespace, '"', then captures anything until the next '"'
                Match match = Regex.Match(trimmedLine, @"\{\s*""(?<key>[^""]+)""");
                if (!match.Success)
                {
                    Console.WriteLine($"Warning: Could not extract key from line: '{line}'. This line will be treated as a comment/non-entry.");
                    currentCommentLines.Add(line); // Treat as a comment if key extraction fails
                    continue;
                }
                string key = match.Groups["key"].Value;

                // Determine the original indentation of the entry line
                string indentation = line.Substring(0, line.IndexOf('{'));

                parsedContentItems.Add(new DictionaryEntryItem { Key = key, FullLine = line, Indentation = indentation, OriginalIndex = itemCounter++ });
            }
            else
            {
                // If it's not an entry, it's a comment or blank line; add to the current comment block
                currentCommentLines.Add(line);
            }
        }

        // Add any trailing comments that were collected after the last entry
        if (currentCommentLines.Any())
        {
            parsedContentItems.Add(new CommentBlockItem { Lines = new List<string>(currentCommentLines), OriginalIndex = itemCounter++ });
        }

        // 7. Separate dictionary entries for sorting
        List<DictionaryEntryItem> dictionaryEntries = parsedContentItems.OfType<DictionaryEntryItem>().ToList();

        // 8. Sort the dictionary entries alphabetically by key
        // Using StringComparer.OrdinalIgnoreCase to match the dictionary's own comparer
        dictionaryEntries.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Key, b.Key));

        // 9. Reconstruct the dictionary content, preserving original comment block positions
        List<string> sortedDictionaryContentLines = new List<string>();
        int sortedEntryIndex = 0;

        // Iterate through the parsed items in their original order
        foreach (var item in parsedContentItems.OrderBy(x => x.OriginalIndex))
        {
            if (item is CommentBlockItem commentBlock)
            {
                // If it's a comment block, add its lines as they are
                sortedDictionaryContentLines.AddRange(commentBlock.GetLines());
            }
            else if (item is DictionaryEntryItem originalEntryPlaceholder)
            {
                // If it's an entry placeholder, replace it with the next entry from our sorted list
                if (sortedEntryIndex < dictionaryEntries.Count)
                {
                    sortedDictionaryContentLines.AddRange(dictionaryEntries[sortedEntryIndex].GetLines());
                    sortedEntryIndex++;
                }
            }
        }

        // 10. Assemble the final output lines
        List<string> finalOutputLines = new List<string>();
        finalOutputLines.AddRange(linesBeforeDictionary);
        finalOutputLines.AddRange(sortedDictionaryContentLines);
        finalOutputLines.AddRange(linesAfterDictionary);

        // 11. Write the formatted content back to the file
        try
        {
            File.WriteAllLines(filePath, finalOutputLines);
            Console.WriteLine($"Successfully sorted dictionary entries in '{filePath}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file '{filePath}': {ex.Message}");
        }
    }
}