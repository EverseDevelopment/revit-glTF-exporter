namespace Common_glTF_Exporter.Utils
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using Autodesk.Revit.UI;

    internal class FilesHelper
    {
        public static bool AskToSave(ref string filename, string filter, string defaultExt, string initialDirectory = null)
        {
            using (System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveDialog.Filter = filter;
                saveDialog.DefaultExt = defaultExt;
                saveDialog.FileName = filename;

                // -- Optional initial directory
                if (initialDirectory != null)
                {
                    saveDialog.InitialDirectory = initialDirectory;
                }

                DialogResult resultDialog = saveDialog.ShowDialog();
                filename = saveDialog.FileName;
                if (resultDialog != System.Windows.Forms.DialogResult.OK)
                {
                    return false;
                }
            }

            if (File.Exists(filename) && FileIsLocked(filename, FileAccess.ReadWrite))
            {
                TaskDialog.Show("Error", "The file is opened by another process, please close it and try again");
                return false;
            }

            return true;
        }

        internal static bool FileIsLocked(string filename, FileAccess file_access)
        {
            if (!File.Exists(filename))
            {
                return false;
            }

            // Try to open the file with the indicated access.
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open, file_access);
                fs.Close();
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}
