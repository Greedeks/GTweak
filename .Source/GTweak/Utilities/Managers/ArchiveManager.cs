using System;
using System.IO;
using System.IO.Compression;

namespace GTweak.Utilities.Managers
{
    internal static class ArchiveManager
    {
        internal static void Unarchive(string path, byte[] resource)
        {
            string folderDir = path.Remove(path.LastIndexOf(@"\", StringComparison.OrdinalIgnoreCase));

            if (!Directory.Exists(folderDir))
                Directory.CreateDirectory(folderDir);

            using MemoryStream fileOut = new MemoryStream(resource);
            using GZipStream gzipStream = new GZipStream(fileOut, CompressionMode.Decompress);
            using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);

            gzipStream.CopyTo(fileStream);
        }
    }
}
