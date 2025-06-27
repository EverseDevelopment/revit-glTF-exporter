using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Model
{
    public class MaterialCacheDTO
    {
        public MaterialCacheDTO(string materialName, string uniqueId)
        {
            MaterialName = materialName;
            UniqueId = uniqueId;
        }

        public string MaterialName { get; set; }
        public string UniqueId { get; set; }
    }
}
