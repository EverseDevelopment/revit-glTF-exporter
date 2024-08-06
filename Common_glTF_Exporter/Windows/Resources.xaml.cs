namespace Revit_glTF_Exporter
{
    using System.Windows;
    using Common_glTF_Exporter;
    using Common_glTF_Exporter.Utils;

    /// <summary>
    /// Resources.
    /// </summary>
    public partial class Resources : ResourceDictionary
    {
        private void EngworksLink(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Hyperlink.Run(Links.everseWebsite);
        }

        private void AddInLink(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Hyperlink.Run("https://e-verse.com");
        }
    }
}
