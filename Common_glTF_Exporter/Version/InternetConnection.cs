using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common_glTF_Exporter.Version
{
    public static class InternetConnection
    {
        /// <summary>
        /// Check if internet connection is available.
        /// </summary>
        /// <returns></returns>
        public static bool Check()
        {
            try
            {
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    using (Stream stream = client.OpenRead("https://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch (System.Net.WebException)
            {
                return false;
            }
        }
    }
}
