using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.Net.WebRequestMethods;

namespace Common_glTF_Exporter
{
    public class Links
    {
        public static string contactLink = "https://e-verse.com/contact/";
        public static string everseWebsite = "https://e-verse.com";
        public static string leiaWebsite = "https://e-verse.com/leia-plugin/";
        public static string notionLink = "https://e-verse.notion.site/Leia-version-4-4-724-0c53931c9cc04ea3ae143af10bfbbc8a";
        public static string configDir =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Leia");
    }
}
