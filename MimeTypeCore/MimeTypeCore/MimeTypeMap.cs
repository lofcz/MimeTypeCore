using System;
#if MODERN
using System.Collections.Frozen;
#endif
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace MimeTypeCore;

/// <summary>
/// Mime type map.
/// </summary>
public static class MimeTypeMap
{
    private const char Dot = '.';
    private const char QuestionMark = '?';
#if MODERN
    private static FrozenDictionary<string, string> mappings = MimeTypeMapMapping.Mappings;
#else
    private static readonly Dictionary<string, string> mappings = MimeTypeMapMapping.Mappings;
#endif
    
    /// <summary>
    /// Adds custom extension-MIME pairs to the underlying dictionary.<br/>
    /// Note: On .NET 8+ the backing collection is a FrozenDictionary, calling this reconstructs the entire collection.
    /// </summary>
    /// <param name="pairs">For example: [{".png", "image/png"}].</param>
    public static void AddMimeTypes(IEnumerable<KeyValuePair<string, string>> pairs)
    {
#if MODERN
        Dictionary<string, string> newMappings = new Dictionary<string, string>(MimeTypeMapMapping.Mappings, StringComparer.OrdinalIgnoreCase);
#endif
        
        foreach (KeyValuePair<string, string> pair in pairs)
        {
            string key = pair.Key;
            string val = pair.Value;
            
            if (!key.StartsWith('.'))
            {
                key = $".{key.Trim()}";
            }

            key = key.ToLowerInvariant().Trim();
            val = val.ToLowerInvariant().Trim();
            
#if !MODERN
            MimeTypeMapMapping.Mappings[key] = val;
#else
            newMappings[key] = val;
#endif
        }
        
#if MODERN
        mappings = newMappings.ToFrozenDictionary();
#endif
    }
    
#if MODERN
    /// <summary>
    /// Tries to get the type of the MIME from the provided string (filename or extension).
    /// This method relies solely on the file extension and does not read file content.
    /// </summary>
    /// <param name="str">The filename or extension (e.g., "document.pdf" or "pdf").</param>
    /// <param name="mimeType">The variable to store the MIME type.</param>
    /// <returns>True if a MIME type was found for the extension, false otherwise.</returns>
    public static bool TryGetMimeType(string str, [NotNullWhen(true)] out string? mimeType)
#else 
    /// <summary>
    /// Tries to get the type of the MIME from the provided string (filename or extension).
    /// This method relies solely on the file extension and does not read file content.
    /// </summary>
    /// <param name="str">The filename or extension (e.g., "document.pdf" or "pdf").</param>
    /// <param name="mimeType">The variable to store the MIME type.</param>
    /// <returns>True if a MIME type was found for the extension, false otherwise.</returns>
    public static bool TryGetMimeType(string str, out string? mimeType)
#endif
    {
        int indexQuestionMark = str.IndexOf(QuestionMark, StringComparison.Ordinal);

        if (indexQuestionMark != -1)
        {
            str = str.Remove(indexQuestionMark);
        }

        string extension;

        if (!str.StartsWith(Dot))
        {
            int index = str.LastIndexOf(Dot, StringComparison.Ordinal);

            if (index != -1 && str.Length > index + 1)
            {
                extension = $"{Dot}{str.Substring(index + 1)}".ToLowerInvariant();
            }
            else
            {
                extension = Dot + str;
            }
        }
        else
        {
            int lastDotIndex = str.LastIndexOf('.');
            extension = $"{Dot}{str.Substring(lastDotIndex + 1)}".ToLowerInvariant();
        }

        return mappings.TryGetValue(extension, out mimeType);
    }

    /// <summary>
    /// Gets the type of the MIME from the provided string (filename or extension).
    /// This method relies solely on the file extension and does not read file content.
    /// </summary>
    /// <param name="str">The filename or extension.</param>
    /// <returns>The MIME type or "application/octet-stream" if not found.</returns>
    public static string? GetMimeType(string str)
    {
        TryGetMimeType(str, out string? result);
        return result;
    }

#if MODERN
    /// <summary>
    /// Tries to get the MIME type from a file stream, using magic bytes for more accurate detection and collision resolution.
    /// If magic bytes don't provide a definitive answer, it falls back to extension-based lookup.
    /// </summary>
    /// <param name="filename">Filename hint (e.g., "document.ts") to help resolve collisions, especially for ZIP-based formats or text files.</param>
    /// <param name="fileStream">The file stream. It will be read from its current position and then reset. The stream must support synchronous reading.</param>
    /// <param name="mimeType">The detected MIME type.</param>
    /// <returns>True if a MIME type was successfully determined, false otherwise.</returns>
    public static bool TryGetMimeType(string filename, Stream fileStream, [NotNullWhen(true)] out string? mimeType)
#else 
    /// <summary>
    /// Tries to get the MIME type from a file stream, using magic bytes for more accurate detection and collision resolution.
    /// If magic bytes don't provide a definitive answer, it falls back to extension-based lookup.
    /// </summary>
    /// <param name="filename">Filename hint (e.g., "document.ts") to help resolve collisions, especially for ZIP-based formats or text files.</param>
    /// <param name="fileStream">The file stream. It will be read from its current position and then reset. The stream must support synchronous reading.</param>
    /// <param name="mimeType">The detected MIME type.</param>
    /// <returns>True if a MIME type was successfully determined, false otherwise.</returns>
    public static bool TryGetMimeType(string filename, Stream fileStream, out string? mimeType)
#endif
    {
        if (!fileStream.CanRead)
        {
            return TryGetMimeType(filename, out mimeType);
        }
        
        if (!fileStream.CanSeek)
        {
            byte[] headerBytes = new byte[MagicByteDetector.MaxBytesToRead];
            int bytesRead = fileStream.Read(headerBytes, 0, headerBytes.Length);

            if (bytesRead == 0)
            {
                return TryGetMimeType(filename, out mimeType);
            }
            
            byte[] actualHeader = new byte[bytesRead];
            Buffer.BlockCopy(headerBytes, 0, actualHeader, 0, bytesRead);

            return ProcessMagicBytes(actualHeader, filename, out mimeType);
        }

        long originalPosition = fileStream.Position;

        try
        {
            byte[] headerBytes = new byte[MagicByteDetector.MaxBytesToRead];
            int bytesRead = fileStream.Read(headerBytes, 0, headerBytes.Length);

            if (bytesRead == 0)
            {
                return TryGetMimeType(filename, out mimeType);
            }

            byte[] actualHeader = new byte[bytesRead];
            Buffer.BlockCopy(headerBytes, 0, actualHeader, 0, bytesRead);

            return ProcessMagicBytes(actualHeader, filename, out mimeType);
        }
        finally
        {
            fileStream.Position = originalPosition;
        }
    }

    /// <summary>
    /// Tries to get the MIME type from a file stream, using magic bytes for more accurate detection and collision resolution.
    /// If magic bytes don't provide a definitive answer, it falls back to extension-based lookup.
    /// </summary>
    /// <param name="filename">Filename hint (e.g., "document.ts") to help resolve collisions, especially for ZIP-based formats or text files.</param>
    /// <param name="fileStream">The file stream. It will be read from its current position and then reset.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>MIME type if detected, otherwise null.</returns>
    public static async Task<string?> TryGetMimeTypeAsync(string filename, Stream fileStream, CancellationToken token = default)
    {
        string? mimeType;

        if (!fileStream.CanRead)
        {
            TryGetMimeType(filename, out mimeType);
            return mimeType;
        }
        
        if (!fileStream.CanSeek)
        {
            byte[] headerBytes = new byte[MagicByteDetector.MaxBytesToRead];
#if NET40
            int bytesRead = fileStream.Read(headerBytes, 0, headerBytes.Length);
#elif NET8_0_OR_GREATER
            int bytesRead = await fileStream.ReadAsync(headerBytes, token);
#else
            int bytesRead = await fileStream.ReadAsync(headerBytes, 0, headerBytes.Length, token);        
#endif

            if (bytesRead == 0)
            {
                TryGetMimeType(filename, out mimeType);
                return mimeType;
            }
            
            byte[] actualHeader = new byte[bytesRead];
            Buffer.BlockCopy(headerBytes, 0, actualHeader, 0, bytesRead);

            ProcessMagicBytes(actualHeader, filename, out mimeType);
            return mimeType;
        }

        long originalPosition = fileStream.Position;

        try
        {
            byte[] headerBytes = new byte[MagicByteDetector.MaxBytesToRead];
#if NET40
            int bytesRead = fileStream.Read(headerBytes, 0, headerBytes.Length);
#elif NET8_0_OR_GREATER
            int bytesRead = await fileStream.ReadAsync(headerBytes, token);
#else
            int bytesRead = await fileStream.ReadAsync(headerBytes, 0, headerBytes.Length, token);        
#endif

            if (bytesRead == 0)
            {
                TryGetMimeType(filename, out mimeType);
                return mimeType;
            }

            byte[] actualHeader = new byte[bytesRead];
            Buffer.BlockCopy(headerBytes, 0, actualHeader, 0, bytesRead);

            ProcessMagicBytes(actualHeader, filename, out mimeType);
            return mimeType;
        }
        finally
        {
            fileStream.Position = originalPosition;
        }
    }
    
    private static bool ProcessMagicBytes(byte[] headerBytes, string filename, out string? mimeType)
    {
        List<Info> magicByteMatches = MagicByteDetector.Detect(headerBytes);

        if (magicByteMatches.Count == 0)
        {
            return TryGetMimeType(filename, out mimeType);
        }

        string? preferredExtension = GetFileExtension(filename);
        Info? bestMatch = SelectBestMatch(magicByteMatches, preferredExtension);

        if (bestMatch?.Mime == null)
        {
            return TryGetMimeType(filename, out mimeType);
        }

        mimeType = bestMatch.Mime;
        return true;
    }
    
    private static string? GetFileExtension(string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            return null;
        }

        int lastDotIndex = filename.LastIndexOf('.');
        
        if (lastDotIndex != -1 && lastDotIndex < filename.Length - 1)
        {
            return filename.Substring(lastDotIndex + 1).ToLowerInvariant();
        }

        return null;
    }
    
    private static Info? SelectBestMatch(List<Info> magicByteMatches, string? preferredExtension)
    {
        Info? bestMatch = null;
        
        if (!string.IsNullOrEmpty(preferredExtension))
        {
            bestMatch = magicByteMatches.FirstOrDefault(m => 
                m.Extension != null && 
                m.Extension.Equals(preferredExtension, StringComparison.OrdinalIgnoreCase));
        }

        if (bestMatch != null)
        {
            return bestMatch;
        }
        
        if (magicByteMatches.Count > 1)
        {
            // PNG vs APNG
            if (magicByteMatches.Any(m => m.TypeName == "png") && magicByteMatches.Any(m => m.TypeName == "apng"))
            {
                return magicByteMatches.FirstOrDefault(m => 
                           m.TypeName == "apng" && preferredExtension == "apng") ?? 
                       magicByteMatches.First(m => m.TypeName == "png");
            }

            // ZIP-based (docx, xlsx, jar, odt, PK\x03\x04)
            if (magicByteMatches.Any(m => m.TypeName == "zip"))
            {
                return magicByteMatches.FirstOrDefault(m => 
                           m.Extension != null && 
                           preferredExtension != null && 
                           m.Extension.Equals(preferredExtension, StringComparison.OrdinalIgnoreCase)) ?? 
                       magicByteMatches.FirstOrDefault(m => m.TypeName == "zip");
            }
            
            return magicByteMatches.First();
        }
        
        return magicByteMatches.FirstOrDefault();
    }

#if MODERN
    /// <summary>
    /// Gets the MIME type from a file stream, using magic bytes for more accurate detection and collision resolution.
    /// </summary>
    /// <param name="fileStream">The file stream. It will be read from its current position and then reset.</param>
    /// <param name="mimeType">The detected MIME type.</param>
    /// <returns>True if a MIME type was successfully determined, false otherwise.</returns>
    public static bool TryGetMimeType(Stream fileStream, [NotNullWhen(true)] out string? mimeType)
#else
    /// <summary>
    /// Gets the MIME type from a file stream, using magic bytes for more accurate detection and collision resolution.
    /// </summary>
    /// <param name="fileStream">The file stream. It will be read from its current position and then reset.</param>
    /// <param name="mimeType">The detected MIME type.</param>
    /// <returns>True if a MIME type was successfully determined, false otherwise.</returns>
    public static bool TryGetMimeType(Stream fileStream, out string? mimeType)
#endif
    {
        string fileName = string.Empty;
        
        #if NETSTANDARD2_0_OR_GREATER || NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER
        if (fileStream is FileStream fs)
        {
            fileName = fs.Name;
        }
        #endif
        
        return TryGetMimeType(fileName, fileStream, out mimeType);
    }
    
#if MODERN
    /// <summary>
    /// Gets the extension from the provided MIME type.
    /// </summary>
    /// <param name="mimeType">Type of the MIME.</param>
    /// <param name="extension">Extension of the file.</param>
    /// <returns>The extension.</returns>
    public static bool TryGetExtension(string mimeType, [NotNullWhen(true)] out string? extension)
#else 
    /// <summary>
    /// Gets the extension from the provided MIME type.
    /// </summary>
    /// <param name="mimeType">Type of the MIME.</param>
    /// <param name="extension">Extension of the file.</param>
    /// <returns>The extension.</returns>
    public static bool TryGetExtension(string mimeType, out string? extension)
#endif
    {
#if !MODERN
        mimeType = mimeType.ToLowerInvariant();        
#endif
        
        return mappings.TryGetValue(mimeType, out extension);
    }
}