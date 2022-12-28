using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Revit_glTF_Exporter
{
    /// <summary>
    /// Container for holding a strict set of items
    /// that is also addressable by a unique ID.
    /// </summary>
    /// <typeparam name="T">The type of item contained.</typeparam>
    public class IndexedDictionary<T>
    {
        private Dictionary<string, int> _dict = new Dictionary<string, int>();
        public List<T> List { get; } = new List<T>();
        public string CurrentKey { get; private set; }
        public Dictionary<string,T> Dict
        {
            get
            {
                var output = new Dictionary<string, T>();
                foreach (var kvp in _dict)
                {
                    output.Add(kvp.Key, List[kvp.Value]);
                }
                return output;
            }
        }

        /// <summary>
        /// The most recently accessed item (not effected by GetElement()).
        /// </summary>
        public T CurrentItem
        {
            get { return List[_dict[CurrentKey]]; }
        }

        /// <summary>
        /// The index of the most recently accessed item (not effected by GetElement()).
        /// </summary>
        public int CurrentIndex
        {
            get { return _dict[CurrentKey]; }
        }

        /// <summary>
        /// Add a new item to the list, if it already exists then the 
        /// current item will be set to this item.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item.</param>
        /// <param name="elem">The item to add.</param>
        /// <returns>true if item did not already exist.</returns>
        public bool AddOrUpdateCurrent(string uuid, T elem)
        {
            if (!_dict.ContainsKey(uuid))
            {
                List.Add(elem);
                _dict.Add(uuid, (List.Count - 1));
                CurrentKey = uuid;
                return true;
            }

            CurrentKey = uuid;
            return false;
        }

        /// <summary>
        /// Check if the container already has an item with this key.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item.</param>
        /// <returns></returns>
        public bool Contains(string uuid)
        {
            return _dict.ContainsKey(uuid);
        }

        /// <summary>
        /// Returns the index for an item given it's unique identifier.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item.</param>
        /// <returns>index of item or -1</returns>
        public int GetIndexFromUUID(string uuid)
        {
            if (!Contains(uuid)) throw new Exception("Specified item could not be found.");
            return _dict[uuid];
        }

        /// <summary>
        /// Returns an item given it's unique identifier.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item</param>
        /// <returns>the item</returns>
        public T GetElement(string uuid)
        {
            int index = GetIndexFromUUID(uuid);
            return List[index];
        }

        /// <summary>
        /// Returns as item given it's index location.
        /// </summary>
        /// <param name="index">The item's index location.</param>
        /// <returns>the item</returns>
        public T GetElement(int index)
        {
            if (index < 0 || index > List.Count - 1) throw new Exception("Specified item could not be found.");
            return List[index];
        }
    }
}
