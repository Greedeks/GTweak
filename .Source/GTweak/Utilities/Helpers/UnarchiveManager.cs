using System.IO;
using System.IO.Compression;

namespace GTweak.Utilities.Helpers
{
    internal sealed class UnarchiveManager
    {
        internal UnarchiveManager(in string path, in byte[] resource)
        {
            byte[] fileSize = default;
            using (MemoryStream fileOut = new MemoryStream(resource))
            using (GZipStream gz = new GZipStream(fileOut, CompressionMode.Decompress))
            using (MemoryStream ms = new MemoryStream())
            {
                gz.CopyTo(ms);
                fileSize = ms.ToArray();
            }

            string folderDir = path.Remove(path.LastIndexOf(@"\"));

            if (!Directory.Exists(folderDir))
                Directory.CreateDirectory(folderDir);

            File.WriteAllBytes(path, fileSize);
        }
    }
}
