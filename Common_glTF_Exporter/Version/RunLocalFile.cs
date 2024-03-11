using System.IO;

namespace Common_glTF_Exporter.Version
{
    public static class RunLocalFile
    {
        public static void Action(string pathFile)
        {
            if (File.Exists(pathFile))
            {
                System.Diagnostics.Process.Start(pathFile);
            }
        }
    }
}
