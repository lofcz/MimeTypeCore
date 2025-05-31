[![MimeTypeCore](https://badgen.net/nuget/v/MimeTypeCore?v=303&icon=nuget&label=MimeTypeCore)](https://www.nuget.org/packages/MimeTypeCore)

# MimeTypeCore

<img align="left" width="128" height="128" alt="Te Reo Icon" src="https://github.com/user-attachments/assets/250152a4-cbcd-409b-9290-36a2dd7c77f8" />
Fast MIME type mapping library for the .NET ecosystem. Supports almost any Core and Framework version, including <code>netstandard1.2</code>, <code>net40</code>, and <code>net8.0</code>. Extensively tested, focused on performance and stability even on complicated object graphs. FastCloner is designed to work with as few gotchas as possible out of the box. The mapping is zero-config by default. Clone your objects and be done with it <em>fast</em>. FastCloner builds upon <a href="https://github.com/force-net/DeepCloner">DeepClone</a>.

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

Sometimes, one extension can have multiple `MIME` types associated with it. For example, `.ts` might be `text/typescript`, or `video/mpeg` (`ts` stands for Transport Stream in this case). To resolve the collision, provide `Stream` to the file, so the header can be sampled for a known sequence of magic bytes:
```csharp
using FileStream streamVideo = File.Open("files/video.ts", FileMode.Open);
using FileStream streamTypescript = File.Open("files/typescript.ts", FileMode.Open);

MimeTypeMap.TryGetMimeType(streamVideo, out string mimeTypeVideo); // video/mpeg
MimeTypeMap.TryGetMimeType(streamTypescript, out string mimeTypeTypescript); // text/typescript
```

## Contributing

To contribute, check the [mapping](https://github.com/lofcz/MimeTypeCore/blob/master/MimeTypeCore/MimeTypeCore/MimeTypeMapMapping.cs) file for the hardcoded mappings, and add new entries. Please follow the code style and alphabetical ordering. Magic headers can be contributed to [this](https://github.com/lofcz/MimeTypeCore/blob/master/MimeTypeCore/MimeTypeCore/MimeTypeMapMagicBytes.cs) file.


## License

This library is licensed under the [MIT](https://github.com/lofcz/FastCloner/blob/next/LICENSE) license. 💜
