using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Service
{
    public class RevitCollectorService
    {
        private UIApplication uiApplication;

        public RevitCollectorService(UIApplication uIApplication)
        {
            uiApplication = uIApplication;
        }

        /// <summary>
        /// Get current active document
        /// </summary>
        /// <returns></returns>
        public Document GetDocument()
        {
            return uiApplication.ActiveUIDocument.Document;
        }

        public Autodesk.Revit.ApplicationServices.Application GetApplication() 
        {
            return uiApplication.Application;
        }
    }
}
