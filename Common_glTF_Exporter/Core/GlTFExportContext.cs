using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Grid = Autodesk.Revit.DB.Grid;
using Transform = Autodesk.Revit.DB.Transform;
using Common_glTF_Exporter.Export;
using Common_glTF_Exporter.Utils;
using Common_glTF_Exporter.Windows.MainWindow;

namespace Revit_glTF_Exporter
{
    class glTFExportContext : IExportContext
    {
        static public bool RetainCurvedSurfaceFacets = false;

        // Unit conversion factors.

        const double _mm_per_inch = 25.4;
        const double _inch_per_foot = 12;
        const double _foot_to_mm = _inch_per_foot * _mm_per_inch;
        private Document _doc;
        private bool _skipElementFlag = false;
        private Element _element;
        private ProgressBarWindow _progressBarWindow;

        #if REVIT2019 || REVIT2020

        private DisplayUnitType _displayUnitType;

        #else

        private ForgeTypeId _forgeTypeId;

        #endif

        private Preferences _preferences;
        /**
         * The following properties are the root
         * elements of the glTF format spec. They
         * will be serialized into the final *.gltf file
         **/

        /// <summary>
        /// List of root nodes defining scenes.
        /// </summary>
        public List<glTFScene> Scenes = new List<glTFScene>();
        /// <summary>
        /// Stateful, uuid indexable list for all nodes in the export.
        /// </summary>
        public IndexedDictionary<glTFNode> Nodes { get; } = new IndexedDictionary<glTFNode>();
        /// <summary>
        /// Stateful, uuid indexable list for all meshes in the export.
        /// </summary>
        public IndexedDictionary<glTFMesh> Meshes { get; } = new IndexedDictionary<glTFMesh>();
        /// <summary>
        /// Stateful, uuid indexable list for all materials in the export.
        /// </summary>
        public IndexedDictionary<glTFMaterial> Materials = new IndexedDictionary<glTFMaterial>();
        /// <summary>
        /// List of all buffers referencing the binary file data.
        /// </summary>
        public List<glTFBuffer> Buffers { get; } = new List<glTFBuffer>();
        /// <summary>
        /// List of all BufferViews referencing the buffers.
        /// </summary>
        public List<glTFBufferView> BufferViews { get; } = new List<glTFBufferView>();
        /// <summary>
        /// List of all Accessors referencing the BufferViews.
        /// </summary>
        public List<glTFAccessor> Accessors { get; } = new List<glTFAccessor>();

        /// <summary>
        /// Container for the vertex/face/normal information that will be serialized into a binary
        /// format for the final *.bin files.
        /// </summary>
        public List<glTFBinaryData> binaryFileData { get; } = new List<glTFBinaryData>();

        /**
         * The following properties are private to this class
         * and used only for intermediate steps of the conversion.
         **/

        /// <summary>
        /// Reference to the rootNode to add children
        /// </summary>
        private glTFNode rootNode;

        /// <summary>
        /// Stateful, uuid indexable list of intermediate geometries for the element currently being
        /// processed, keyed by material. This is re-initialized on each new element.
        /// </summary>
        private IndexedDictionary<GeometryData> _currentGeometry;
        /// <summary>
        /// Stateful, uuid indexable list of intermediate vertex data for the element currently
        /// being processed, keyed by material. This is re-initialized on each new element.
        /// </summary>
        private IndexedDictionary<VertexLookupInt> _currentVertices;

        private Stack<Transform> _transformStack = new Stack<Transform>();
        private Transform CurrentTransform { get { return _transformStack.Peek(); } }

        public glTFExportContext(Document doc,

            #if REVIT2019 || REVIT2020

            DisplayUnitType displayUnitType,

            #else

            ForgeTypeId forgeTypeId,

            #endif

            ProgressBarWindow progressBarWindow)
        {
            _preferences = Common_glTF_Exporter.Windows.MainWindow.Settings.GetInfo();
            _doc = doc;
            _progressBarWindow = progressBarWindow;

#if REVIT2019 || REVIT2020

            _displayUnitType = displayUnitType;

#else

            _forgeTypeId = forgeTypeId;

#endif
        }

        /// <summary>
        /// Runs once at beginning of export. Sets up the root node and scene.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            _transformStack.Push(Transform.Identity);

            rootNode = new glTFNode();
            rootNode.name = "rootNode";
            rootNode.children = new List<int>();

            Nodes.AddOrUpdateCurrent("rootNode", rootNode);

            glTFScene defaultScene = new glTFScene();
            defaultScene.nodes.Add(0);
            Scenes.Add(defaultScene);

            return true;
        }

        /// <summary>
        /// Runs once at end of export. Serializes the gltf properties and wites out the *.gltf and
        /// *.bin files.
        /// </summary>
        public void Finish()
        {
            // TODO: [RM] Standardize what non glTF spec elements will go into this "BIM glTF
            // superset" and write a spec for it. Gridlines below are an example.

            // Add gridlines as gltf nodes in the format: Origin {Vec3<double>}, Direction
            // {Vec3<double>}, Length {double}

            if (_preferences.grids)
            {
                FilteredElementCollector col = new FilteredElementCollector(_doc)
                    .OfClass(typeof(Grid));

                var grids = col.ToElements();

                foreach (Grid g in grids)
                {
                    Line l = g.Curve as Line;

                    var origin = l.Origin;
                    var direction = l.Direction;
                    var length = l.Length;

                    var xtras = new glTFExtras();
                    var grid = new GridParameters();

                    #if REVIT2019 || REVIT2020

                    grid.origin = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(origin.X, _displayUnitType),
                    Util.ConvertFeetToUnitTypeId(origin.Y, _displayUnitType),
                    Util.ConvertFeetToUnitTypeId(origin.Z, _displayUnitType) };

                    grid.direction = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(direction.X, _displayUnitType),
                    Util.ConvertFeetToUnitTypeId(direction.Y, _displayUnitType),
                    Util.ConvertFeetToUnitTypeId(direction.Z, _displayUnitType) };

                    grid.length = Util.ConvertFeetToUnitTypeId(length, _displayUnitType);

                    #else

                    grid.origin = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(origin.X, _forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(origin.Y, _forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(origin.Z, _forgeTypeId) };

                    grid.direction = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(direction.X, _forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(direction.Y, _forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(direction.Z, _forgeTypeId) };

                    grid.length = Util.ConvertFeetToUnitTypeId(length, _forgeTypeId);

                    #endif

                    xtras.GridParameters = grid;
                    xtras.UniqueId = g.UniqueId;
                    xtras.parameters = Util.GetElementParameters(g, true);

                    var gridNode = new glTFNode();
                    gridNode.name = g.Name;
                    gridNode.extras = xtras;

                    Nodes.AddOrUpdateCurrent(g.UniqueId, gridNode);
                    rootNode.children.Add(Nodes.CurrentIndex);
                }
            }

            Binaries.Save(_preferences.singleBinary, BufferViews, _preferences.fileName, Buffers,
                _preferences.path, binaryFileData, _preferences.batchId, _preferences.normals);

            GltfFile.Create(Scenes, Nodes.List, Meshes.List, Materials.List,
                Buffers, BufferViews, Accessors, _preferences.path, _preferences.batchId, _preferences.normals);

            Compression.Run(_preferences.path, _preferences.compression);
        }

        /// <summary>
        /// Runs once for each element, we create a new glTFNode and glTF Mesh keyed to the elements
        /// uuid, and reset the "_current" variables.
        /// </summary>
        /// <param name="elementId">ElementId of Element being processed</param>
        /// <returns></returns>
        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            _progressBarWindow.ViewModel.ProgressBarValue++;

            _element = _doc.GetElement(elementId);

            if (_element.Category.Name == "Cameras" ||
                (_element is Level && !_preferences.levels))
            {
                return RenderNodeAction.Skip;
            }

            if (Nodes.Contains(_element.UniqueId))
            {
                // Duplicate element, skip adding.
                _skipElementFlag = true;
                return RenderNodeAction.Skip;
            }

            // create a new node for the element
            glTFNode newNode = new glTFNode();

            newNode.name = Util.ElementDescription(_element);

            if (_preferences.exportProperties)
            {
                // get the extras for this element
                glTFExtras extras = new glTFExtras();

                extras.UniqueId = _element.UniqueId;

                extras.parameters = Util.GetElementParameters(_element, true);

                if (_preferences.elementId)
                {
                    extras.elementId = _element.Id.IntegerValue;
                }

                extras.elementCategory = _element.Category.Name;

                newNode.extras = extras;
            }

            Nodes.AddOrUpdateCurrent(_element.UniqueId, newNode);

            // add the index of this node to our root node children array
            rootNode.children.Add(Nodes.CurrentIndex);

            // Reset _currentGeometry for new element
            _currentGeometry = new IndexedDictionary<GeometryData>();
            _currentVertices = new IndexedDictionary<VertexLookupInt>();

            //GetGeometryData();

            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// Runs every time, and immediately prior to, a mesh being processed (OnPolymesh). It
        /// supplies the material for the mesh, and we use this to create a new material in our
        /// material container, or switch the current material if it already exists.
        /// </summary>
        /// <param name="node"></param>
        public void OnMaterial(MaterialNode node)
        {
            if (_preferences.materials)
            {
                RevitMaterials.Export(node, _doc, ref Materials);
            }            
        }

        /// <summary>
        /// Runs for every polymesh being processed. Typically this is a single face of an element's
        /// mesh. Here we populate the data into our "_current" variables (geometry and vertices)
        /// keyed on the element/material combination (this is important because within a single
        /// element, materials can be changed and repeated in unknown order).
        /// </summary>
        /// <param name="polymesh"></param>
        public void OnPolymesh(PolymeshTopology polymesh)
        {
            string vertex_key = Nodes.CurrentKey + "_" + Materials.CurrentKey;

            // Add new "_current" entries if vertex_key is unique
            _currentGeometry.AddOrUpdateCurrent(vertex_key, new GeometryData());
            _currentVertices.AddOrUpdateCurrent(vertex_key, new VertexLookupInt());

            // populate current vertices vertex data and current geometry faces data
            Transform transform = CurrentTransform;
            IList<XYZ> pts = polymesh.GetPoints();
            pts = pts.Select(p => transform.OfPoint(p)).ToList();
            IList<PolymeshFacet> facets = polymesh.GetFacets();

            foreach (PolymeshFacet facet in facets)
            {
#if REVIT2019 || REVIT2020

                int v1 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V1], _preferences.flipAxis, _displayUnitType));
                int v2 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V2], _preferences.flipAxis, _displayUnitType));
                int v3 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V3], _preferences.flipAxis, _displayUnitType));

#else

                int v1 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V1], _preferences.flipAxis, _forgeTypeId));
                int v2 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V2], _preferences.flipAxis, _forgeTypeId));
                int v3 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V3], _preferences.flipAxis, _forgeTypeId));

#endif

                _currentGeometry.CurrentItem.faces.Add(v1);
                _currentGeometry.CurrentItem.faces.Add(v2);
                _currentGeometry.CurrentItem.faces.Add(v3);
            }

            if (_preferences.normals)
            {
                glTFExportUtils.AddNormals(_preferences.flipAxis, transform, polymesh, _currentGeometry.CurrentItem.normals);
            }
        }

        /// <summary>
        /// Runs at the end of an element being processed, after all other calls for that element.
        /// Here we compile all the "_current" variables (geometry and vertices) onto glTF buffers.
        /// We do this at OnElementEnd because it signals no more meshes or materials are coming for
        /// this element.
        /// </summary>
        /// <param name="elementId"></param>
        public void OnElementEnd(ElementId elementId)
        {
            if (_element.Category.Name == "Cameras" ||
            _currentVertices == null ||
            _currentVertices.List.Count == 0)
            {
                return;
            }

            if (_skipElementFlag)
            {
                // Duplicate element, skip.
                _skipElementFlag = false;
                return;
            }

            // create a new mesh for the node (we're assuming 1 mesh per node w/ multiple primitives
            // on mesh)
            glTFMesh newMesh = new glTFMesh();
            newMesh.name = _element.Name;
            newMesh.primitives = new List<glTFMeshPrimitive>();
            Meshes.AddOrUpdateCurrent(_element.UniqueId, newMesh);

            // add the index of this mesh to the current node.
            Nodes.CurrentItem.mesh = Meshes.CurrentIndex;

            // Add vertex data to _currentGeometry for each geometry/material pairing
            foreach (KeyValuePair<string, VertexLookupInt> kvp in _currentVertices.Dict)
            {
                string vertex_key = kvp.Key;

                foreach (KeyValuePair<PointInt, int> p in kvp.Value)
                {
                    _currentGeometry.GetElement(vertex_key).vertices.Add(p.Key.X);
                    _currentGeometry.GetElement(vertex_key).vertices.Add(p.Key.Y);
                    _currentGeometry.GetElement(vertex_key).vertices.Add(p.Key.Z);
                }
            }

            // Convert _currentGeometry objects into glTFMeshPrimitives
            foreach (KeyValuePair<string, GeometryData> kvp in _currentGeometry.Dict)
            {
                glTFBinaryData elementBinary = glTFExportUtils.AddGeometryMeta(Buffers, Accessors, BufferViews, kvp.Value, kvp.Key,
                    elementId.IntegerValue, _preferences.batchId, _preferences.normals);

                binaryFileData.Add(elementBinary);

                string material_key = kvp.Key.Split('_')[1];

                glTFMeshPrimitive primitive = new glTFMeshPrimitive();

                primitive.attributes.POSITION = elementBinary.vertexAccessorIndex;

                if (_preferences.normals)
                {
                    primitive.attributes.NORMAL = elementBinary.normalsAccessorIndex;
                }

                if (_preferences.batchId)
                {
                    primitive.attributes._BATCHID = elementBinary.batchIdAccessorIndex;
                }

                primitive.indices = elementBinary.indexAccessorIndex;

                if (_preferences.materials)
                {
                    primitive.material = Materials.GetIndexFromUUID(material_key);
                }

                Meshes.CurrentItem.primitives.Add(primitive);

                Meshes.CurrentItem.name = _element.Name;
            }
        }

        /// <summary>
        /// This is called when family instances are encountered, immediately after OnElementBegin.
        /// We're using it here to maintain the transform stack for that element's heirarchy.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            var transform = node.GetTransform();
            var transformationMutiply = CurrentTransform.Multiply(transform);
            _transformStack.Push(transformationMutiply);

            // We can either skip this instance or proceed with rendering it.
            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// This is called when family instances are encountered, immediately before OnElementEnd.
        /// We're using it here to maintain the transform stack for that element's heirarchy.
        /// </summary>
        /// <param name="node"></param>
        public void OnInstanceEnd(InstanceNode node)
        {
            // Note: This method is invoked even for instances that were skipped.
            _transformStack.Pop();
        }

        public bool IsCanceled()
        {
            // This method is invoked many times during the export process.
            return false;
        }

        public RenderNodeAction OnViewBegin(ViewNode node)
        {
            // TODO: we could use this to handle multiple scenes in the gltf file.
            return RenderNodeAction.Proceed;
        }

        public void OnViewEnd(ElementId elementId)
        {
            // do nothing
        }

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            _transformStack.Push(
                CurrentTransform.Multiply(node.GetTransform())
            );

            // We can either skip this instance or proceed with rendering it.
            return RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(LinkNode node)
        {
            // Note: This method is invoked even for instances that were skipped.
            _transformStack.Pop();
        }

        public RenderNodeAction

            Begin(FaceNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnFaceEnd(FaceNode node)
        {
            // This method is invoked only if the custom exporter was set to include faces.
        }

        public void OnRPC(RPCNode node)
        {
            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.View = _doc.ActiveView;

            GeometryElement geoEle = _element.get_Geometry(opt);

            foreach (GeometryObject geoObject in geoEle)
            {
                if (geoObject is GeometryInstance)
                {
                    GeometryInstance geoInst = geoObject as GeometryInstance;

                    foreach (var geoObj in geoInst.GetInstanceGeometry())
                    {
                        if (geoObj is Mesh)
                        {
                            Mesh mesh = geoObj as Mesh;
                            int triangles = mesh.NumTriangles;

                            if (triangles == 0)
                                continue;

                            glTFMaterial gl_mat = new glTFMaterial();
                            Material material = Util.GetMeshMaterial(_doc, mesh);

                            if (_preferences.materials)
                            {
                                if (material == null)
                                {
                                    material = Collectors.GetRandomMaterial(_doc);
                                }
                                gl_mat = Util.GetGLTFMaterial(Materials.List, material);
                                Materials.AddOrUpdateCurrent(material.UniqueId, gl_mat);
                            }

                            // Add new "_current" entries if vertex_key is unique
                            string vertex_key = Nodes.CurrentKey + "_" + Materials.CurrentKey;
                            _currentGeometry.AddOrUpdateCurrent(vertex_key, new GeometryData());
                            _currentVertices.AddOrUpdateCurrent(vertex_key, new VertexLookupInt());

                            for (int i = 0; i < triangles; i++)
                            {
                                try
                                {
                                    MeshTriangle triangle = mesh.get_Triangle(i);

                                    if (triangle == null)
                                        continue;

#if REVIT2019 || REVIT2020

                                    int v1 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(0), _preferences.flipAxis, _displayUnitType));
                                    int v2 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(1), _preferences.flipAxis, _displayUnitType));
                                    int v3 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(2), _preferences.flipAxis, _displayUnitType));

#else

                                    int v1 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(0), _preferences.flipAxis, _forgeTypeId));
                                    int v2 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(1), _preferences.flipAxis, _forgeTypeId));
                                    int v3 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(2), _preferences.flipAxis, _forgeTypeId));

#endif

                                    _currentGeometry.CurrentItem.faces.Add(v1);
                                    _currentGeometry.CurrentItem.faces.Add(v2);
                                    _currentGeometry.CurrentItem.faces.Add(v3);

                                    if (_preferences.normals)
                                    {
                                        XYZ side1 = triangle.get_Vertex(1) - (triangle.get_Vertex(0));
                                        XYZ side2 = triangle.get_Vertex(2) - triangle.get_Vertex(0);
                                        XYZ normal = side1.CrossProduct(side2);

                                        normal = normal.Normalize();

                                        if (_preferences.flipAxis)
                                        {
                                            normal = normal.FlipCoordinates();
                                        }

                                        for (int j = 0; j < 3; j++)
                                        {
                                            _currentGeometry.CurrentItem.normals.Add(normal.X);
                                            _currentGeometry.CurrentItem.normals.Add(normal.Y);
                                            _currentGeometry.CurrentItem.normals.Add(normal.Z);
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }
        public void OnLight(LightNode node)
        {
            // do nothing
        }

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            return RenderNodeAction.Proceed;
            //nothing
        }
    }
}