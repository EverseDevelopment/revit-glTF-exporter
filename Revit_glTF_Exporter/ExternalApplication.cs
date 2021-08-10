using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace Revit_glTF_Exporter
{
    class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            /// <summary>
            /// Create a new Tab on Ribbon Bar.
            /// </summary>
            const string RIBBON_TAB = "glTF Exporter";
            Ribbon.CreateRibbonTab(application, RIBBON_TAB);

            /// <summary>
            /// Create a new Panel on Ribbon Tab.
            /// </summary>

            const string RIBBON_PANEL = "glTF";
            RibbonPanel ribbonPanel = Ribbon.CreateRibbonPanel(application, RIBBON_PANEL, RIBBON_TAB);

            /// <summary>
            /// Create new Buttons on Panel.
            /// </summary>
            /// 
            const string PUSH_BUTTON_NAME = "glTF Exporter";
            const string PUSH_BUTTON_TEXT = "glTF Exporter";

            PushButtonData pushDataButton = Ribbon.CreatePushButtonData(PUSH_BUTTON_NAME, PUSH_BUTTON_TEXT, "Revit_glTF_Exporter.ExternalCommand");

            PushButton pushButton = ribbonPanel.AddItem(pushDataButton) as PushButton;
            System.Drawing.Bitmap ico = Properties.Resources.gltf;
            System.Windows.Media.Imaging.BitmapSource icon = Ribbon.Icon(ico);
            pushButton.LargeImage = icon;

            return Result.Succeeded;



        }
    }
}
