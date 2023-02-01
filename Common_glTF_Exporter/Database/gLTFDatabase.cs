using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace Common_glTF_Exporter.Database
{
    public class gLTFDatabase : CustomEmbeddedDatabase
    {

        public gLTFDatabase()
            : base()
        {
            this.SetDefaultValues();
        }

        private static readonly string BinaryLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string appSettingsName = string.Concat(Assembly.GetExecutingAssembly().GetName().Name, ".db");
        private static string appSettingsFile = System.IO.Path.Combine(BinaryLocation, appSettingsName);

        protected override string ConnectionString { get { return $"Data Source={appSettingsFile};Version=3;New=True;Compress=True;"; } }

        public void SetDefaultValues()
        {
            if (GetByKey("first_execution") == null)
            {
                AddKeyValueRow(new KeyValueDto() { Key = "first_execution", Value = "true" }); // -- just to validate if the first time was executed
                AddKeyValueRow(new KeyValueDto() { Key = "format", Value = "gltf" });
                AddKeyValueRow(new KeyValueDto() { Key = "elementId", Value = "false" });
                AddKeyValueRow(new KeyValueDto() { Key = "normals", Value = "false" });
                AddKeyValueRow(new KeyValueDto() { Key = "levels", Value = "false" });
                AddKeyValueRow(new KeyValueDto() { Key = "lights", Value = "false" });
                AddKeyValueRow(new KeyValueDto() { Key = "grids", Value = "false" });
                AddKeyValueRow(new KeyValueDto() { Key = "batchId", Value = "false" });
                AddKeyValueRow(new KeyValueDto() { Key = "properties", Value = "false" });
                AddKeyValueRow(new KeyValueDto() { Key = "relocateTo0", Value = "false" });
                AddKeyValueRow(new KeyValueDto() { Key = "flipAxis", Value = "true" });
                AddKeyValueRow(new KeyValueDto() { Key = "exportProperties", Value = "false" });
                AddKeyValueRow(new KeyValueDto() { Key = "units", Value = "null" });
                AddKeyValueRow(new KeyValueDto() { Key = "compression", Value = "none" });
                AddKeyValueRow(new KeyValueDto() { Key = "path", Value = @"C:\Users\User\Desktop" });
                AddKeyValueRow(new KeyValueDto() { Key = "fileName", Value = "3dExport" });
                AddKeyValueRow(new KeyValueDto() { Key = "materials", Value = "true" });
            }
        }
    }
}
