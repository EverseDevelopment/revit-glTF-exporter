namespace Ductulator
{
    using System.Windows;

    /// <summary>
    /// Resources.
    /// </summary>
    public partial class Resources : ResourceDictionary
    {
        private void EngworksLink(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://e-verse.com/");
        }

        private void AddInLink(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://e-verse.com");
        }
    }
}
