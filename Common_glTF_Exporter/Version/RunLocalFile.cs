using System.IO;
using Common_glTF_Exporter.Utils;

namespace Common_glTF_Exporter.Version
{
    public static class RunLocalFile
    {
        public static void Action(string pathFile)
        {
            if (File.Exists(pathFile))
            {
                Hyperlink.Run(pathFile);
            }
        }
    }
}
