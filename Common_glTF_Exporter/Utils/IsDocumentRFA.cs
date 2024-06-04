using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Common_glTF_Exporter.Utils
{
    class IsDocumentRFA
    {
        public static bool Check(Document doc)
        {
            if (doc.IsFamilyDocument)
            {
                SettingsConfig.SetValue("isRFA", "true");
                return true;
            }
            else
            {
                SettingsConfig.SetValue("isRFA", "false");
                return false;
            }
        }
    }
}
