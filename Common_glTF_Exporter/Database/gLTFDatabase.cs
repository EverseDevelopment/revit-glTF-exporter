namespace Common_glTF_Exporter.Database
{
    public class gLTFDatabase : CustomEmbeddedDatabase
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public gLTFDatabase() : base()
        {
            this.SetDefaultValues();
        }

        protected override string ConnectionString { get { return @"Data Source=C:\Users\Gaston Agusti\OneDrive\Escritorio\EVerse\tst_db\testing.db;Version=3;New=True;Compress=True;"; } }


        public void SetDefaultValues()
        {
            if(base.GetByKey("first_execution") == null)
            {
                base.AddKeyValueRow(new KeyValueDto() { Key = "first_execution", Value = "true" }); // -- just to validate if the first time was executed
                base.AddKeyValueRow(new KeyValueDto() { Key = "materials", Value = "false" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "elementId", Value = "false" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "normals", Value = "false" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "levels", Value = "false" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "lights", Value = "false" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "grids", Value = "false" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "batchId", Value = "false" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "properties", Value = "false" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "relocateTo0", Value = "false" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "flipAxis", Value = "true" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "exportProperties", Value = "false" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "singleBinary", Value = "true" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "units", Value = "null" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "compression", Value = "none" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "digits", Value = "3" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "path", Value = @"C:\Users\User\Desktop" });
                base.AddKeyValueRow(new KeyValueDto() { Key = "fileName", Value = "3dExport" });
            }
        }
    }
}
