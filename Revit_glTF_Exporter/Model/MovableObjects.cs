using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Revit_glTF_Exporter.Model
{
    public class MovableObjects
    {
        public List<MovableObject> ObjectsList { get; set; }

        public MovableObjects()
        {
            ObjectsList = new List<MovableObject>();
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
