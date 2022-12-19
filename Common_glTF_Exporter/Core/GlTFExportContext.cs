using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Grid = Autodesk.Revit.DB.Grid;
using Common_glTF_Exporter.Export;
using Common_glTF_Exporter.Utils;
using Autodesk.Revit.UI;
using System.Numerics;
using Autodesk.Revit.DB.DirectContext3D;
using Common_glTF_Exporter.Windows.MainWindow;

namespace Revit_glTF_Exporter
{
    class glTFExportContext : IExportContext
    {
        private Document _doc;
        private bool _skipElementFlag = false;
        private Element _element;
        private ProgressBarWindow _progressBarWindow;
        private bool _exportNormals;
        private bool _exportElementId;
        private bool _exportBatchId;
        private bool _exportGrid;
        private bool _exportLevel;

        #if REVIT2019 || REVIT2020

        private DisplayUnitType _displayUnitType;

        #else

        private ForgeTypeId _forgeTypeId;

        #endif
        /// <summary>
        /// The name for the .gltf file.
        /// </summary>
        private string _filename;
        /// <summary>
        /// The directory for the .bin files.
        /// </summary>
        private string _directory;

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

        public glTFExportContext(Document doc, string filename, string directory,

            #if REVIT2019 || REVIT2020

            DisplayUnitType displayUnitType,

            #else 

            ForgeTypeId forgeTypeId,

            #endif

            ProgressBarWindow progressBarWindow)
            bool exportNormals, bool exportElementId, bool exportBatchId,
            bool singleBinary = true, bool exportProperties = true, bool flipCoords = true, bool exportMaterials = true)

        {

            _preferences = Common_glTF_Exporter.Windows.MainWindow.Settings.GetInfo();
            _doc = doc;
            _filename = filename;
            _directory = directory;
            _progressBarWindow = progressBarWindow;
            _exportMaterials = exportMaterials;
            _exportNormals = exportNormals;
            _exportElementId = exportElementId;
            _exportBatchId = exportBatchId;
            _exportGrid = exportGrid;
            _exportLevel= exportLevel;

            #if REVIT2019 || REVIT2020

            _displayUnitType = displayUnitType;

            #else

            _forgeTypeId = forgeTypeId;        

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

            if (_exportGrid)
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

            Binaries.Save(_preferences.singleBinary, BufferViews, _filename, Buffers, 
                _directory, binaryFileData);

            GltfFile.Create(Scenes, Nodes.List, Meshes.List, Materials.List, 
                Buffers, BufferViews, Accessors, _filename);

            Compression.Run(_filename, _preferences.compression);

        }

        /// <summary>
        /// Runs once for each element, we create a new glTFNode and glTF Mesh
        /// keyed to the elements uuid, and reset the "_current" variables.
        /// </summary>
        /// <param name="elementId">ElementId of Element being processed</param>
        /// <returns></returns>
        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            _progressBarWindow.ViewModel.ProgressBarValue++;

            _element = _doc.GetElement(elementId);

            if (_element.Category.Name == "Cameras" ||                 
                (_element is Level && !_exportLevel))
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

                if (_exportElementId)
                {
                    extras.elementId = _element.Id.IntegerValue;
                }

                extras.elementCategory = _element.Category.Name;

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
            if (!_preferences.materials)
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

            //Populate current geometry normals data
            if (polymesh.DistributionOfNormals == DistributionOfNormals.AtEachPoint)
            {
                IList<XYZ> norms = polymesh.GetNormals();
                foreach (XYZ norm in norms)
                {
                    _currentGeometry.CurrentItem.normals.Add(norm.X);
                    _currentGeometry.CurrentItem.normals.Add(norm.Y);
                    _currentGeometry.CurrentItem.normals.Add(norm.Z);
                }
            }

            // populate current vertices vertex data and current geometry faces data
            Transform t = CurrentTransform;
            IList<XYZ> pts = polymesh.GetPoints();
            pts = pts.Select(p => t.OfPoint(p)).ToList();
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


                if (polymesh.DistributionOfNormals == DistributionOfNormals.OnePerFace)
                {
                    _currentGeometry.CurrentItem.normals.Add(polymesh.GetNormal(0).X);
                    _currentGeometry.CurrentItem.normals.Add(polymesh.GetNormal(0).Y);
                    _currentGeometry.CurrentItem.normals.Add(polymesh.GetNormal(0).Z);
                    _currentGeometry.CurrentItem.normals.Add(polymesh.GetNormal(0).X);
                    _currentGeometry.CurrentItem.normals.Add(polymesh.GetNormal(0).Y);
                    _currentGeometry.CurrentItem.normals.Add(polymesh.GetNormal(0).Z);
                    _currentGeometry.CurrentItem.normals.Add(polymesh.GetNormal(0).X);
                    _currentGeometry.CurrentItem.normals.Add(polymesh.GetNormal(0).Y);
                    _currentGeometry.CurrentItem.normals.Add(polymesh.GetNormal(0).Z);
                }


                //if (_exportNormals && polymesh.DistributionOfNormals == DistributionOfNormals.OnePerFace)
                //{
                //    var point1 = pts[facet.V1];
                //    var point2 = pts[facet.V2];
                //    var point3 = pts[facet.V3];

                //    XYZ normal1 = (point2 - point1).CrossProduct(point3 - point1);
                //    normal1 = normal1.Normalize();

                //    _currentGeometry.CurrentItem.normals.Add(normal1.X);
                //    _currentGeometry.CurrentItem.normals.Add(normal1.Y);
                //    _currentGeometry.CurrentItem.normals.Add(normal1.Z);
                //}

#else

                int v1 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V1], _flipCoords, _forgeTypeId));
                int v2 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V2], _flipCoords, _forgeTypeId));
                int v3 = _currentVertices.CurrentItem.AddVertex(new PointInt(pts[facet.V3], _flipCoords, _forgeTypeId));    
                
#endif
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

            // create a new mesh for the node (we're assuming 1 mesh per node w/ multiple primatives on mesh)
            glTFMesh newMesh = new glTFMesh();
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
                glTFBinaryData elementBinary = AddGeometryMeta(kvp.Value, kvp.Key, elementId.IntegerValue);

                binaryFileData.Add(elementBinary);

                string material_key = kvp.Key.Split('_')[1];

                glTFMeshPrimitive primitive = new glTFMeshPrimitive();
                primitive.attributes.POSITION = elementBinary.vertexAccessorIndex;

                if (_exportNormals)
                {
                    primitive.attributes.NORMAL = elementBinary.normalsAccessorIndex;
                }

                if (_exportBatchId)
                {
                    primitive.attributes._BATCHID = elementBinary.batchIdAccessorIndex;
                }   

                primitive.indices = elementBinary.indexAccessorIndex;

                if (_preferences.materials)
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

            foreach (var index in geomData.faces)
            {
                bufferData.indexBuffer.Add(index);
            }

            if (_exportBatchId)
            {
                foreach (var coord in geomData.vertices)
                {
                    bufferData.batchIdBuffer.Add(elementId);
                }
            }

            if (_exportNormals)
            {
                foreach (var normal in geomData.normals)
                {
                    bufferData.normalBuffer.Add((float)normal);
                }
            }

            // Get max and min for vertex data
            float[] vertexMinMax = Util.GetVec3MinMax(bufferData.vertexBuffer);

            // Get max and min for index data
            int[] faceMinMax = Util.GetScalarMinMax(bufferData.indexBuffer);

            // Get max and min for batchId data
            float[] batchIdMinMax = default;
            if (_exportBatchId)
            {
                batchIdMinMax = Util.GetVec3MinMax(bufferData.batchIdBuffer);
            }

            //Get max and min for normal data

            float[] normalMinMax = default;
            if (_exportNormals)
            {
                normalMinMax = Util.GetVec3MinMax(bufferData.normalBuffer);
            }

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

            ////Add a normals(vec3) buffer view
            glTFBufferView vec3ViewNormals = new glTFBufferView();
            int elementsPerNormal = default;
            int vec3ViewNormalsIdx = default;
            if (_exportNormals)
            {
                elementsPerNormal = 3;
                int bytesPerNormalElement = 4;
                int bytesPerNormal = elementsPerNormal * bytesPerNormalElement;
                int numVec3Normals = (geomData.normals.Count) / elementsPerNormal;
                int sizeOfVec3ViewNormals = numVec3Normals * bytesPerNormal;
                vec3ViewNormals = new glTFBufferView();
                vec3ViewNormals.buffer = bufferIdx;
                vec3ViewNormals.byteOffset = vec3View.byteLength;
                vec3ViewNormals.byteLength = sizeOfVec3ViewNormals;
                vec3ViewNormals.target = Targets.ELEMENT_ARRAY_BUFFER;
                BufferViews.Add(vec3ViewNormals);
                vec3ViewNormalsIdx = BufferViews.Count - 1;
            }

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

            // Add a batchId buffer view
            int batchIdsViewIdx = default;
            if (_exportBatchId)
            {
                glTFBufferView batchIdsView = new glTFBufferView();
                batchIdsView.buffer = bufferIdx;
                batchIdsView.byteOffset = facesView.byteOffset + facesView.byteLength;
                batchIdsView.byteLength = sizeOfVec3View;
                batchIdsView.target = Targets.ARRAY_BUFFER;
                BufferViews.Add(batchIdsView);
                batchIdsViewIdx = BufferViews.Count - 1;
            }

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

            if (_exportBatchId)
            {
                // add a batchId accessor
                glTFAccessor batchIdAccessor = new glTFAccessor();
                batchIdAccessor.bufferView = batchIdsViewIdx;
                batchIdAccessor.byteOffset = 0;
                batchIdAccessor.componentType = ComponentType.FLOAT;
                batchIdAccessor.count = geomData.vertices.Count / elementsPerVertex;
                batchIdAccessor.type = "VEC3";
                batchIdAccessor.max = new List<float>() { batchIdMinMax[1], batchIdMinMax[3], batchIdMinMax[5] };
                batchIdAccessor.min = new List<float>() { batchIdMinMax[0], batchIdMinMax[2], batchIdMinMax[4] };
                batchIdAccessor.name = "BATCH_ID";
                Accessors.Add(batchIdAccessor);
                bufferData.batchIdAccessorIndex = Accessors.Count - 1;
            }

            if (_exportNormals)
            {
                ////add a normals accessor
                //glTFAccessor normalsAccessor = new glTFAccessor();
                //normalsAccessor.bufferView = vec3ViewNormalsIdx;
                //normalsAccessor.byteOffset = 0;
                //normalsAccessor.componentType = ComponentType.FLOAT;
                //normalsAccessor.count = geomData.normals.Count / elementsPerNormal;
                //normalsAccessor.type = "VEC3";
                //normalsAccessor.max = new List<float>() { normalMinMax[1], normalMinMax[3], normalMinMax[5] };
                //normalsAccessor.min = new List<float>() { normalMinMax[0], normalMinMax[2], normalMinMax[4] };
                //normalsAccessor.name = "NORMALS";
                //Accessors.Add(normalsAccessor);
                //bufferData.normalsAccessorIndex = Accessors.Count - 1;
            }

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

        public RenderNodeAction 
            
            Begin(FaceNode node)
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

                            for (int i = 0; i < triangles - 1; i++)
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

                                    XYZ side1 = triangle.get_Vertex(1) - (triangle.get_Vertex(0));
                                    XYZ side2 = triangle.get_Vertex(2) - triangle.get_Vertex(0);
                                    XYZ normal = side1.CrossProduct(side2);
                                    normal = normal.Normalize();

                                    _currentGeometry.CurrentItem.normals.Add(normal.X);
                                    _currentGeometry.CurrentItem.normals.Add(normal.Y);
                                    _currentGeometry.CurrentItem.normals.Add(normal.Z);
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