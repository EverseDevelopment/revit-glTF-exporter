namespace Revit_glTF_Exporter
{
    using System;
    using System.IO;
    using System.Windows.Media.Imaging;
    using Autodesk.Revit.UI;
    using Autodesk.Windows;
    using Autodesk.Revit.DB;
    using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;
    using System.Linq;

    /// <summary>
    /// External Application.
    /// </summary>
    public class ExternalApplication : IExternalApplication
    {
        private static readonly string RIBBONTAB = "e-verse";
        private static readonly string RIBBONPANEL = "Export glTF";
        private static readonly string LEIAURL = @"https://e-verse.com/leia-gltf-exporter/";
        private static string pushButtonName = "Leia";
        private static string pushButtonText = "Leia";
        private static string addInPath = typeof(ExternalApplication).Assembly.Location;
        private static string buttonIconsFolder = Path.GetDirectoryName(addInPath) + "\\Images\\";
        internal Document Document { get; set; }
        public static UIApplication UiApp { get; private set; }

        /// <summary>
        /// Indicates if the version was validated
        /// </summary>
        private bool versionAlreadyValidated = false;

        /// <summary>
        /// Creates a new Ribbon tab in the Revit UI.
        /// </summary>
        /// <param name="application">Revit Application.</param>
        /// <param name="ribbonTabName">Ribbon tab name.</param>
        public static void CreateRibbonTab(UIControlledApplication application, string ribbonTabName)
        {
            RibbonControl ribbon = ComponentManager.Ribbon;
            RibbonTab tab = ribbon.FindTab(ribbonTabName);

            if (tab == null)
            {
                application.CreateRibbonTab(ribbonTabName);
            }
        }

        /// <summary>
        /// On Revit Application shutdown.
        /// </summary>
        /// <param name="application">Revit application.</param>
        /// <returns>Result.</returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /// <summary>
        /// On Revit Application startup.
        /// </summary>
        /// <param name="application">Revit application.</param>
        /// <returns>Result.</returns>
        public Result OnStartup(UIControlledApplication application)
        {
            // -- Events Subscription
            application.ViewActivated += Application_ViewActivated;

            Autodesk.Windows.RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;
            Autodesk.Windows.RibbonTab tab =
                        ribbon.Tabs.FirstOrDefault(tabAbout => tabAbout.Id.Contains("e-verse"));

            if (tab == null)
            {
                CreateRibbonTab(application, RIBBONTAB);
            }

            tab = ribbon.Tabs.FirstOrDefault(tabAbout => tabAbout.Id.Contains("e-verse"));

            Autodesk.Windows.RibbonPanel panel =
                       tab.Panels.FirstOrDefault(panelLeia => panelLeia.Source.Id.Contains(RIBBONPANEL));

            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, LEIAURL);
            if (panel == null)
            {
                RibbonPanel leiaPanel = application.CreateRibbonPanel(RIBBONTAB, RIBBONPANEL);

                PushButtonData pushDataButton = new PushButtonData(pushButtonName, pushButtonText, addInPath, "Revit_glTF_Exporter.ExternalCommand");
                pushDataButton.LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIconsFolder, "logo.png"), UriKind.Absolute));
                pushDataButton.SetContextualHelp(contexHelp);
                pushDataButton.ToolTip = "Export 3D elements to glTF.";
                pushDataButton.LongDescription = "Export any 3D model to use in the cloud or in any other tools like Unity or Unreal.";

                leiaPanel.AddItem(pushDataButton);
            }

            Autodesk.Windows.RibbonPanel Aboutpanel =
                       tab.Panels.FirstOrDefault(panelAbout => panelAbout.Source.Id.Contains("About"));

            if (Aboutpanel == null)
            {
                createAboutUs(application, contexHelp);
            }
            else
            {
                //TODO: Add logic for the about button to always be at the end
                //RibbonItemCollection collctn = Aboutpanel.Source.Items;
                //foreach (Autodesk.Windows.RibbonItem ri in collctn)
                //{
                //    MessageBox.Show(ri.Id, ri.Id);
                //    Aboutpanel.Source.Items.Remove(ri);
                //}
                //createAboutUs(application, contexHelp);
            }

            return Result.Succeeded;
        }

        private void createAboutUs(UIControlledApplication application, ContextualHelp contexHelp)
        {
            RibbonPanel panelAbout = application.CreateRibbonPanel(RIBBONTAB, "About");

            PushButtonData pushDataButtonAbout = new PushButtonData("About us", "About us", addInPath, "Revit_glTF_Exporter.AboutUs");
            pushDataButtonAbout.LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIconsFolder, "e-verse-isologo.png"), UriKind.Absolute));
            pushDataButtonAbout.ToolTip = "About e-verse";
            pushDataButtonAbout.SetContextualHelp(contexHelp);
            pushDataButtonAbout.LongDescription = "Know more about us and our tools";

            panelAbout.AddItem(pushDataButtonAbout);
        }

        private void Application_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
            Document = e.Document;

            #pragma warning disable CS4014
            if (!versionAlreadyValidated)
            {
                VersionValidation.Run();
                versionAlreadyValidated = !versionAlreadyValidated;
            }
            #pragma warning restore CS4014
        }
    }
}