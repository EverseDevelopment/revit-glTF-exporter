using Autodesk.Revit.DB;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Windows.MainWindow;
using glTF.Manipulator.Schema;
using Revit_glTF_Exporter;

namespace Common_glTF_Exporter.EportUtils
{
    public static class GLTFNodeActions
    {
        public static BaseNode CreateGLTFNodeFromElement(Element currentElement, Preferences preferences)
        {
            var node = new BaseNode
            {
                description = Util.ElementDescription(currentElement),
                uuid = currentElement.UniqueId,
                name = currentElement.Name
            };

            if (!preferences.properties)
                return node;

            var parameters = Util.GetElementParameters(currentElement, true);
            parameters["UniqueId"] = currentElement.UniqueId;

            if (currentElement.Category != null)
                parameters["Category"] = currentElement.Category.Name;

            node.extras = new Extras
            {
                parameters = parameters
            };

            return node;
        }
    }
}
