using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Autodesk.Revit.DB;
using Common_glTF_Exporter.Export;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Transform;
using Common_glTF_Exporter.Utils;
using Common_glTF_Exporter.Windows.MainWindow;
using Revit_glTF_Exporter;
using Transform = Autodesk.Revit.DB.Transform;
using Common_glTF_Exporter.EportUtils;
using Common_glTF_Exporter.Core;

namespace Common_glTF_Exporter.EportUtils
{
    public static class ElementValidations
    {
        public static bool ShouldSkipElement(Element currentElement, Autodesk.Revit.DB.View currentView,
            Document currentDocument, Preferences preferences, IndexedDictionary<GLTFNode> nodes)
        {
            if (currentElement == null)
            {
                return true;
            }

            bool isHiddenOrLocked = !Util.CanBeLockOrHidden(currentElement, currentView, currentDocument.IsFamilyDocument);
            bool isLevelToSkip = currentElement is Level && !preferences.levels;
            bool isAlreadyProcessed = nodes.Contains(currentElement.UniqueId);

            if (isHiddenOrLocked || isLevelToSkip || isAlreadyProcessed)
            {
                return true;
            }

            return false;
        }

        public static bool ShouldOmitElement(Element currentElement, 
            IndexedDictionary<VertexLookupIntObject> currentVertices, 
            View currentView, Document currentDocument)
        {
            if (currentElement == null)
                return true;

            if (currentVertices == null || !currentVertices.List.Any())
                return true;

            if (!Util.CanBeLockOrHidden(currentElement, currentView, currentDocument.IsFamilyDocument) ||
                currentElement is RevitLinkInstance)
                return true;

            return false;
        }
    }
}
