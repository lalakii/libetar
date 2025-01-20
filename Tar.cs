using System.IO;
using System.IO.Compression;
using static System.Convert;
using static System.Text.Encoding;

namespace CN.Lalaki.Archive
{
    public static class Tar
    {
        private const int BufSize = 40960;

        private enum TypeFlag
        {
            AREGTYPE = '\0',
            REGTYPE = '0',
            LNKTYPE = '1',
            SYMTYPE = '2',
            CHRTYPE = '3',
            BLKTYPE = '4',
            DIRTYPE = '5',
            FIFOTYPE = '6',
            CONTTYPE = '7',
            XGLTYPE = 'g',
            XHDTYPE = 'x',
        }

        public static void ExtractAll(Stream ts, string outDir, bool mOverride)
        {
            if (!ts.CanRead)
            {
                throw new IOException("Unable to read stream.");
            }

            if (string.IsNullOrWhiteSpace(outDir))
            {
                throw new IOException("Output directory cannot be null or empty.");
            }

            FileStream fs = null;
            if (ts is not GZipStream gz)
            {
                var pos = ts.Position;
                var tarGz = ts.ReadByte() == 0x1F && ts.ReadByte() == 0x8B;
                ts.Position = pos;
                if (tarGz)
                {
                    gz = new(ts, CompressionMode.Decompress);
                    fs = CreateTempFile();
                    gz.CopyTo(fs);
                    fs.Position = 0L;
                    gz.Dispose();
                    ts.Dispose();
                    ts = fs;
                }
            }
            else
            {
                ts = fs = CreateTempFile();
                gz.CopyTo(fs);
                ts.Position = 0L;
                gz.Dispose();
            }

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            var buf = new byte[BufSize];
            while (true)
            {
                int bytesRead = ts.Read(buf, 0, 100);
                if (bytesRead != 100)
                {
                    break;
                }

                var fileName = UTF8.GetString(buf, 0, 100).TrimEnd('\0');
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    break;
                }

                bytesRead = ts.Read(buf, 100, 400);
                if (bytesRead != 400)
                {
                    break;
                }

                uint cksum = ToUInt32(ASCII.GetString(buf, 148, 8).TrimEnd('\0'), 8);
                uint vcksum = 256U;
                for (uint i = 0U; i < 148U; i++)
                {
                    vcksum += buf[i];
                }

                for (uint i = 155U; i < 500U; i++)
                {
                    vcksum += buf[i];
                }

                if (cksum != vcksum)
                {
                    ReleaseStreams(ts, fs);
                    throw new IOException("Checksum mismatch.");
                }

                ts.Position += 12L;
                var fileSize = ToInt64(ASCII.GetString(buf, 124, 12).TrimEnd('\0'), 8);
                var type = (TypeFlag)buf[156];
                var prefix = UTF8.GetString(buf, 345, 155).TrimEnd('\0');
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    fileName = Path.Combine(prefix, fileName);
                }

                var output = Path.Combine(outDir, fileName);
                switch (type)
                {
                    case TypeFlag.REGTYPE:
                    case TypeFlag.AREGTYPE:
                        if (!File.Exists(output) || mOverride)
                        {
                            using FileStream os = new(output, FileMode.Create, FileAccess.Write);
                            long total = 0;
                            int endOp = (int)(fileSize % BufSize);
                            int pos = BufSize;
                            while (total < fileSize)
                            {
                                if (total + pos > fileSize)
                                {
                                    pos = endOp;
                                }

                                bytesRead = ts.Read(buf, 0, pos);
                                os.Write(buf, 0, bytesRead);
                                total += bytesRead;
                            }

                            if (total != fileSize)
                            {
                                ReleaseStreams(ts, fs);
                                throw new IOException("File length mismatch.");
                            }
                        }
                        else
                        {
                            ts.Position += fileSize;
                        }

                        break;

                    case TypeFlag.SYMTYPE:
                        break;

                    case TypeFlag.DIRTYPE:
                        var dir = Path.GetDirectoryName(output);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        break;

                    case TypeFlag.XGLTYPE:
                        ts.Position += fileSize;
                        break;
                }

                ts.Position += 512L - ts.Position & 511L;
            }

            ReleaseStreams(ts, fs);
        }

        private static FileStream CreateTempFile()
        {
            return File.Create(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        }

        private static void ReleaseStreams(Stream ts, Stream fs)
        {
            if (ts == fs)
            {
                fs.SetLength(0L);
            }

            ts.Dispose();
        }
    }
}