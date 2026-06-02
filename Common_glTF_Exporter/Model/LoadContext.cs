#if REVIT2025 || REVIT2026 || REVIT2027
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