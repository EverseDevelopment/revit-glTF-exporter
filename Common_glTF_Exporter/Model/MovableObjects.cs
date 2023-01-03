namespace Revit_glTF_Exporter.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Autodesk.Revit.DB;

    public class MovableObjects
    {
        public MovableObjects()
        {
            this.ObjectsList = new List<MovableObject>();
        }

        public List<MovableObject> ObjectsList { get; set; }

        public int Count
        {
            get { return this.Count; }
            set { this.Count = this.ObjectsList.Count(); }
        }

        public Category Category
        {
            get { return this.Category; }
            set { this.Category = this.ObjectsList.FirstOrDefault().Category; }
        }
    }
}
