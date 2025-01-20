using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace CN.Lalaki.Archive.Demo
{
    public static class Demo
    {
        public static void Main()
        {
            const string outDir = "D:\\tmp";
            if (Directory.Exists(outDir))
            {
                Directory.Delete(outDir, true);
            }

            const string tarFile = "D:\\example.tar";
            const string tarGzFile = $"{tarFile}.gz";
            var watch = Stopwatch.StartNew();

            // tar file extract.
            using var tar = File.OpenRead(tarFile);
            Tar.ExtractAll(tar, outDir, true);

            // tar.gz stream extract.
            using var targzStream = new GZipStream(File.OpenRead(tarGzFile), CompressionMode.Decompress);
            Tar.ExtractAll(targzStream, outDir, false);

            // tar.gz file extract.
            using var targz = File.OpenRead(tarGzFile);
            Tar.ExtractAll(targz, outDir, true);
            watch.Stop();
            Console.WriteLine("Duration: {0} ms", watch.ElapsedMilliseconds);

            Console.ReadKey();
        }
    }
}