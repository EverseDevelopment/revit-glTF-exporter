namespace Revit_glTF_Exporter
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Export;
    using Common_glTF_Exporter.Model;
    using Common_glTF_Exporter.Transform;
    using Common_glTF_Exporter.Utils;
    using Common_glTF_Exporter.Windows.MainWindow;
    using Transform = Autodesk.Revit.DB.Transform;

    /// <summary>
    /// GLTF Export Content class.
    /// </summary>
    public class GLTFExportContext : IExportContext
    {
        public static bool cancelation { get; set; } = false;

        /// <summary>
        /// Gets a stateful, uuid indexable list for all nodes in the export.
        /// </summary>
        public IndexedDictionary<GLTFNode> nodes = new IndexedDictionary<GLTFNode>();

        /// <summary>
        /// Gets a stateful, uuid indexable list for all materials in the export.
        /// </summary>
        public IndexedDictionary<GLTFMaterial> materials = new IndexedDictionary<GLTFMaterial>();

        private Document doc
        {
            get
            {
                if (documents.Count == 1)
                {
                    // normal document
                    return documents[0];
                }
                else
                {
                    // link document
                    return documents[1];
                }
            }
        }

        private bool skipElementFlag = false;
        private Element element;

        public Transform linkTransformation { get; private set; }

        private View view;
        private Preferences preferences;

        // The following properties are private to this class and used only for intermediate steps of the conversion.

        /// <summary>
        /// Reference to the rootNode to add children.
        /// </summary>
        private GLTFNode rootNode;

        /// <summary>
        /// Stateful, uuid indexable list of intermediate geometries for the element currently being
        /// processed, keyed by material. This is re-initialized on each new element.
        /// </summary>
        private IndexedDictionary<GeometryDataObject> currentGeometry;

        /// <summary>
        /// Stateful, uuid indexable list of intermediate vertex data for the element currently
        /// being processed, keyed by material. This is re-initialized on each new element.
        /// </summary>
        private IndexedDictionary<VertexLookupIntObject> currentVertices;

        private Stack<Transform> transformStack = new Stack<Transform>();

        /// <summary>
        /// container for the documents
        /// </summary>
        List<Document> documents = new List<Document>();

        public GLTFExportContext(Document doc)
        {
            preferences = Common_glTF_Exporter.Windows.MainWindow.Settings.GetInfo();
            documents.Add(doc);
            view = doc.ActiveView;
        }

        // The following properties are the root elements of the glTF format spec. They will be serialized into the final *.gltf file.

        /// <summary>
        /// Gets a list of root nodes defining scenes.
        /// </summary>
        public List<GLTFScene> scenes { get; } = new List<GLTFScene>();

        /// <summary>
        /// Gets a stateful, uuid indexable list for all meshes in the export.
        /// </summary>
        /// <value>
        /// A stateful, uuid indexable list for all meshes in the export.
        /// </value>
        public IndexedDictionary<GLTFMesh> meshes { get; } = new IndexedDictionary<GLTFMesh>();

        /// <summary>
        /// Gets a list of all buffers referencing the binary file data.
        /// </summary>
        /// <value>
        /// A list of all buffers referencing the binary file data.
        /// </value>
        public List<GLTFBuffer> buffers { get; } = new List<GLTFBuffer>();

        /// <summary>
        /// Gets a list of all BufferViews referencing the buffers.
        /// </summary>
        public List<GLTFBufferView> bufferViews { get; } = new List<GLTFBufferView>();

        /// <summary>
        /// Gets a list of all Accessors referencing the BufferViews.
        /// </summary>
        public List<GLTFAccessor> accessors { get; } = new List<GLTFAccessor>();

        /// <summary>
        /// Gets the container for the vertex/face/normal information that will be serialized into a binary
        /// format for the final *.bin files.
        /// </summary>
        public List<GLTFBinaryData> binaryFileData { get; } = new List<GLTFBinaryData>();

        private Transform CurrentTransform
        {
            get
            {
                return transformStack.Peek();
            }
        }

        /// <summary>
        /// Runs once at beginning of export.
        /// </summary>
        /// <returns>TRUE if starded.</returns>
        public bool Start()
        {
            cancelation = false;
            transformStack.Push(Transform.Identity);

            // Creation Root Node
            rootNode = new GLTFNode();
            rootNode.name = "rootNode";
            rootNode.rotation = ModelRotation.Get(preferences.flipAxis);
            rootNode.scale = ModelScale.Get(preferences);
            rootNode.translation = ModelTraslation.GetPointToRelocate(doc, rootNode.scale[0], preferences.flipAxis);
            rootNode.children = new List<int>();

            nodes.AddOrUpdateCurrent("rootNode", rootNode);

            // Creation default scene
            GLTFScene defaultScene = new GLTFScene();
            defaultScene.nodes.Add(0);
            scenes.Add(defaultScene);

            return true;
        }

        /// <summary>
        /// Runs once at end of export.
        /// </summary>
        public void Finish()
        {
            if (cancelation)
            {
                return;
            }

            if (preferences.grids)
            {
                RevitGrids.Export(doc, ref nodes, ref rootNode, preferences);
            }

            FileExport.Run(preferences, bufferViews, buffers, binaryFileData,
                scenes, nodes, meshes, materials, accessors);

            Compression.Run(preferences);
        }

        /// <summary>
        /// Runs once for each element, we create a new glTFNode and glTF Mesh keyed to the elements
        /// uuid, and reset the "_current" variables.
        /// </summary>
        /// <param name="elementId">ElementId of Element being processed.</param>
        /// <returns>RenderNodeAction.</returns>
        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            element = doc.GetElement(elementId);

            if (!Util.CanBeLockOrHidden(element, view) ||
                (element is Level && !preferences.levels))
            {
                return RenderNodeAction.Skip;
            }

            linkTransformation = (element as RevitLinkInstance)?.GetTransform();

            if (nodes.Contains(element.UniqueId))
            {
                // Duplicate element, skip adding.
                skipElementFlag = true;
                return RenderNodeAction.Skip;
            }

            if (linkTransformation==null & ProgressBarWindow.ViewModel.ProgressBarValue<100)
                ProgressBarWindow.ViewModel.ProgressBarValue++;

            // create a new node for the element
            GLTFNode newNode = new GLTFNode();

            // get the extras for this element
            GLTFExtras extras = new GLTFExtras();

            if (preferences.properties)
            {
                newNode.name = Util.ElementDescription(element);
                extras.uniqueId = element.UniqueId;
                extras.parameters = Util.GetElementParameters(element, true);
                extras.elementCategory = element.Category.Name;
                extras.elementId = element.Id.IntegerValue;
            }

            newNode.extras = extras;

            nodes.AddOrUpdateCurrent(element.UniqueId, newNode);

            // add the index of this node to our root node children array
            rootNode.children.Add(nodes.CurrentIndex);

            // Reset _currentGeometry for new element
            if (currentGeometry == null)
                currentGeometry = new IndexedDictionary<GeometryDataObject>();
            else
                currentGeometry.Reset();

            if (currentVertices == null)
                currentVertices = new IndexedDictionary<VertexLookupIntObject>();
            else
                currentVertices.Reset();

            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// Runs every time, and immediately prior to, a mesh being processed (OnPolymesh). It
        /// supplies the material for the mesh, and we use this to create a new material in our
        /// material container, or switch the current material if it already exists.
        /// </summary>
        /// <param name="node">Material node.</param>
        public void OnMaterial(MaterialNode node)
        {
            if (preferences.materials)
            {
                RevitMaterials.Export(node, doc, ref materials);
            }
        }

        /// <summary>
        /// Runs for every polymesh being processed. Typically this is a single face of an element's
        /// mesh. Here we populate the data into our "_current" variables (geometry and vertices)
        /// keyed on the element/material combination (this is important because within a single
        /// element, materials can be changed and repeated in unknown order).
        /// </summary>
        /// <param name="polymesh">PolymeshTopology.</param>
        public void OnPolymesh(PolymeshTopology polymesh)
        {
            GLTFExportUtils.AddOrUpdateCurrentItem(nodes, currentGeometry, currentVertices, materials);

            // populate current vertices vertex data and current geometry faces data
            Transform transform = CurrentTransform;
            IList<XYZ> pts = polymesh.GetPoints();
            pts = pts.Select(p => transform.OfPoint(p)).ToList();

            foreach (PolymeshFacet facet in polymesh.GetFacets())
            {
                foreach (int index in facet.GetVertices())
                {
                    int vertexIndex = currentVertices.CurrentItem.AddVertex(new PointIntObject(pts[index]));
                    currentGeometry.CurrentItem.Faces.Add(vertexIndex);
                }
            }

            if (preferences.normals)
            {
                GLTFExportUtils.AddNormals(preferences, transform, polymesh, currentGeometry.CurrentItem.Normals);
            }
        }

        const char UNDERSCORE = '_';

        /// <summary>
        /// Runs at the end of an element being processed, after all other calls for that element.
        /// Here we compile all the "_current" variables (geometry and vertices) onto glTF buffers.
        /// We do this at OnElementEnd because it signals no more meshes or materials are coming for
        /// this element.
        /// </summary>
        /// <param name="elementId">Element Id.</param>
        public void OnElementEnd(ElementId elementId)
        {
            if (currentVertices == null || !currentVertices.List.Any())
            {
                return;
            }

            if (skipElementFlag)
            {
                // Duplicate element, skip.
                skipElementFlag = false;
                return;
            }

            if (!Util.CanBeLockOrHidden(element, view))
            {
                return;
            }

            // create a new mesh for the node (we're assuming 1 mesh per node w/ multiple primitives
            // on mesh)
            GLTFMesh newMesh = new GLTFMesh();
            newMesh.name = element.Name;
            newMesh.primitives = new List<GLTFMeshPrimitive>();
            meshes.AddOrUpdateCurrent(element.UniqueId, newMesh);

            // add the index of this mesh to the current node.
            nodes.CurrentItem.mesh = meshes.CurrentIndex;

            // Add vertex data to _currentGeometry for each geometry/material pairing
            foreach (KeyValuePair<string, VertexLookupIntObject> kvp in currentVertices.Dict)
            {
                var vertices = currentGeometry.GetElement(kvp.Key).Vertices;
                foreach (KeyValuePair<PointIntObject, int> p in kvp.Value)
                {
                    vertices.Add(p.Key.X);
                    vertices.Add(p.Key.Y);
                    vertices.Add(p.Key.Z);
                }
            }

            // Convert _currentGeometry objects into glTFMeshPrimitives
            foreach (KeyValuePair<string, GeometryDataObject> kvp in currentGeometry.Dict)
            {
                GLTFBinaryData elementBinary = GLTFExportUtils.AddGeometryMeta(
                    buffers,
                    accessors,
                    bufferViews,
                    kvp.Value,
                    kvp.Key,
                    elementId.IntegerValue,
                    preferences.batchId,
                    preferences.normals);

                binaryFileData.Add(elementBinary);

                string material_key = kvp.Key.Split(UNDERSCORE)[1];
                GLTFMeshPrimitive primitive = new GLTFMeshPrimitive();

                primitive.attributes.POSITION = elementBinary.vertexAccessorIndex;

                if (preferences.normals)
                {
                    primitive.attributes.NORMAL = elementBinary.normalsAccessorIndex;
                }

                if (preferences.batchId)
                {
                    primitive.attributes._BATCHID = elementBinary.batchIdAccessorIndex;
                }

                primitive.indices = elementBinary.indexAccessorIndex;

                if (preferences.materials)
                {
                    primitive.material = materials.GetIndexFromUUID(material_key);
                }

                meshes.CurrentItem.primitives.Add(primitive);

                meshes.CurrentItem.name = element.Name;
            }
        }

        /// <summary>
        /// This is called when family instances are encountered, immediately after OnElementBegin.
        /// We're using it here to maintain the transform stack for that element's heirarchy.
        /// </summary>
        /// <param name="node">InstanceNode.</param>
        /// <returns>RenderNodeAction.</returns>
        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            var transform = node.GetTransform();
            var transformationMutiply = CurrentTransform.Multiply(transform);
            transformStack.Push(transformationMutiply);

            // We can either skip this instance or proceed with rendering it.
            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// This is called when family instances are encountered, immediately before OnElementEnd.
        /// We're using it here to maintain the transform stack for that element's heirarchy.
        /// </summary>
        /// <param name="node">InstanceNode.</param>
        public void OnInstanceEnd(InstanceNode node)
        {
            // Note: This method is invoked even for instances that were skipped.
            transformStack.Pop();
        }

        public bool IsCanceled()
        {
            // This method is invoked many times during the export process.
            return cancelation;
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
            documents.Add(node.GetDocument());

            transformStack.Push(CurrentTransform.Multiply(linkTransformation));

            // We can either skip this instance or proceed with rendering it.
            return RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(LinkNode node)
        {
            // Note: This method is invoked even for instances that were skipped.
            transformStack.Pop();

            documents.RemoveAt(1); // remove the item added in OnLinkBegin
        }

        public RenderNodeAction Begin(FaceNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnFaceEnd(FaceNode node)
        {
            // This method is invoked only if the custom exporter was set to include faces.
        }

        public void OnRPC(RPCNode node)
        {
            var meshes = GeometryUtils.GetMeshes(doc, element);
            if (!meshes.Any())
            {
                return;
            }

            foreach (var mesh in meshes)
            {
                int triangles = mesh.NumTriangles;
                if (triangles.Equals(0))
                {
                    continue;
                }

                MaterialUtils.SetMaterial(doc, preferences, mesh, materials, true);

                GLTFExportUtils.AddOrUpdateCurrentItem(nodes, currentGeometry, currentVertices, materials);

                for (int i = 0; i < triangles; i++)
                {
                    MeshTriangle triangle = mesh.get_Triangle(i);
                    if (triangle.Equals(null))
                    {
                        continue;
                    }

                    GLTFExportUtils.AddVerticesAndFaces(currentVertices.CurrentItem, currentGeometry.CurrentItem, triangle);

                    if (preferences.normals)
                    {
                        GLTFExportUtils.AddRPCNormals(preferences, triangle, currentGeometry.CurrentItem);
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
        }
    }
}