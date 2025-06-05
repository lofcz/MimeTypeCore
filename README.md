[![MimeTypeCore](https://badgen.net/nuget/v/MimeTypeCore?v=303&icon=nuget&label=MimeTypeCore)](https://www.nuget.org/packages/MimeTypeCore)

# MimeTypeCore

<img align="left" width="128" height="128" alt="Te Reo Icon" src="https://github.com/user-attachments/assets/250152a4-cbcd-409b-9290-36a2dd7c77f8" />
Fast MIME type mapping library for the .NET ecosystem. Supports almost any Core and Framework version, including <code>netstandard1.2</code>, <code>net40</code>, and <code>net8.0</code>. Extensively tested, focused on performance, and <i>working out of the box</i>. Get your <code>MIME</code> type or extension and be done with it <i>fast</i>. The mapping is zero-config by default and sourced from authoritative sources, such as <a href="https://www.iana.org/assignments/media-types/media-types.xhtml">iana</a>, and <a href="https://mimetype.io/all-types">mimetype</a>. About 1,500 extensions and MIME types are mapped. MimeTypeCore builds upon <a href="https://github.com/samuelneff/MimeTypeMap">MimeTypeMap</a>.

<br/><br/>

## Getting Started

Install the package via NuGet:

```powershell
dotnet add package MimeTypeCore
```

Get `MIME` type from an extension, or vice versa:

```csharp
using MimeTypeCore;
MimeTypeMap.TryGetMimeType(".png", out string mimeTypePng); // image/png
MimeTypeMap.TryGetExtension("image/png", out string extension); // .png
```

⭐ **That's it!** Please consider starring this repository if you find it helpful.

## Collisions

Sometimes, one extension can have multiple `MIME` types associated. For example, `.ts` might be `text/typescript`, or `video/mpeg` (`ts` stands for Transport Stream in this case). To resolve the collision, provide `Stream` to the file, so the header can be sampled for a known sequence of magic bytes:
```csharp
using FileStream streamVideo = File.Open("files/video.ts", FileMode.Open);
using FileStream streamTypescript = File.Open("files/typescript.ts", FileMode.Open);

MimeTypeMap.TryGetMimeType(streamVideo, out string mimeTypeVideo); // video/mpeg
MimeTypeMap.TryGetMimeType(streamTypescript, out string mimeTypeTypescript); // text/typescript
```

## Browser

When dealing with user-provided files, either from Blazor or MVC, your input is probably `IBrowserFile` or `IFormFile`. These streams don't support synchronous reading, use `MimeTypeMap.TryGetMimeTypeAsync`:
```cs
IBrowserFile file; // for example, from InputFileChangeEventArgs

try
{
    // 500 MB, use reasonable limits out there
    await using Stream stream = file.OpenReadStream(512_000_000);

    // null if not recognized, your MIME type otherwise
    string? mimeType = await MimeTypeMap.TryGetMimeTypeAsync(args.File.Name, stream);
}
catch (Exception e) // for example, the file size is over the OpenReadStream limit, etc.
{
    
}
```

## Contributing

To contribute, check the [mapping](https://github.com/lofcz/MimeTypeCore/blob/master/MimeTypeCore/MimeTypeCore/MimeTypeMapMapping.cs) file for the hardcoded mappings, and add new entries. Please follow the code style and alphabetical ordering. Magic headers can be contributed to [this](https://github.com/lofcz/MimeTypeCore/blob/master/MimeTypeCore/MimeTypeCore/MimeTypeMapMagicBytes.cs) file. If you are touching anything beyond that, provide relevant [test cases](https://github.com/lofcz/MimeTypeCore/tree/master/MimeTypeCore/MimeTypeCore.Tests). Thank you.


## License

This library is licensed under the [MIT](https://github.com/lofcz/FastCloner/blob/next/LICENSE) license. 💜
