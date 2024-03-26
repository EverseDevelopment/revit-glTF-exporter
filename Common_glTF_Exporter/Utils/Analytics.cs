using Autodesk.Revit.DB;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common_glTF_Exporter.Utils
{
    public class Analytics
    {
        public static async Task Send(string action, string details)
        {
            DateTime currentDate = DateTime.Now;
            string date = currentDate.ToString("yyyy-MM-dd");

            RegionInfo currentRegion = new RegionInfo(CultureInfo.CurrentCulture.Name);
            string location = currentRegion.EnglishName.ToString();

            string user = SettingsConfig.GetValue("user");
            string software = SettingsConfig.GetValue("release");
            string version = SettingsConfig.GetValue("version");

            // Use string interpolation to incorporate variables into the JSON string
            string json = $@"{{
                ""User"": ""{user}"",
                ""Location"": ""{location}"",
                ""Software"": ""{software}"",
                ""Date"": ""{date}"",
                ""Action"": ""{action}"",
                ""Details"": ""{details}"",
                ""Version"": ""{version}""
            }}";

            string url = "https://us-central1-data-warehouse-320521.cloudfunctions.net/LeiaGltfExporter";

            using (var client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string apiKey = SettingsConfig.GetValue("apikey"); ;
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Request successful");
                    // Optionally, you can read the response content here
                    // string responseContent = await response.Content.ReadAsStringAsync();
                    // Console.WriteLine(responseContent);
                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            }
        }

    }
}


