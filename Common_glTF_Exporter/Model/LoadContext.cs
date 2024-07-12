#if REVIT2025
using System.Runtime.Loader;

namespace Common_glTF_Exporter.Model
{
    public class NonCollectibleAssemblyLoadContext : AssemblyLoadContext
    {
        public NonCollectibleAssemblyLoadContext() : base(isCollectible: false)
        {
        }
    }
   
}
#endif