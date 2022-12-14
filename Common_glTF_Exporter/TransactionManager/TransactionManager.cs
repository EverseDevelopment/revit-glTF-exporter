using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.TransactionManager
{
    public class TransactionManager
    {
        public static void CreateNewTransaction(Document doc, string transactionName, Action action)
        {
            using (var t = new Transaction(doc))
            {
                t.Start(transactionName);
                action();
                t.Commit();
            }
        }
    }
}
