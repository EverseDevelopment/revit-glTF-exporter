using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common_glTF_Exporter.Service
{
    internal static class UIApplicationExtension
    {
        //https://forums.autodesk.com/t5/revit-api-forum/how-to-get-uiapplication-from-iexternalapplication/td-p/6355729
        /// <summary>
        /// Get <see cref="Autodesk.Revit.UI.UIApplication"/> using the <paramref name="application"/>
        /// </summary>
        /// <param name="application">Revit UIApplication</param>
        public static UIApplication GetUIApplication(this UIControlledApplication application)
        {
            var type = typeof(UIControlledApplication);

            var propertie = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(e => e.FieldType == typeof(UIApplication));

            return propertie?.GetValue(application) as UIApplication;
        }
    }
}
