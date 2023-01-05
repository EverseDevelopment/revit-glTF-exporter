namespace Revit_glTF_Exporter.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;

    public class FixedObjects
    {
        public FixedObjects()
        {
            this.ObjectsList = new List<FixedObject>();
        }

        public List<FixedObject> ObjectsList { get; set; }

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
