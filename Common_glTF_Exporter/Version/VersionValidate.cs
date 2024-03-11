using Common_glTF_Exporter.Version;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Revit_glTF_Exporter
{
    public static class VersionValidation
    {
        /// <summary>
        /// Validate internet connection and version config enable
        /// </summary>
        public static async Task Run()
        {
            if (InternetConnection.Check())
            {
                await LatestVersion.Get();
            }
        }
    }
}
