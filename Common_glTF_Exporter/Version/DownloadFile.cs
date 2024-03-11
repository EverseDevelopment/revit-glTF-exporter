using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Common_glTF_Exporter.Utils;
using Revit_glTF_Exporter;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common_glTF_Exporter.Version
{
    public static class DownloadFile
    {
        public static async Task FromServer(string pathFile, string installerName)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://vxfcsp1qu4.execute-api.us-east-1.amazonaws.com/Prod/LatestInstaller?bucketName=everse.assets&folderName=Installers");

            string version = SettingsConfig.GetValue("version");
            string urlParameters = "?inputVersion=" + version +
                "&&folderName=" + "LeiaGltfExporter" ;

            HttpResponseMessage result = client.GetAsync(urlParameters, HttpCompletionOption.ResponseHeadersRead).Result;
            HttpContent content = result.Content;
            string myContent =  await content.ReadAsStringAsync();

            if (!result.IsSuccessStatusCode)
            {
                return;
            }

            string fileLocation = System.IO.Path.Combine(pathFile, installerName);

            System.Timers.Timer aTimer = new System.Timers.Timer(500);

            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += (s, en) =>
            {
                if (!File.Exists(fileLocation))
                {
                    return;
                }

                FileInfo info = new FileInfo(fileLocation);
                int currentMegas = (int)(info.Length / 1000000);
            };
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            await DownloadFileAsync(myContent, fileLocation);

            aTimer.Dispose();  
        }

        static async Task DownloadFileAsync(string url, string filePath)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    TaskDialog.Show("Title", $"Failed to download file. Status code: {response.StatusCode}");
                    Console.WriteLine($"Failed to download file. Status code: {response.StatusCode}");
                }
            }
        }
    }
}
