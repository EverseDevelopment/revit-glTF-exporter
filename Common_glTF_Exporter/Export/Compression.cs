namespace Common_glTF_Exporter.Export
{
    using System.Collections.Generic;
    using System.IO;
    using Common_glTF_Exporter.Windows.MainWindow;
    using dracowrapper;

    public class Compression
    {
        /// <summary>
        /// Run compression.
        /// </summary>
        /// <param name="preferences">preferences.</param>
        public static void Run(Preferences preferences)
        {
            switch (preferences.compression)
            {
                case CompressionEnum.ZIP:
                    ZIP.Compress(preferences);
                    break;
                case CompressionEnum.Draco:
                    Draco.Compress(preferences);
                    break;
                case CompressionEnum.Meshopt:
                    break;
                default:
                    break;
            }
        }
    }
}
