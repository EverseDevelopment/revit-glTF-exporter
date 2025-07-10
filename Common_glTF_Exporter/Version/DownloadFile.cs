using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Common_glTF_Exporter.Utils;
using Revit_glTF_Exporter;
using System;
using System.IO;
using System.Linq;
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

            string version = SettingsConfig.currentVersion;
            string urlParameters = "?inputVersion=" + version +
                "&&folderName=" + "e-verse/LeiaGltfExporter";

            HttpResponseMessage result = client.GetAsync(urlParameters, HttpCompletionOption.ResponseHeadersRead).Result;
            HttpContent content = result.Content;
            string myContent =  await content.ReadAsStringAsync();

            if (!result.IsSuccessStatusCode)
            {
                return;
            }

            double fileSize = await getFileSize(myContent);
            if (fileSize == 0)
            {
                fileSize =  5;
            }

            string fileLocation = System.IO.Path.Combine(pathFile, installerName);

            ProgressBarWindow progressBar =
                    ProgressBarWindow.Create(fileSize, 0, "Downloading Installer");
            
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
                ProgressBarWindow.ViewModel.ProgressBarValue = currentMegas;
            };
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            await DownloadFileAsync(myContent, fileLocation);

            ProgressBarWindow.ViewModel.ProgressBarValue = 100 + 1;
            ProgressBarWindow.ViewModel.ProgressBarPercentage = 100;
            ProgressBarWindow.ViewModel.Message = "Download Complete!";
            progressBar.Close();

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
                    Autodesk.Revit.UI.TaskDialog.Show("Title", $"Failed to download file. Status code: {response.StatusCode}");
                    Console.WriteLine($"Failed to download file. Status code: {response.StatusCode}");
                }
            }
        }

        static async Task<double> getFileSize(string signedUrl)
        {

            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage for a HEAD request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, signedUrl);

                // Send the request
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Get the Content-Length header value
                    if (response.Headers.TryGetValues("Content-Length", out var values))
                    {
                        string contentLength = values.FirstOrDefault();
                        if (long.TryParse(contentLength, out long fileSizeBytes))
                        {
                            double fileSizeMegabytes = fileSizeBytes / 1024.0 / 1024.0;
                            return fileSizeMegabytes;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }

                return 0;

            }
        }
    }
}
