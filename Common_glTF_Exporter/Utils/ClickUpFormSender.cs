using Common_glTF_Exporter.Utils;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public static class ClickUpFormSender
{
    public static async Task CreateClickUpTask(string email, string errorDescription, string exError)
    {
        string url = "ClickupEndPoint";
        string description = $"{email}\n{errorDescription}\n{exError}";

        using (var client = new HttpClient())
        {
            // Leer el archivo de forma síncrona (compatible con .NET Framework)
            byte[] fileBytes = File.ReadAllBytes(ExportLog.logFilePath);

            var content = new ByteArrayContent(fileBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // Crear el JSON dinámicamente, incluyendo la descripción
            string taskJson = $"{{\"name\":\"Bug Report\",\"description\":\"{EscapeJson(description)}\"}}";

            // Agregar headers
            content.Headers.Add("x-file-name", ExportLog.FileLogName);
            content.Headers.Add("x-task-json", taskJson);

            try
            {
                Console.WriteLine("Uploading file...");
                HttpResponseMessage response = await client.PostAsync(url, content);

                string responseText = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response status: " + response.StatusCode);
                Console.WriteLine("Response body: " + responseText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    // Escapa caracteres especiales para que no rompan el JSON
    private static string EscapeJson(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }
}
