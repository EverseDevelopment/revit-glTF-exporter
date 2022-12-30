using System;
using System.Collections.Generic;
using System.Text;
using Revit_glTF_Exporter;

namespace Common_glTF_Exporter.Model
{
    /// <summary>
    /// From Jeremy Tammik's RvtVa3c exporter:
    /// https://github.com/va3c/RvtVa3c
    /// A vertex lookup class to eliminate 
    /// duplicate vertex definitions.
    /// </summary>
    class VertexLookupIntObject : Dictionary<PointIntObject, int>
    {
        /// <summary>
        /// Define equality for integer-based PointInt.
        /// </summary>
        class PointIntEqualityComparer : IEqualityComparer<PointIntObject>
        {
            public bool Equals(PointIntObject p, PointIntObject q)
            {
                return 0 == p.CompareTo(q);
            }

            public int GetHashCode(PointIntObject p)
            {
                return (p.X.ToString()
                  + "," + p.Y.ToString()
                  + "," + p.Z.ToString())
                  .GetHashCode();
            }
        }

        /// <summary>
        /// Return the index of the given vertex,
        /// adding a new entry if required.
        /// </summary>
        public int AddVertex(PointIntObject p)
        {
            return ContainsKey(p)
              ? this[p]
              : this[p] = Count;
        }
    }

}
