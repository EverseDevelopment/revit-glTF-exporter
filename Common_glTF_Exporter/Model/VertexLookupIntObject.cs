namespace Common_glTF_Exporter.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// From Jeremy Tammik's RvtVa3c exporter:
    /// https://github.com/va3c/RvtVa3c
    /// A vertex lookup class to eliminate duplicate vertex definitions.
    /// </summary>
    public class VertexLookupIntObject : Dictionary<PointIntObject, int>
    {
        /// <summary>
        /// Return the index of the given vertex,
        /// adding a new entry if required.
        /// </summary>
        /// <param name="p">PointIntObject.</param>
        /// <returns>Key position.</returns>
        public int AddVertexAndFlatten(PointIntObject p, List<double> vertexBuffer)
        {
            if (this.ContainsKey(p))
            {
                return this[p];
            }

            int index = this.Count;
            this[p] = index;

            // Flatten and append vertex coordinates
            vertexBuffer.Add(p.X);
            vertexBuffer.Add(p.Y);
            vertexBuffer.Add(p.Z);

            return index;
        }

        public int AddVertex(PointIntObject p)
        {
            return this.ContainsKey(p)
              ? this[p]
              : this[p] = this.Count;
        }
        /// <summary>
        /// Define equality for integer-based PointInt.
        /// </summary>
        public class PointIntEqualityComparer : IEqualityComparer<PointIntObject>
        {
            public bool Equals(PointIntObject p, PointIntObject q)
            {
                return p.CompareTo(q) == 0;
            }

            public int GetHashCode(PointIntObject p)
            {
                return string.Concat(p.X.ToString(), ",", p.Y.ToString(), ",", p.Z.ToString()).GetHashCode();
            }
        }
    }
}
