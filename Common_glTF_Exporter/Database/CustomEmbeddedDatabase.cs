using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Common_glTF_Exporter.Database
{
    public abstract class CustomEmbeddedDatabase
    {
        /// <summary>
        /// To set your connection string
        /// Example of ConString: "Data Source=database.db;Version=3;New=True;Compress=True;"
        /// </summary>
        protected abstract string ConnectionString { get; }

        /// <summary>
        /// The default name for the KeyValue Table
        /// </summary>
        protected virtual string KeyValueDefaultName { get { return "Config"; } }

        /// <summary>
        /// Ctor
        /// </summary>
        protected CustomEmbeddedDatabase()
        {
            CreateKeyValueTable();
        }

        /// <summary>
        /// Helper method to execute Insert/Updates
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="args">Args</param>
        protected int ExecuteWrite(string query, Dictionary<string, object> args)
        {
            int numberOfRowsAffected;

            using (var con = new SQLiteConnection(ConnectionString))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(query, con))
                {
                    //set the arguments given in the query
                    foreach (var pair in args)
                    {
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    }

                    //execute the query and get the number of row affected
                    numberOfRowsAffected = cmd.ExecuteNonQuery();
                }

                return numberOfRowsAffected;
            }
        }

        protected DataTable ExecuteRead(string query, Dictionary<string, object> args)
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;

            using (var con = new SQLiteConnection(ConnectionString))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(query, con))
                {
                    foreach (KeyValuePair<string, object> entry in args)
                    {
                        cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                    }

                    var da = new SQLiteDataAdapter(cmd);

                    var dt = new DataTable();
                    da.Fill(dt);

                    da.Dispose();
                    return dt;
                }
            }
        }

        /// <summary>
        /// Method helper to create a custom Key/Value Table
        /// </summary>
        protected virtual bool CreateKeyValueTable()
        {
            string KeyValueTableQuery = $"CREATE TABLE IF NOT EXISTS {KeyValueDefaultName}(Id INTEGER PRIMARY KEY AUTOINCREMENT, [Key] TEXT NOT NULL, [Value] TEXT NOT NULL);";

            using (var con = new SQLiteConnection(ConnectionString))
            {
                con.Open();
                using (SQLiteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = KeyValueTableQuery;
                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }

        /// <summary>
        /// Add Key Value Row
        /// </summary>
        public int AddKeyValueRow(KeyValueDto dto)
        {
            string query = $"INSERT INTO {KeyValueDefaultName}([Key], [Value]) VALUES(@key, @value)";
            var args = new Dictionary<string, object>
            {
                {"@key", dto.Key},
                {"@value", dto.Value}
            };

            return ExecuteWrite(query, args);
        }

        /// <summary>
        /// Update Key Value Row
        /// </summary>
        public int UpdateKeyValueRow(KeyValueDto dto)
        {
            string query = $"UPDATE {KeyValueDefaultName} SET [Value] = @value where [Key] = @key";
            var args = new Dictionary<string, object>
            {
                {"@key", dto.Key},
                {"@value", dto.Value}
            };

            return ExecuteWrite(query, args);
        }

        /// <summary>
        /// Delete by Key
        /// </summary>
        public int DeleteKeyValueRow(string key)
        {
            string query = $"DELETE FROM {KeyValueDefaultName} where [Key] = @key";
            var args = new Dictionary<string, object>
            {
                {"@key", key},
            };

            return ExecuteWrite(query, args);
        }

        /// <summary>
        /// Get By Key
        /// </summary>
        public KeyValueDto GetByKey(string key)
        {
            var query = $"SELECT * FROM {KeyValueDefaultName} WHERE [Key] = @key";
            var args = new Dictionary<string, object>
            {
                {"@key", key}
            };

            DataTable dt = ExecuteRead(query, args);
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }

            var dto = new KeyValueDto
            {
                Key = Convert.ToString(dt.Rows[0]["Key"]),
                Value = Convert.ToString(dt.Rows[0]["Value"])
            };

            return dto;
        }
    }

    public class KeyValueDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
