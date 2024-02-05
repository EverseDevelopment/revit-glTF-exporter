using GltfInstaller.Properties;
using System;
using WixSharp;
using WixSharp.Nsis;

namespace GltfInstaller
{
    internal class Program
    {
        static void Main()
        {
            //#if REVIT2019 || REVIT2020

            var project = new ManagedProject("Leia glTF exporter",
                              new Dir(@"%AppDataFolder%\Autodesk\ApplicationPlugins",
                                  new Dir(@"gltfExporter.bundle",
                                  new File(@"..\Common_glTF_Exporter\PackageContents.xml"),
                                      new Dir(@"Contents",
                                        new Dir(@"2019",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new Files(@"..\Revit_glTF_Exporter_2019\bin\Release\*.*")),
                                        new Dir(@"2020",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new Files(@"..\Revit_glTF_Exporter_2020\bin\Release\*.*")),
                                        new Dir(@"2021",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new Files(@"..\Revit_glTF_Exporter_2021\bin\Release\*.*")),
                                        new Dir(@"2022",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new Files(@"..\Revit_glTF_Exporter_2022\bin\Release\*.*")),
                                        new Dir(@"2023",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new Files(@"..\Revit_glTF_Exporter_2023\bin\Release\*.*")),
                                        new Dir(@"2024",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new Files(@"..\Revit_glTF_Exporter_2024\bin\Release\*.*")))))
                              );

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
                IconFile = "Resources\\logo.ico",

                VersionInfo = new VersionInformation("1.1.0.0")
                {
                    ProductName = "Test Application",
                    LegalCopyright = "Copyright Test company",
                    FileDescription = "Test Application",
                    FileVersion = "1.1.0",
                    CompanyName = "Test company",
                    InternalName = "setup.exe",
                    OriginalFilename = "setup.exe"
                },
            };

            bootstrapper.Build();

            //#endif
        }
    }
}