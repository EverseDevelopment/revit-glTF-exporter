using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shapes;
using WixSharp;
using File = WixSharp.File;

namespace GltfInstaller
{
    internal class Program
    {
        public static string versionValue = "0.0.0";
        static void Main()
        {
            var project = new ManagedProject($"Leia - glTF exporter {versionValue}",
                              new Dir(@"%AppDataFolder%\Autodesk\ApplicationPlugins",
                                  new Dir(@"leia.bundle",
                                  new WixSharp.File(@"..\Common_glTF_Exporter\PackageContents.xml"),
                                      new Dir(@"Contents",
                                       new Dir(@"2019",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2019\bin\Release\Leia_glTF_Exporter.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2019\bin\Release\DracoWrapper.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2019\bin\Release\MeshOpt.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2019\bin\Release\Newtonsoft.Json.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2019\bin\Release\*.png")),
                                        new Dir(@"2020",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2020\bin\Release\Leia_glTF_Exporter.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2020\bin\Release\DracoWrapper.dll",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2020\bin\Release\MeshOpt.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2020\bin\Release\Newtonsoft.Json.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2020\bin\Release\*.png")),
                                        new Dir(@"2021",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2021\bin\Release\Leia_glTF_Exporter.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2021\bin\Release\DracoWrapper.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2021\bin\Release\MeshOpt.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2021\bin\Release\Newtonsoft.Json.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2021\bin\Release\*.png")),
                                        new Dir(@"2022",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2022\bin\Release\Leia_glTF_Exporter.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2022\bin\Release\DracoWrapper.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2022\bin\Release\MeshOpt.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2022\bin\Release\Newtonsoft.Json.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2022\bin\Release\*.png")),
                                        new Dir(@"2023",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2023\bin\Release\Leia_glTF_Exporter.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2023\bin\Release\DracoWrapper.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2023\bin\Release\MeshOpt.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2023\bin\Release\Newtonsoft.Json.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2023\bin\Release\*.png")),
                                        new Dir(@"2024",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2024\bin\Release\Leia_glTF_Exporter.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2024\bin\Release\DracoWrapper.dll",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2024\bin\Release\MeshOpt.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2024\bin\Release\Newtonsoft.Json.dll", 
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2024\bin\Release\*.png")),
                                        new Dir(@"2025",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2025\bin\Release\net8.0-windows\Leia_glTF_Exporter.dll",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2025\bin\Release\net8.0-windows\DracoWrapper.dll",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2025\bin\Release\net8.0-windows\MeshOpt.dll",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2025\bin\Release\net8.0-windows\Newtonsoft.Json.dll",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2025\bin\Release\net8.0-windows\Leia_glTF_Exporter.deps.json",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2025\bin\Release\net8.0-windows\*.png")),
                                        new Dir(@"2026",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2026\bin\Release\net8.0-windows\Leia_glTF_Exporter.dll",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2026\bin\Release\net8.0-windows\DracoWrapper.dll",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2026\bin\Release\net8.0-windows\MeshOpt.dll",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2026\bin\Release\net8.0-windows\Newtonsoft.Json.dll",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new File(@"..\Revit_glTF_Exporter_2026\bin\Release\net8.0-windows\Leia_glTF_Exporter.deps.json",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2026\bin\Release\net8.0-windows\*.png")))))
                              );

            project.GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b");
            project.ControlPanelInfo.Manufacturer = "e-verse";
            project.Version = new Version(versionValue);

            project.ManagedUI = new ManagedUI();

            project.ControlPanelInfo.ProductIcon = "Resources\\logo.ico";

            project.ManagedUI.InstallDialogs.Add<GltfInstaller.WelcomeDialog>()
                                            .Add<GltfInstaller.LicenceDialog>()
                                            .Add<GltfInstaller.ProgressDialog>()
                                            .Add<GltfInstaller.ExitDialog>();

            project.ManagedUI.ModifyDialogs.Add<GltfInstaller.MaintenanceTypeDialog>()
                                           .Add<GltfInstaller.FeaturesDialog>()
                                           .Add<GltfInstaller.ProgressDialog>()
                                           .Add<GltfInstaller.ExitDialog>();

            project.MajorUpgrade = new MajorUpgrade
            {
                AllowDowngrades = false,
                AllowSameVersionUpgrades = true,
                DowngradeErrorMessage = "A newer version of your product is already installed.",
                IgnoreRemoveFailure = true,
                Schedule = UpgradeSchedule.afterInstallInitialize
            };

            project.Actions = new WixSharp.Action[]
            {
                new ManagedAction(CustomActions.CheckRevitProcess, Return.check, When.Before, Step.LaunchConditions, Condition.NOT_Installed)
            };

            var msiFile = project.BuildMsi();
        }
    }

    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CheckRevitProcess(Session session)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("Revit");
                if (processes.Length > 0)
                {
                    var result = MessageBox.Show("Revit is currently running. Would you like to close it to continue with the installation?", "Revit is running", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (result == DialogResult.Yes)
                    {
                        foreach (var process in processes)
                        {
                            process.Kill();
                            process.WaitForExit();
                        }

                        ActionResult resultCheck;

                        resultCheck = CheckFilesAreOverwritable("%ProgramData%\\Autodesk\\ApplicationPlugins\\leia.bundle\\Contents");
                        if (resultCheck == ActionResult.Failure)
                        {
                            return ActionResult.Failure;
                        }

                        resultCheck = CheckFilesAreOverwritable("%AppData%\\Autodesk\\ApplicationPlugins\\leia.bundle\\Contents");
                        if (resultCheck == ActionResult.Failure)
                        {
                            return ActionResult.Failure;
                        }
                    }
                    else if (result == DialogResult.No)
                    {
                        return ActionResult.UserExit;
                    }
                    else
                    {
                        return ActionResult.Failure;
                    }
                }
            }
            catch (Exception ex)
            {
                session.Log("Error checking Revit process: " + ex.Message);
                MessageBox.Show(ex.Message, "Error");

                return ActionResult.Failure;
            }
            return ActionResult.Success;
        }

        private static ActionResult CheckFilesAreOverwritable(string basePath)
        {
            basePath = Environment.ExpandEnvironmentVariables(basePath);

            for (int year = 2019; year <= 2026; year++)
            {
                string filePath = System.IO.Path.Combine(basePath, year.ToString(), "Leia_glTF_Exporter.dll");

                if (!WaitForFilesToBeOverwritable(filePath))
                {
                    MessageBox.Show($"The file Leia_glTF_Exporter.dll {year} is still in use", "Warning");
                    return ActionResult.Failure;
                }
            }

            return ActionResult.Success;
        }

        public static bool WaitForFilesToBeOverwritable(string filePath, int waitTimeMilliseconds = 1000, int maxAttempts = 15)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return true;
            }

            int attempts = 0;
            while (IsFileInUse(filePath) && attempts < maxAttempts)
            {
                Thread.Sleep(waitTimeMilliseconds);
                attempts++;
            }

            return attempts < maxAttempts;
        }

        public static bool IsFileInUse(string filePath)
        {
            try
            {
                using (FileStream stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}
