using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Autodesk.Revit.DB;
using System.Diagnostics;
using Grid = Autodesk.Revit.DB.Grid;
using Common_glTF_Exporter.Export;
using Common_glTF_Exporter.Utils;
using Autodesk.Revit.UI;

namespace Revit_glTF_Exporter
{
    class glTFExportContext : IExportContext
    {
        private Document _doc;
        private bool _skipElementFlag = false;
        private Element _element;
        private bool _isRPC = false;

#if REVIT2021 || REVIT2022 || REVIT2023
        private ForgeTypeId _forgeTypeId;
#endif

#if REVIT2019 || REVIT2020
        private DisplayUnitType _displayUnitType;
#endif
        /// <summary>
        /// Flag to write coords as Z up instead of Y up (if true).
        /// </summary>
        private bool _flipCoords;
        /// <summary>
        /// Flag to export all the properties for each element.
        /// </summary>
        private bool _exportProperties;
        /// <summary>
        /// Flag to export all buffers into a single .bin file (if true).
        /// </summary>
        private bool _singleBinary;

        /// <summary>
        /// The name for the .gltf file.
        /// </summary>
        private string _filename;
        /// <summary>
        /// The directory for the .bin files.
        /// </summary>
        private string _directory;

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
        public IndexedDictionary<glTFMaterial> Materials { get; } = new IndexedDictionary<glTFMaterial>();
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
        /// Container for the vertex/face/normal information
        /// that will be serialized into a binary format
        /// for the final *.bin files.
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
        /// Stateful, uuid indexable list of intermediate geometries for
        /// the element currently being processed, keyed by material. This
        /// is re-initialized on each new element.
        /// </summary>
        private IndexedDictionary<GeometryData> _currentGeometry;
        /// <summary>
        /// Stateful, uuid indexable list of intermediate vertex data for
        /// the element currently being processed, keyed by material. This
        /// is re-initialized on each new element.
        /// </summary>
        private IndexedDictionary<VertexLookupInt> _currentVertices;

        private Stack<Transform> _transformStack = new Stack<Transform>();
        private Transform CurrentTransform { get { return _transformStack.Peek(); } }
        private bool _exportMaterials;
        public glTFExportContext(Document doc, string filename, string directory,

            #if REVIT2021 || REVIT2022 || REVIT2023

            ForgeTypeId forgeTypeId,

            #elif REVIT2019 || REVIT2020

            DisplayUnitType displayUnitType,

            #endif

            bool singleBinary = true, bool exportProperties = true, bool flipCoords = true, bool exportMaterials = true)

        {
            _doc = doc;
            _exportProperties = exportProperties;
            _flipCoords = flipCoords;
            _singleBinary = singleBinary;
            _filename = filename;
            _directory = directory;
            _exportMaterials = exportMaterials;

            #if REVIT2021 || REVIT2022 || REVIT2023

            _forgeTypeId = forgeTypeId;

            #elif REVIT2019 || REVIT2020

           _displayUnitType = displayUnitType;

            #endif
        }

        /// <summary>
        /// Runs once at beginning of export. Sets up the root node
        /// and scene.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            _transformStack.Push(Transform.Identity);

            //float scale = 1f; // could play with this to match units in a different viewer.

            rootNode = new glTFNode();
            rootNode.name = "rootNode";

            //rootNode.matrix = new List<float>()
            //{
            //    scale, 0, 0, 0,
            //    0, scale, 0, 0,
            //    0, 0, scale, 0,
            //    0, 0, 0, scale
            //};

            rootNode.children = new List<int>();
            Nodes.AddOrUpdateCurrent("rootNode", rootNode);

            glTFScene defaultScene = new glTFScene();
            defaultScene.nodes.Add(0);
            Scenes.Add(defaultScene);

            return true;
        }

        /// <summary>
        /// Runs once at end of export. Serializes the gltf
        /// properties and wites out the *.gltf and *.bin files.
        /// </summary>
        public void Finish()
        {
            // TODO: [RM] Standardize what non glTF spec elements will go into
            // this "BIM glTF superset" and write a spec for it. Gridlines below
            // are an example.

            // Add gridlines as gltf nodes in the format:
            // Origin {Vec3<double>}, Direction {Vec3<double>}, Length {double}
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

                #if REVIT2021 || REVIT2022 || REVIT2023

                grid.origin = new List<double>() { 
                    Util.ConvertFeetToUnitTypeId(origin.X, _forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(origin.Y, _forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(origin.Z, _forgeTypeId) };

                grid.direction = new List<double>() { 
                    Util.ConvertFeetToUnitTypeId(direction.X, _forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(direction.Y, _forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(direction.Z, _forgeTypeId) };

                grid.length = Util.ConvertFeetToUnitTypeId(length, _forgeTypeId);

                #elif REVIT2019 || REVIT2020


                grid.origin = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(origin.X, _displayUnitType),
                    Util.ConvertFeetToUnitTypeId(origin.Y, _displayUnitType),
                    Util.ConvertFeetToUnitTypeId(origin.Z, _displayUnitType) };

                grid.direction = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(direction.X, _displayUnitType),
                    Util.ConvertFeetToUnitTypeId(direction.Y, _displayUnitType),
                    Util.ConvertFeetToUnitTypeId(direction.Z, _displayUnitType) };

                grid.length = Util.ConvertFeetToUnitTypeId(length, _displayUnitType);

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

            if (_singleBinary)
            {
                int bytePosition = 0;
                int currentBuffer = 0;
                foreach (var view in BufferViews)
                {
                    if (view.buffer == 0)
                    {
                        bytePosition += view.byteLength;
                        continue;
                    }

                    if (view.buffer != currentBuffer)
                    {
                        view.buffer = 0;
                        view.byteOffset = bytePosition;
                        bytePosition += view.byteLength;
                    }
                }

                glTFBuffer buffer = new glTFBuffer();
                string bufferUri = String.Concat(Path.GetFileNameWithoutExtension(_filename), ".bin");
                buffer.uri = bufferUri;
                buffer.byteLength = bytePosition;
                Buffers.Clear();
                Buffers.Add(buffer);

                BinFile.Create(_directory, bufferUri, binaryFileData);
            }
            else        
            {
                foreach (var bin in binaryFileData)
                {
                    using (FileStream f = File.Create(_directory + bin.name))
                    {
                        using (BinaryWriter writer = new BinaryWriter(f))
                        {
                            foreach (var coord in bin.vertexBuffer)
                            {
                                writer.Write((float)coord);
                            }
                            foreach (var index in bin.indexBuffer)
                            {
                                writer.Write((int)index);
                            }
                        }
                    }
                }
            }

            GltfFile.Create(Scenes, Nodes.List, Meshes.List, Materials.List, 
                Buffers, BufferViews, Accessors, _filename);
        }

        /// <summary>
        /// Runs once for each element, we create a new glTFNode and glTF Mesh
        /// keyed to the elements uuid, and reset the "_current" variables.
        /// </summary>
        /// <param name="elementId">ElementId of Element being processed</param>
        /// <returns></returns>
        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            _element = _doc.GetElement(elementId);

            if (Nodes.Contains(_element.UniqueId))
            {
                // Duplicate element, skip adding.
                _skipElementFlag = true;
                return RenderNodeAction.Skip;
            }

            // create a new node for the element
            glTFNode newNode = new glTFNode();

            newNode.name = Util.ElementDescription(_element);

            if (_exportProperties)
            {
                // get the extras for this element
                glTFExtras extras = new glTFExtras();

                extras.UniqueId = _element.UniqueId;
                extras.parameters = Util.GetElementParameters(_element, true);

                //extras.elementId = e.Id.IntegerValue;
                extras.elementCategory = _element.Category.Name;
                //extras.dependentElements = Util.GetDependentElements(e);

                newNode.extras = extras;
                Nodes.AddOrUpdateCurrent(_element.UniqueId, newNode);

                // add the index of this node to our root node children array
                rootNode.children.Add(Nodes.CurrentIndex);
            }

            // Reset _currentGeometry for new element
            _currentGeometry = new IndexedDictionary<GeometryData>();
            _currentVertices = new IndexedDictionary<VertexLookupInt>();

            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// Runs every time, and immediately prior to, a mesh being processed (OnPolymesh).
        /// It supplies the material for the mesh, and we use this to create a new material
        /// in our material container, or switch the current material if it already exists.
        /// </summary>
        /// <param name="node"></param>
        public void OnMaterial(MaterialNode node)
        {
            if (!_exportMaterials)
            {
                return;
            }

            string matName;
            ElementId id = node.MaterialId;
            glTFMaterial gl_mat = new glTFMaterial();
            float opacity = 1 - (float)node.Transparency;
            glTFMaterial current_gl_mat = null;

            if (id != ElementId.InvalidElementId)
            {
                // construct a material from the node
                Element m = _doc.GetElement(node.MaterialId);
                matName = m.Name;

                // construct the material
                gl_mat.name = matName;
                glTFPBR pbr = new glTFPBR();
                pbr.baseColorFactor = new List<float>() { node.Color.Red / 255f, node.Color.Green / 255f, node.Color.Blue / 255f, opacity };
                pbr.metallicFactor = 0f;
                pbr.roughnessFactor = 1f;
                gl_mat.pbrMetallicRoughness = pbr;

                Materials.AddOrUpdateCurrent(m.UniqueId, gl_mat);
            }
            else
            {
                // I'm really not sure what situation this gets triggered in?
                // make your own damn material!
                // (currently this is equivalent to above until I understand BlinnPhong/PBR conversion better)
                string uuid = Guid.NewGuid().ToString();

                // construct the material
                matName = string.Format("MaterialNode_{0}_{1}", Util.ColorToInt(node.Color), Util.RealString(opacity * 100));
                gl_mat.name = matName;
                glTFPBR pbr = new glTFPBR();
                pbr.baseColorFactor = new List<float>() { node.Color.Red / 255f, node.Color.Green / 255f, node.Color.Blue / 255f, opacity };
                pbr.metallicFactor = 0f;
                pbr.roughnessFactor = 1f;
                gl_mat.pbrMetallicRoughness = pbr;

                // prevent duplicated materials
                current_gl_mat = Materials.List
                    .FirstOrDefault(x => x.name == matName);

                if (current_gl_mat == null)
                {
                    Materials.AddOrUpdateCurrent(uuid, gl_mat);
                }
            }
        }

        /// <summary>
        /// Runs for every polymesh being processed. Typically this is a single face
        /// of an element's mesh. Here we populate the data into our "_current" variables
        /// (geometry and vertices) keyed on the element/material combination (this is important
        /// because within a single element, materials can be changed and repeated in unknown order).
        /// </summary>
        /// <param name="polymesh"></param>
        public void OnPolymesh(PolymeshTopology polymesh)
        {
            string vertex_key = String.Concat(Nodes.CurrentKey, "_", Materials.CurrentKey);

            // Add new "_current" entries if vertex_key is unique
            _currentGeometry.AddOrUpdateCurrent(vertex_key, new GeometryData());
            _currentVertices.AddOrUpdateCurrent(vertex_key, new VertexLookupInt());

            // populate current vertices vertex data and current geometry faces data
            Transform t = CurrentTransform;
            IList<XYZ> pts = polymesh.GetPoints();
            pts = pts.Select(p => t.OfPoint(p)).ToList();
            IList<PolymeshFacet> facets = polymesh.GetFacets();

            foreach (PolymeshFacet facet in facets)
            {
                #if REVIT2021 || REVIT2022 || REVIT2023

                int v1 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V1], _flipCoords, _forgeTypeId));
                int v2 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V2], _flipCoords, _forgeTypeId));
                int v3 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V3], _flipCoords, _forgeTypeId));

                #elif REVIT2019 || REVIT2020

                int v1 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V1], _flipCoords, _displayUnitType));
                int v2 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V2], _flipCoords, _displayUnitType));
                int v3 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V3], _flipCoords, _displayUnitType));

                #endif

                _currentGeometry.CurrentItem.faces.Add(v1);
                _currentGeometry.CurrentItem.faces.Add(v2);
                _currentGeometry.CurrentItem.faces.Add(v3);
            }
        }

        /// <summary>
        /// Runs at the end of an element being processed, after all other calls for that element.
        /// Here we compile all the "_current" variables (geometry and vertices) onto glTF buffers.
        /// We do this at OnElementEnd because it signals no more meshes or materials are
        /// coming for this element.
        /// </summary>
        /// <param name="elementId"></param>
        public void OnElementEnd(ElementId elementId)
        {
            if (_skipElementFlag)
            {
                // Duplicate element, skip.
                _skipElementFlag = false;
                return;
            }

            if (!_isRPC && _currentVertices.List.Count == 0)
            {
                return;
            }

            Element e = _doc.GetElement(elementId);

            // create a new mesh for the node (we're assuming 1 mesh per node w/ multiple primatives on mesh)
            glTFMesh newMesh = new glTFMesh();
            newMesh.primitives = new List<glTFMeshPrimitive>();
            Meshes.AddOrUpdateCurrent(e.UniqueId, newMesh);

            //TaskDialog.Show("debug", Meshes.List.Count.ToString());

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
                glTFBinaryData elementBinary = AddGeometryMeta(kvp.Value, kvp.Key, elementId.IntegerValue);
                binaryFileData.Add(elementBinary);

                string material_key = kvp.Key.Split('_')[1];

                glTFMeshPrimitive primitive = new glTFMeshPrimitive();
                primitive.attributes.POSITION = elementBinary.vertexAccessorIndex;

                primitive.indices = elementBinary.indexAccessorIndex;
                
                if(_exportMaterials)
                { 
                  primitive.material = Materials.GetIndexFromUUID(material_key);
                }

                Meshes.CurrentItem.primitives.Add(primitive);
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

        /// <summary>
        /// Takes the intermediate geometry data and performs the calculations
        /// to convert that into glTF buffers, views, and accessors.
        /// </summary>
        /// <param name="geomData"></param>
        /// <param name="name">Unique name for the .bin file that will be produced.</param>
        /// <param name="elementId">Revit element's Element ID that will be used as the batchId value.</param>
        /// <returns></returns>
        public glTFBinaryData AddGeometryMeta(GeometryData geomData, string name, int elementId)
        {
            // add a buffer
            glTFBuffer buffer = new glTFBuffer();
            buffer.uri = String.Concat(name,".bin" );
            Buffers.Add(buffer);
            int bufferIdx = Buffers.Count - 1;

            /**
             * Buffer Data
             **/
            glTFBinaryData bufferData = new glTFBinaryData();
            bufferData.name = buffer.uri;

            foreach (var coord in geomData.vertices)
            {
                float vFloat = Convert.ToSingle(coord);
                bufferData.vertexBuffer.Add(vFloat);
            }

            // TODO: Leaving this to export normals in the future
            //foreach (var normal in geomData.normals)
            //{
            //    bufferData.normalBuffer.Add((float)normal);
            //}

            foreach (var index in geomData.faces)
            {
                bufferData.indexBuffer.Add(index);
            }

            // Get max and min for vertex data
            float[] vertexMinMax = Util.GetVec3MinMax(bufferData.vertexBuffer);

            // Get max and min for index data
            int[] faceMinMax = Util.GetScalarMinMax(bufferData.indexBuffer);

            /**
             * BufferViews
             **/
            // Add a vec3 buffer view
            int elementsPerVertex = 3;
            int bytesPerElement = 4;
            int bytesPerVertex = elementsPerVertex * bytesPerElement;
            int numVec3 = (geomData.vertices.Count) / elementsPerVertex;
            int sizeOfVec3View = numVec3 * bytesPerVertex;
            glTFBufferView vec3View = new glTFBufferView();
            vec3View.buffer = bufferIdx;
            vec3View.byteOffset = 0;
            vec3View.byteLength = sizeOfVec3View;
            vec3View.target = Targets.ARRAY_BUFFER;
            BufferViews.Add(vec3View);
            int vec3ViewIdx = BufferViews.Count - 1;

            // Add a faces / indexes buffer view
            int elementsPerIndex = 1;
            int bytesPerIndexElement = 4;
            int bytesPerIndex = elementsPerIndex * bytesPerIndexElement;
            int numIndexes = geomData.faces.Count;
            int sizeOfIndexView = numIndexes * bytesPerIndex;
            glTFBufferView facesView = new glTFBufferView();
            facesView.buffer = bufferIdx;
            facesView.byteOffset = vec3View.byteLength;

            facesView.byteLength = sizeOfIndexView;
            facesView.target = Targets.ELEMENT_ARRAY_BUFFER;
            BufferViews.Add(facesView);
            int facesViewIdx = BufferViews.Count - 1;

            /**
             * Accessors
             **/
            // add a position accessor
            glTFAccessor positionAccessor = new glTFAccessor();
            positionAccessor.bufferView = vec3ViewIdx;
            positionAccessor.byteOffset = 0;
            positionAccessor.componentType = ComponentType.FLOAT;
            positionAccessor.count = geomData.vertices.Count / elementsPerVertex;
            positionAccessor.type = "VEC3";
            positionAccessor.max = new List<float>() { vertexMinMax[1], vertexMinMax[3], vertexMinMax[5] };
            positionAccessor.min = new List<float>() { vertexMinMax[0], vertexMinMax[2], vertexMinMax[4] };

            Accessors.Add(positionAccessor);
            bufferData.vertexAccessorIndex = Accessors.Count - 1;

            // add a face accessor
            glTFAccessor faceAccessor = new glTFAccessor();
            faceAccessor.bufferView = facesViewIdx;
            faceAccessor.byteOffset = 0;
            faceAccessor.componentType = ComponentType.UNSIGNED_INT;

            faceAccessor.count = geomData.faces.Count / elementsPerIndex;
            faceAccessor.type = "SCALAR";
            faceAccessor.max = new List<float>() { faceMinMax[1] };
            faceAccessor.min = new List<float>() { faceMinMax[0] };
            faceAccessor.name = "FACE";
            Accessors.Add(faceAccessor);
            bufferData.indexAccessorIndex = Accessors.Count - 1;

            return bufferData;
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

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnFaceEnd(FaceNode node)
        {
            // This method is invoked only if the 
            // custom exporter was set to include faces.
        }

        public void OnRPC(RPCNode node)
        {
            _isRPC = true;

            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.View = _doc.ActiveView;

            GeometryElement geoEle = _element.get_Geometry(opt);

            // Add new "_current" entries if vertex_key is unique
            string vertex_key = Nodes.CurrentKey + "_" + Materials.CurrentKey;

            _currentGeometry.AddOrUpdateCurrent(vertex_key, new GeometryData());
            _currentVertices.AddOrUpdateCurrent(vertex_key, new VertexLookupInt());

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

                            if (material == null && _exportMaterials)
                            {
                                material = Collectors.GetRandomMaterial(_doc);
                                gl_mat = Util.GetGLTFMaterial(Materials.List, material);
                            }
                            else if (material == null && !_exportMaterials)
                            {
                                material = Collectors.GetRandomMaterial(_doc);
                                //TODO: set gl_mat to the default material for all the elements
                                //gl_mat = Util.GetGLTFMaterial(Materials.List, material);
                            }
                            else if (material != null && _exportMaterials)
                            {
                                gl_mat = Util.GetGLTFMaterial(Materials.List, material);
                            }
                            else if (material != null && !_exportMaterials)
                            {
                                //TODO: set gl_mat to the default material for all the elements
                                //gl_mat = Util.GetGLTFMaterial(Materials.List, material);
                            }

                            Materials.AddOrUpdateCurrent(material.UniqueId, gl_mat);

                            for (int i = 0; i < triangles - 1; i++)
                            {
                                try
                                {
                                    MeshTriangle triangle = mesh.get_Triangle(i);

                                    if (triangle == null)
                                        continue;

                                    #if REVIT2021 || REVIT2022 || REVIT2023

                                    int v1 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(0), _flipCoords, _forgeTypeId));
                                    int v2 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(1), _flipCoords, _forgeTypeId));
                                    int v3 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(2), _flipCoords, _forgeTypeId));
                                    
                                    #elif REVIT2019 || REVIT2020

                                    int v1 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(0), _flipCoords, _displayUnitType));
                                    int v2 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(1), _flipCoords, _displayUnitType));
                                    int v3 = _currentVertices.CurrentItem.AddVertex(new PointInt(triangle.get_Vertex(2), _flipCoords, _displayUnitType));
                                    
                                    #endif

                                    _currentGeometry.CurrentItem.faces.Add(v1);
                                    _currentGeometry.CurrentItem.faces.Add(v2);
                                    _currentGeometry.CurrentItem.faces.Add(v3);
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
    }
}