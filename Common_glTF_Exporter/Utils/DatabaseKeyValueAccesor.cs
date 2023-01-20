namespace Common_glTF_Exporter.Utils
{
    using Common_glTF_Exporter.Database;

    public static class DatabaseKeyValueAccesor
    {
        static gLTFDatabase db = new gLTFDatabase();

        public static string GetValue(string key)
        {
            var parameter = db.GetByKey(key);
            if (parameter == null)
            {
                return null;
            }
            else
            {
                return parameter.Value;
            }
        }

        public static void SetValue(string key, string value)
        {
            db.UpdateKeyValueRow(new KeyValueDto() { Key = key, Value = value });
        }
    }
}
