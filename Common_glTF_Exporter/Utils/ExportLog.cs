using System;
using System.IO;
using System.Text;

namespace Common_glTF_Exporter.Utils
{
    public class ExportLog
    {
        private static readonly string logFilePath = Path.Combine(Links.configDir, "leia_log.txt");

        public static void StartLog()
        {
            if (!Directory.Exists(Links.configDir))
            {
                Directory.CreateDirectory(Links.configDir);
            }

            string path = Path.Combine(Links.configDir, "leia_log.txt");
            File.WriteAllText(path, $"[START] Export started at {DateTime.Now}\n");
        }

        public static void EndLog()
        {
            File.AppendAllText(logFilePath, $"[END] Export ended at {DateTime.Now}\n");
        }

        public static void Write(string message)
        {
            try
            {
                string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}\n";
                File.AppendAllText(logFilePath, timestampedMessage, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error writing to log: {ex.Message}");
            }
        }

        public void WriteException(Exception ex)
        {
            try
            {
                string message = $"[ERROR] {DateTime.Now:HH:mm:ss} - {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logFilePath, message, Encoding.UTF8);
            }
            catch (Exception logEx)
            {
                Console.Error.WriteLine($"Error writing exception to log: {logEx.Message}");
            }
        }

        public string GetLogPath()
        {
            return logFilePath;
        }
    }

}
