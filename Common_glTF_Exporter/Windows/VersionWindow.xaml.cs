using Autodesk.Revit.UI;
using Common_glTF_Exporter.Utils;
using Common_glTF_Exporter.Version;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common_glTF_Exporter;

namespace Revit_glTF_Exporter
{
    /// <summary>
    /// Interaction logic for VersionWindow.xaml
    /// </summary>
    public partial class VersionWindow : Window
    {
        public static string _fileVersion { get; set; }
        public static string _installerName { get; set; }
        public VersionWindow(string fileVersion)
        {
            _installerName = fileVersion + ".msi";
            _fileVersion = fileVersion;
            InitializeComponent();
            labelMessage.Text = labelMessage.Text + fileVersion + " is Ready";
            Theme.ApplyDarkLightMode(this.Resources.MergedDictionaries[0]);
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            this.Hide();

            string versionFolder = VersionFolder();
            await DownloadFile.FromServer(versionFolder, _installerName);

            string fileLocation = System.IO.Path.Combine(versionFolder, _installerName);;
            RunLocalFile.Action(fileLocation);

            Close();
        }


        private async void Button_UpdateOnExit(object sender, RoutedEventArgs e)
        {
            this.Hide();
            string versionFolder = VersionFolder();
            await DownloadFile.FromServer(versionFolder, _installerName);

            string fileLocation = System.IO.Path.Combine(versionFolder, _installerName);
            ExternalApplication.InstallerRoute = fileLocation;
            Close();
        }

        private static string VersionFolder()
        {
            string tempPath = Path.GetTempPath();
            string tempFolder = System.IO.Path.Combine(tempPath, "LeiaInstaller");
            string versionFolder = System.IO.Path.Combine(tempFolder, _fileVersion);
            DirectoryUtils.CreateDirectoryIfNotExists(versionFolder);
            DirectoryUtils.DeleteFilesInDirectoyy(versionFolder);
            return versionFolder;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Version_Link(object sender, RoutedEventArgs e)
        {
            Hyperlink.Run(Links.notionLink);
        }

        private void UpdatesNotes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Process.Start(DocumentationHelper.BuildUri("/Release-Notes-227f11dd61ec4bafa7c432f29f01f31b"));
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
