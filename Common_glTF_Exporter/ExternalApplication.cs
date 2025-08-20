namespace Revit_glTF_Exporter
{
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Autodesk.Windows;
    using Common_glTF_Exporter;
    using Common_glTF_Exporter.Service;
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;

    /// <summary>
    /// External Application.
    /// </summary>
    public class ExternalApplication : IExternalApplication
    {
        public static RevitCollectorService RevitCollectorService;
        private static readonly string RIBBONTAB = "e-verse";
        private static readonly string RIBBONPANEL = "Export glTF";
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
            RevitCollectorService = new RevitCollectorService(application.GetUIApplication());

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

            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, Links.leiaWebsite);
            if (panel == null)
            {
                RibbonPanel leiaPanel = application.CreateRibbonPanel(RIBBONTAB, RIBBONPANEL);

                PushButtonData pushDataButton = new PushButtonData(pushButtonName, pushButtonText, addInPath, "Revit_glTF_Exporter.ExternalCommand");
                string logoPath = "M168.7 80.5V33.2l-25-14.1l-20.2 12.5l-41.3-24L57.3 21.7v24.1L15.9 69.8V98l19.3 11.6v0.8v47.4L60 171.8  l22.1-12.5l41.4 24.1l24.9-14.1v-24.1l41.4-24.1V94L168.7 80.5z M181.5 95.2L124 129.4l-37.1-20.7l16.9-9.5l20.3 11.6L146.5 97  l0-22.6l19.1 10.7L181.5 95.2z M38.6 79.7L24 71.5l33.3-19.3v17.1L38.6 79.7z M106.6 71L106.6 71L106.6 71L81.2 55.7L62.8 66.1V49  v-3V28l58 32.5v42l-14.2-8.1V71.8 M163.1 39.6v37.8l-16.6-9.3l0-19.1L163.1 39.6z M126.3 60.5l14.7-8.4l0 41.8l-14.7 9V60.5z   M81.1 62.1l15.3 9.2L61 92.3l-16.7-9.4L81.1 62.1z M63.3 97.4L101.1 75v19.4l-22.1 12.5v26.3l18.3 10.4l-18.8 11.5l-15.1 8.5V97.4z   M84.4 130v-16.4l36.8 20.6v16.7L84.4 130z M102.7 146.7l18.6 10.6v18.4L87.4 156L102.7 146.7z M143.8 25.5l16.5 9.3l-16.4 9.3  l-15.1-9.3L143.8 25.5z M120 36l18.4 11.3l-14.9 8.4L65.6 23.3L82.1 14L120 36z M21.4 94.9V76.4l13.7 7.7v19L21.4 94.9z M40.6 110.4  v-3.9V87.2l17.1 9.6v67.4l-17.1-9.7V110.4z M126.8 175.2v-17.9l16.1-9v17.8L126.8 175.2z M145.8 140.3L126.8 151v-16.8l57.5-34.2v18  L145.8 140.3z";
                pushDataButton.LargeImage = CreateLogo(logoPath);
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
            string logoPath = "M60.6 10.7H42.3l-0.2 0c0 0 0 0 0 0l0 0H14.7c-6.9 0.6-10 7.6-7 12.8l17.1 30l0 0l4.9 8.5c2.9 5.2 10.9 6.2 14.6 0.3l2.7-4.6c0.8-1.4 0.8-3 0-4.4l-4.1-7.1l-8.6-15.1c-0.2-0.4-0.3-0.7-0.4-1.2c-0.2-0.6-0.2-1.2-0.1-1.8c0-0.1 0-0.2 0-0.3c0.2-1.6 1.4-3.1 3-3.8c2.2-1 5 0.1 6.2 2.2l7.7 13.5c1.7 3 6 3 7.7 0L68 23.6C71.3 17.9 67.2 10.7 60.6 10.7z";
            pushDataButtonAbout.LargeImage = CreateLogo(logoPath);
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

        private BitmapSource CreateLogo(string logoPath, double size = 25)
        {
            Geometry pathGeometry = PathGeometry.Parse(logoPath);
            Rect bounds = pathGeometry.Bounds;

            double scale = Math.Min(size / bounds.Width, size / bounds.Height);

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(scale, scale));
            transformGroup.Children.Add(new TranslateTransform(
                -bounds.X * scale + (size - bounds.Width * scale) / 2,
                -bounds.Y * scale + (size - bounds.Height * scale) / 2));

            GeometryDrawing drawing = new GeometryDrawing
            {
                Geometry = pathGeometry,
                Brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(249, 79, 70)),
                Pen = null
            };

            DrawingGroup drawingGroup = new DrawingGroup
            {
                Transform = transformGroup
            };
            drawingGroup.Children.Add(drawing);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawDrawing(drawingGroup);
            }

            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)size, (int)size, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            bitmap.Freeze();

            return bitmap;
        }

    }
}