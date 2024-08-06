using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common_glTF_Exporter.Utils
{
    public static class Hyperlink
    {
        public static void Run(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
