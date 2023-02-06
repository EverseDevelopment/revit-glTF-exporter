using System;
using WixSharp;
using WixSharp.Nsis;

namespace GltfInstaller
{
    internal class Program
    {
        static void Main()
        {
            var project = new ManagedProject("Leia glTF exporter",
                              new Dir(@"%ProgramFiles%\My Company\My Product",
                                  new File("Program.cs")));

            project.GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b");

            // project.ManagedUI = ManagedUI.DefaultWpf; // all stock UI dialogs

            //custom set of UI WPF dialogs
            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs.Add<GltfInstaller.WelcomeDialog>()
                                            .Add<GltfInstaller.LicenceDialog>()
                                            .Add<GltfInstaller.ProgressDialog>()
                                            .Add<GltfInstaller.ExitDialog>();

            project.ManagedUI.ModifyDialogs.Add<GltfInstaller.MaintenanceTypeDialog>()
                                           .Add<GltfInstaller.FeaturesDialog>()
                                           .Add<GltfInstaller.ProgressDialog>()
                                           .Add<GltfInstaller.ExitDialog>();

            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";

            var msiFile = project.BuildMsi();

            var bootstrapper = new NsisBootstrapper
            {
                Primary = { FileName = msiFile },

                OutputFile = "glTFExporter.exe",
                //IconFile = "",

                VersionInfo = new VersionInformation("1.0.0.0")
                {
                    ProductName = "Test Application",
                    LegalCopyright = "Copyright Test company",
                    FileDescription = "Test Application",
                    FileVersion = "1.0.0",
                    CompanyName = "Test company",
                    InternalName = "setup.exe",
                    OriginalFilename = "setup.exe"
                },
            };

            bootstrapper.Build();
        }
    }
}