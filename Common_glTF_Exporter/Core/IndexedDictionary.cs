namespace Revit_glTF_Exporter
{
    using System;
    using System.Collections.Generic;
    using Common_glTF_Exporter.Core;

    /// <summary>
    /// Container for holding a strict set of items
    /// that is also addressable by a unique ID.
    /// </summary>
    /// <typeparam name="T">The type of item contained.</typeparam>
    public class IndexedDictionary<T>
    {
        private Dictionary<string, int> dict = new Dictionary<string, int>();

        /// <summary>
        /// Gets all the generic elements inside the IndexedDictionary.
        /// </summary>
        public List<T> List { get; } = new List<T>();

        /// <summary>
        /// Gets the current key from actual element.
        /// </summary>
        public string CurrentKey { get; private set; }

        /// <summary>
        /// Temp output used in 'this.Dict'
        /// </summary>
        Dictionary<string, T> output = new Dictionary<string, T>();

        /// <summary>
        /// Gets the dictionary.
        /// </summary>
        public Dictionary<string, T> Dict
        {
            get
            {
                output.Clear();
                foreach (var kvp in this.dict)
                {
                    output.Add(kvp.Key, this.List[kvp.Value]);
                }

                return output;
            }
        }

        /// <summary>
        /// Gets the most recently accessed item (not effected by GetElement()).
        /// </summary>
        public T CurrentItem
        {
            get { return this.List[this.dict[this.CurrentKey]]; }
        }

        /// <summary>
        /// Gets the index of the most recently accessed item (not effected by GetElement()).
        /// </summary>
        public int CurrentIndex
        {
            get { return this.dict[this.CurrentKey]; }
        }

        /// <summary>
        /// Add a new item to the list, if it already exists then the current item will be set to this item.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item.</param>
        /// <param name="elem">The item to add.</param>
        /// <returns>true if item did not already exist.</returns>
        public bool AddOrUpdateCurrent(string uuid, T elem)
        {
            if (!this.dict.ContainsKey(uuid))
            {
                this.List.Add(elem);
                this.dict.Add(uuid, this.List.Count - 1);
                this.CurrentKey = uuid;
                return true;
            }

            this.CurrentKey = uuid;

            return false;
        }

        /// <summary>
        /// Add a new gltfMaterial to the list, if it already exists then the current item will be set to this item.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item.</param>
        /// <param name="elem">The item to add.</param>
        /// <param name="doubleSided">Identify if the material is double sided.</param>
        /// <returns>true if item did not already exist.</returns>
        public bool AddOrUpdateCurrentMaterial(string uuid, T elem, bool doubleSided)
        {
            if (!this.dict.ContainsKey(uuid))
            {
                this.List.Add(elem);
                this.dict.Add(uuid, this.List.Count - 1);
                this.CurrentKey = uuid;
                return true;
            }

            this.CurrentKey = uuid;

            if (elem is GLTFMaterial)
            {
                var mat = this.GetElement(uuid) as GLTFMaterial;
                mat.doubleSided = doubleSided;
            }

            return false;
        }

        /// <summary>
        /// Check if the container already has an item with this key.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item.</param>
        /// <returns>Returns TRUE if the dictionary contains the given element, otherwise, returns FALSE.</returns>
        public bool Contains(string uuid)
        {
            return this.dict.ContainsKey(uuid);
        }

        /// <summary>
        /// Returns the index for an item given it's unique identifier.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item.</param>
        /// <returns>index of item or -1. </returns>
        public int GetIndexFromUUID(string uuid)
        {
            try
            {
                return this.dict[uuid];
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("Specified item could not be found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting the specified item {ex.Message}");
            }
        }

        /// <summary>
        /// Returns an item given it's unique identifier.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item. </param>
        /// <returns>Element.</returns>
        public T GetElement(string uuid)
        {
            int index = this.GetIndexFromUUID(uuid);
            return this.List[index];
        }

        public void Reset()
        {
            this.dict.Clear();
            this.List.Clear();
            this.Dict.Clear();
            this.CurrentKey = string.Empty;
        }
    }
}
