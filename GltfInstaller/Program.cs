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
                              new Dir(@"%CommonAppDataFolder%\Autodesk\ApplicationPlugins",
                                  new Dir(@"leia.bundle",
                                  new WixSharp.File(@"..\Common_glTF_Exporter\PackageContents.xml"),
                                      new Dir(@"Contents",
                                        new Dir(@"2019",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2019\bin\Release\Leia_glTF_Exporter.dll.config",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2019\bin\Release\*.dll"),
                                            new Files(@"..\Revit_glTF_Exporter_2019\bin\Release\*.png")),
                                        new Dir(@"2020",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2020\bin\Release\Leia_glTF_Exporter.dll.config",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2020\bin\Release\*.dll"),
                                            new Files(@"..\Revit_glTF_Exporter_2020\bin\Release\*.png")),
                                        new Dir(@"2021",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2021\bin\Release\Leia_glTF_Exporter.dll.config",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2021\bin\Release\*.dll"),
                                            new Files(@"..\Revit_glTF_Exporter_2021\bin\Release\*.png")),
                                        new Dir(@"2022",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2022\bin\Release\Leia_glTF_Exporter.dll.config",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2022\bin\Release\*.dll"),
                                            new Files(@"..\Revit_glTF_Exporter_2022\bin\Release\*.png")),
                                        new Dir(@"2023",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2023\bin\Release\Leia_glTF_Exporter.dll.config",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2023\bin\Release\*.dll"),
                                            new Files(@"..\Revit_glTF_Exporter_2023\bin\Release\*.png")),
                                        new Dir(@"2024",
                                            new File(@"..\Common_glTF_Exporter\Leia_glTF_Exporter.addin"),
                                            new File(@"..\Revit_glTF_Exporter_2024\bin\Release\Leia_glTF_Exporter.dll.config",
                                                new FilePermission("Everyone", GenericPermission.All)),
                                            new Files(@"..\Revit_glTF_Exporter_2024\bin\Release\*.dll"),
                                            new Files(@"..\Revit_glTF_Exporter_2024\bin\Release\*.png")))))
                              );

            project.GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b");
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

            // Set majorUpgrade to automatically uninstall old versions
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

                        string basePath = Environment.ExpandEnvironmentVariables("%ProgramData%\\Autodesk\\ApplicationPlugins\\leia.bundle\\Contents");
                        List<string> filePaths = new List<string>();

                        for (int year = 2019; year <= 2024; year++)
                        {
                            string filePath = $"{basePath}\\{year}\\Leia_glTF_Exporter.dll";
                            bool overwrittable = WaitForFilesToBeOverwritable(filePath);
                            if (!overwrittable)
                            {
                                MessageBox.Show($"The file Leia_glTF_Exporter.dll {year} is still in use", "Warning");
                                return ActionResult.Failure;
                            }
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
                return ActionResult.Failure;
            }
            return ActionResult.Success;
        }

        public static bool WaitForFilesToBeOverwritable(string filePath, int waitTimeMilliseconds = 1000, int maxAttempts = 10)
        {
                int attempts = 0;
                while (IsFileInUse(filePath) && attempts < maxAttempts)
                {
                    Thread.Sleep(waitTimeMilliseconds);
                    attempts++;
                }

                if (attempts >= maxAttempts)
                {
                    return false;
                }

                return true;
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
