using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Revit_glTF_Exporter.Model
{
    public interface IObjects<T>
    {
        List<T> ObjectsList { get; set; }
        int Count { get; set; }
        Category Category { get; set; }
    }
}
