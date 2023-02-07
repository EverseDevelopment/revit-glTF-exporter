using System;
using System.Windows.Forms;
using WixSharp;
using WixSharp.UI.WPF;

namespace text
{
    internal class Program
    {
        static void Main()
        {
            var project = new ManagedProject("MyProduct",
                              new Dir(@"%ProgramFiles%\My Company\My Product",
                                  new File("Program.cs")));

            project.GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b");

            // project.ManagedUI = ManagedUI.DefaultWpf; // all stock UI dialogs

            //custom set of UI WPF dialogs
            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs.Add<text.WelcomeDialog>()
                                            .Add<text.LicenceDialog>()
                                            .Add<text.FeaturesDialog>()
                                            .Add<text.InstallDirDialog>()
                                            .Add<text.ProgressDialog>()
                                            .Add<text.ExitDialog>();

            project.ManagedUI.ModifyDialogs.Add<text.MaintenanceTypeDialog>()
                                           .Add<text.FeaturesDialog>()
                                           .Add<text.ProgressDialog>()
                                           .Add<text.ExitDialog>();

            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";

            project.BuildMsi();
        }
    }
}