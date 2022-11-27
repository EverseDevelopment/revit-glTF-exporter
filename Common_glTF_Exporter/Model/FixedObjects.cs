using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Revit_glTF_Exporter.Model
{
    public class FixedObjects
    {
        public List<FixedObject> ObjectsList { get; set; }

        public FixedObjects()
        {
            ObjectsList = new List<FixedObject>();
        }

        public int Count
        {
            get { return Count; }
            set
            {
                this.Count = ObjectsList.Count();
            }
        }

        public Category Category
        {
            get { return Category; }
            set
            {
                this.Category = ObjectsList.FirstOrDefault().Category;
            }
        }
    }
}
