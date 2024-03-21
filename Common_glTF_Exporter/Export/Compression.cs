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
        public static void Run(Preferences preferences, Common_glTF_Exporter.ViewModel.ProgressBarWindowViewModel progressBar)
        {
            switch (preferences.compression)
            {
                case CompressionEnum.ZIP:
                    progressBar.Message = "Compressing to ZIP";
                    ZIP.Compress(preferences);
                    break;
                case CompressionEnum.Draco:
                    progressBar.Message = "Compressing to Draco";
                    Draco.Compress(preferences);
                    break;
                case CompressionEnum.Meshopt:
                    progressBar.Message = "Compressing to MeshOpt";
                    MeshOpt.Compress(preferences);
                    break;
                default:
                    break;
            }
        }
    }
}
