using Autodesk.Revit.UI;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Utils;
using Revit_glTF_Exporter;
using System;
using System.Net.Http;
using System.Threading.Tasks;


namespace Revit_glTF_Exporter
{
    public static class LatestVersion
    {
        public static async Task Get()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://APIAutoUpdate");

            string version = SettingsConfig.GetValue("version");
            string urlParameters = "?inputVersion=" + version +
                "&&folderName=" + "e-verse/LeiaGltfExporter";

            try {
                HttpResponseMessage result = client.GetAsync(urlParameters, HttpCompletionOption.ResponseHeadersRead).Result;

                if (result.IsSuccessStatusCode)
                {

                    HttpContent content = result.Content;
                    string myContent = await content.ReadAsStringAsync();

                    Payload payload = Newtonsoft.Json.JsonConvert.DeserializeObject<Payload>(myContent);

                    if (!payload.Update)
                    {
                        VersionWindow versionWindow = new VersionWindow(payload.Version);
                        versionWindow.ShowDialog();
                    }

                    content.Dispose();
                }

                client.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
