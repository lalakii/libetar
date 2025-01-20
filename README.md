# LibeTar
[![Available on NuGet https://www.nuget.org/packages?q=libetar](https://img.shields.io/nuget/v/libetar.svg?style=flat-square)](https://www.nuget.org/packages?q=libetar)

[libetar](https://www.nuget.org/packages?q=libetar) is a lightweight C# library for extracting TAR archives. It provides a simple API to extract all files from TAR archives.

## API
```cs
Tar.ExtractAll(Stream src, string outputDirectory, bool overrideIfExisting);
```

## Demo
[Demo.cs](https://github.com/lalakii/libetar/blob/master/Demo.cs)
```cs
using CN.Lalaki.Archive;
using System.IO;
using System;
// ...sample code
using (var tar = File.OpenRead("path\\of\\example.tar")) // tar file extract.
{
    Tar.ExtractAll(tar, "path\\of\\outputDir\\", true);
}

using (var targz = File.OpenRead("path\\of\\example.tar.gz")) // tar.gz file extract
{
    Tar.ExtractAll(targz, "path\\of\\outputDir\\", true);
}

using (var targz = new GzipStream(..., CompressionMode.Decompress)) // tar.gz stream extract
{
    Tar.ExtractAll(targz, "path\\of\\outputDir\\", true);
}
```
## License
[MIT](https://github.com/lalakii/libetar/blob/master/LICENSE)
