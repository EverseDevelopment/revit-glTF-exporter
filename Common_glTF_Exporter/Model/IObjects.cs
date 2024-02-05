namespace Common_glTF_Exporter.Model
{
    using System.Collections.Generic;
    using Autodesk.Revit.DB;

    public interface IObjects<T>
    {
        List<T> ObjectsList { get; set; }

        int Count { get; set; }

        Category Category { get; set; }
    }
}
