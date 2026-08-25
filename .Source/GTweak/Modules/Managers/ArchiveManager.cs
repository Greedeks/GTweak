using System.IO;
using System.IO.Compression;
using GTweak.Modules.Helpers;

namespace GTweak.Modules.Managers
{
    internal static class ArchiveManager
    {
        internal static void Unarchive(string path, byte[] resource)
        {
            FileDirectoryHelper.CreateDirectory(Path.GetDirectoryName(path));

            using MemoryStream fileOut = new MemoryStream(resource);
            using GZipStream gzipStream = new GZipStream(fileOut, CompressionMode.Decompress);
            using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);

            gzipStream.CopyTo(fileStream);
        }
    }
}
