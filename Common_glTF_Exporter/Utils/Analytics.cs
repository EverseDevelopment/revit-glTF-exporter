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
        private static string apiUrl = "https://expoterAPI";
        private static HttpClient client = new HttpClient();
        public static async Task Send(string action, string details)
        {
            DateTime currentDate = DateTime.Now;
            string date = currentDate.ToString();

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

            try { 
 
                 var content = new StringContent(json, Encoding.UTF8, "application/json");

                 string apiKey = SettingsConfig.GetValue("apikey"); ;
                 client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                 var response = await client.PostAsync(apiUrl, content);

                 if (response.IsSuccessStatusCode)
                 {
                     Console.WriteLine("Request successful");
                 }
                 else
                 {
                     Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                 }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}


