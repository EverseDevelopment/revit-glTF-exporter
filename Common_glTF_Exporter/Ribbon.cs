using System;
using System.Linq;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
using Autodesk.Windows;

namespace Revit_glTF_Exporter
{
    public static class Ribbon
    {
        /// <summary>
        /// Hierarchy:
        /// Tab can have many Panels
        /// Each Panel can have many Items. 
        /// </summary>

        public static void CreateRibbonTab(UIControlledApplication application, string ribbonTabName)
        {
            RibbonControl ribbon = ComponentManager.Ribbon;
            RibbonTab tab = ribbon.FindTab(ribbonTabName);
            
            if (tab == null)
                application.CreateRibbonTab(ribbonTabName);

        }

        public static Autodesk.Revit.UI.RibbonPanel CreateRibbonPanel(UIControlledApplication application, string ribbonPanelName, string ribbonTabName = null)
        {
            /// <summary>
            /// This metthod is used to create a new Ribbon panel.
            /// Add a ribbonTabName if you need to include this panel inside on Existing Tab.
            /// <param name="application">An object that is passed to the external application 
            /// </summary>

            Autodesk.Revit.UI.RibbonPanel panel = application.GetRibbonPanels(ribbonTabName)
                                                             .FirstOrDefault(n => n.Name.Equals(ribbonTabName, StringComparison.InvariantCulture));
            if (panel != null)
                return panel;
            else
            {
                panel = application.CreateRibbonPanel(ribbonTabName, ribbonPanelName);
                return panel;
            }
        }

        public static PushButtonData CreatePushButtonData(string pushButtonName, string pushButtonText, string className)
        {
            /// <summary>
            /// This metthod is used to create a new PushButtonData.
            /// className: The name of the class containing the implementation for the command.
            /// <param name="application">An object that is passed to the external application 
            /// </summary>

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButtonData = new PushButtonData(pushButtonName, pushButtonText, assemblyPath, className);
            return pushButtonData;
        }

        public static void AddPushButton(Autodesk.Revit.UI.RibbonPanel ribbonPanel, PushButtonData pushButtonData)
        {
            PushButton pushButton = ribbonPanel.AddItem(pushButtonData) as PushButton;
        }

        public static BitmapSource Icon(System.Drawing.Bitmap ico)
        {
            IntPtr img = ico.GetHbitmap();
            BitmapSource icon = Imaging.CreateBitmapSourceFromHBitmap(
                img,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
                );
            return icon;
        }

    }
}
