using System.IO;

namespace Common_glTF_Exporter.Utils
{
    public static class DirectoryUtils
    {
        /// <summary>
        /// Create Directory if it does not exists
        /// </summary>
        public static bool CreateDirectoryIfNotExists(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Delete Directory if it does exists
        /// </summary>
        public static void DeleteDirectoryIfExists(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }

        public static void DeleteFilesInDirectoyy(string path)
        {
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }
}
