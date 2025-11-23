using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Model
{
    public class BaseObject
    {
        public List<BaseImage> images;
        public List<BaseTexture> textures;
        public List<BaseMaterial> materials;
        public List<BaseNode> nodes;
    }
}
