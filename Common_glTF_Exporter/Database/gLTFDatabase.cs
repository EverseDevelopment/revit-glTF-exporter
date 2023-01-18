using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Database
{
    public class gLTFDatabase : CustomEmbeddedDatabase
    {
        protected override string ConnectionString { get { return @"Data Source=C:\Users\Gaston Agusti\OneDrive\Escritorio\EVerse\tst_db\testing.db;Version=3;New=True;Compress=True;"; } }
    }
}
