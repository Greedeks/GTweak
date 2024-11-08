using System.IO;
using System.IO.Compression;

namespace GTweak.Utilities.Helpers
{
    internal sealed class UnarchiveManager
    {
        internal UnarchiveManager(string path, byte[] resource)
        {
            byte[] fileSize = default;
            using (MemoryStream fileOut = new MemoryStream(resource))
            using (GZipStream gz = new GZipStream(fileOut, CompressionMode.Decompress))
            using (MemoryStream ms = new MemoryStream())
            {
                gz.CopyTo(ms);
                fileSize = ms.ToArray();
            }
            File.WriteAllBytes(path, fileSize);
        }
    }
}
