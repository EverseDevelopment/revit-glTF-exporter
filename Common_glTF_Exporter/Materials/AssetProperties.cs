using Autodesk.Revit.DB.Visual;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Materials
{
    public static class AssetProperties
    {
        public static Asset GetDiffuseBitmap(Asset theAsset)
        {
            foreach (var name in new[] { "opaque_albedo", "generic_diffuse" })
            {
                var prop = theAsset.FindByName(name);
                if (prop?.NumberOfConnectedProperties > 0)
                {
                    var connected = prop.GetSingleConnectedAsset();
                    if (connected != null)
                        return connected;
                }
            }

            return null;
        }
    }
}
