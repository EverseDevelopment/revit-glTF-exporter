namespace Common_glTF_Exporter.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Export;
    using Common_glTF_Exporter.Model;
    using Common_glTF_Exporter.Transform;
    using Common_glTF_Exporter.Utils;
    using Common_glTF_Exporter.Windows.MainWindow;
    using Revit_glTF_Exporter;
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

        /// <summary>
        /// Gets a list of all textures in the export.
        /// </summary>
        public List<GLTFTexture> textures { get; } = new List<GLTFTexture>();

        /// <summary>
        /// Gets a list of all images in the export.
        /// </summary>
        public List<GLTFImage> images { get; } = new List<GLTFImage>();



        public Transform linkTransformation { get; private set; }

        public Transform linkOriginalTranformation { get; private set; }

        public bool isLink { get; private set; }

        private View view;
        private Preferences preferences;

        /// <summary>
        /// Reference to the rootNode to add children.
        /// </summary>
        private GLTFNode rootNode;

        private Document Currentdocument;
        private GLTFNode currentNode;
        private Element currentElement;
        private Face currentFace;
        private GLTFMaterial currentMaterial;

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

        public GLTFExportContext(Document doc)
        {
            Currentdocument = doc;
            view = doc.ActiveView;
        }

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
            preferences = Common_glTF_Exporter.Windows.MainWindow.Settings.GetInfo();

            cancelation = false;
            transformStack.Push(Transform.Identity);

            // Creation Root Node
            rootNode = new GLTFNode();
            rootNode.name = "rootNode";
            rootNode.rotation = ModelRotation.Get(preferences.flipAxis);
            rootNode.scale = ModelScale.Get(preferences);
            rootNode.translation = ModelTraslation.GetPointToRelocate(Currentdocument, 
                rootNode.scale[0], preferences);
            rootNode.children = new List<int>();

            nodes.AddOrUpdateCurrent("rootNode", rootNode);

            // Creation default scene
            GLTFScene defaultScene = new GLTFScene();
            defaultScene.nodes.Add(0);
            scenes.Add(defaultScene);

            currentGeometry = new IndexedDictionary<GeometryDataObject>();
            currentVertices = new IndexedDictionary<VertexLookupIntObject>();

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
                RevitGrids.Export(Currentdocument, ref nodes, ref rootNode, preferences);
            }

            if (bufferViews.Count != 0)
            {
                FileExport.Run(preferences, bufferViews, buffers, binaryFileData,
                    scenes, nodes, meshes, materials, accessors, textures, images);
                Compression.Run(preferences, ProgressBarWindow.ViewModel);
            }
        }

        /// <summary>
        /// Runs once for each element, we create a new glTFNode and glTF Mesh keyed to the elements
        /// uuid, and reset the "_current" variables.
        /// </summary>
        /// <param name="elementId">ElementId of Element being processed.</param>
        /// <returns>RenderNodeAction.</returns>
        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            currentElement = Currentdocument.GetElement(elementId);

            if (currentElement == null)
            {
                currentElement = null;
                return RenderNodeAction.Skip;
            }

            if (!Util.CanBeLockOrHidden(currentElement, view, Currentdocument.IsFamilyDocument) ||
                (currentElement is Level && !preferences.levels))
            {
                currentElement = null;
                return RenderNodeAction.Skip;
            }

            if (nodes.Contains(currentElement.UniqueId))
            {
                currentElement = null;
                return RenderNodeAction.Skip;
            }

            linkTransformation = (currentElement as RevitLinkInstance)?.GetTransform();

            if (!isLink)
            {
                if (!currentElement.IsHidden(view) && 
                    view.IsElementVisibleInTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate, elementId))
                {
                    ProgressBarWindow.ViewModel.ProgressBarValue++;
                }
            }

            // create a new node for the element
            GLTFNode newNode = new GLTFNode();
            newNode.name = Util.ElementDescription(currentElement);

            if (preferences.properties)
            {
                // get the extras for this element
                GLTFExtras extras = new GLTFExtras();
                extras.uniqueId = currentElement.UniqueId;
                extras.parameters = Util.GetElementParameters(currentElement, true);
                if (currentElement.Category != null)
                {
                    extras.elementCategory = currentElement.Category.Name;
                }
                #if REVIT2024 || REVIT2025 || REVIT2026
                extras.elementId = currentElement.Id.Value;
                #else
                extras.elementId = currentElement.Id.IntegerValue;
                #endif

                newNode.extras = extras;
            }

            currentNode = newNode;
            currentGeometry.Reset();
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
            if (preferences.materials == MaterialsEnum.materials || preferences.materials ==  MaterialsEnum.textures)
            {
                currentMaterial = RevitMaterials.Export(node, ref materials, preferences, Currentdocument);
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
            GLTFExportUtils.AddOrUpdateCurrentItem(currentElement, currentGeometry, currentVertices, materials);

            IList<XYZ> pts = polymesh.GetPoints();
            pts = pts.Select(p => CurrentTransform.OfPoint(p)).ToList();

            foreach (PolymeshFacet facet in polymesh.GetFacets())
            {
                foreach (int index in facet.GetVertices())
                {
                    XYZ vertex = pts[index];
                    int vertexIndex = currentVertices.CurrentItem.AddVertex(new PointIntObject(vertex));
                    currentGeometry.CurrentItem.Faces.Add(vertexIndex);
     
                    if (preferences.materials == MaterialsEnum.textures && currentMaterial?.EmbeddedTexturePath != null)
                    {
                        // ✅ Compute UV from face
                        if (currentFace != null)
                        {
                            IntersectionResult projection = currentFace.Project(vertex);
                            if (projection != null)
                            {
                                UV uv = projection.UVPoint;
                                UV uvInMeters = new UV(uv.U * 12, uv.V * 12);
                                currentGeometry.CurrentItem.Uvs.Add(uvInMeters);
                            }
                            else
                            {
                                // Fallback: use dummy UVs if projection fails
                                currentGeometry.CurrentItem.Uvs.Add(new UV(0, 0));
                            }
                        }
                        else
                        {
                            // If no face is available (shouldn't happen), add dummy UV
                            currentGeometry.CurrentItem.Uvs.Add(new UV(0, 0));
                        }
                    }
                }
            }

            if (preferences.normals)
            {
                GLTFExportUtils.AddNormals(CurrentTransform, polymesh, currentGeometry.CurrentItem.Normals);
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
            if (currentElement == null)
            {        
                return;
            }

            if (currentVertices == null || !currentVertices.List.Any())
            {
                return;
            }

            if (!Util.CanBeLockOrHidden(currentElement, view, Currentdocument.IsFamilyDocument) || currentElement is RevitLinkInstance)
            {
                return;
            }

            nodes.AddOrUpdateCurrent(currentElement.UniqueId, currentNode);
            rootNode.children.Add(nodes.CurrentIndex);

            // create a new mesh for the node (we're assuming 1 mesh per node w/ multiple primitives
            // on mesh)
            GLTFMesh newMesh = new GLTFMesh();
            newMesh.name = currentElement.Name;
            newMesh.primitives = new List<GLTFMeshPrimitive>();
            meshes.AddOrUpdateCurrent(currentElement.UniqueId, newMesh);

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
                string material_key = kvp.Key.Split(UNDERSCORE)[1];
                GLTFMaterial mat;
                if (material_key == "")
                {
                   mat = currentMaterial;
                }
                else
                {
                    bool hasValidUVs = kvp.Value.Uvs.Count != 0;
                    GLTFMaterial currentMat = materials.GetElement(material_key);

                    int vertexCount = kvp.Value.Vertices.Count / 3;
                    if (!hasValidUVs)
                    {
                        
                        for (int i = 0; i < vertexCount; i++)
                        {
                            kvp.Value.Uvs.Add(new UV(0, 0));
                        }
                    }

                    if (vertexCount != kvp.Value.Uvs.Count)
                    {
                        int difference = vertexCount - kvp.Value.Uvs.Count;
                        for (int i = 0; i < difference; i++)
                        {
                            kvp.Value.Uvs.Add(new UV(0, 0));
                        }
                    }

                    mat = currentMat;

                }

                GLTFBinaryData elementBinary = GLTFExportUtils.AddGeometryMeta(
                    buffers,
                    accessors,
                    bufferViews,
                    kvp.Value,
                    kvp.Key,
                    #if REVIT2024 || REVIT2025 || REVIT2026
                    elementId.Value,
                    #else
                    elementId.IntegerValue,
                    #endif
                    preferences,
                    mat,
                    images,
                    textures);

                binaryFileData.Add(elementBinary);

                
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
       
                if (elementBinary.uvAccessorIndex != -1 && 
                    preferences.materials == MaterialsEnum.textures && 
                    mat.EmbeddedTexturePath != null)
                {
                    primitive.attributes.TEXCOORD_0 = elementBinary.uvAccessorIndex;
                }          

                primitive.indices = elementBinary.indexAccessorIndex;

                if (preferences.materials == MaterialsEnum.materials || preferences.materials == MaterialsEnum.textures)
                {
                    if (materials.Contains(material_key))
                    {
                        primitive.material = materials.GetIndexFromUUID(material_key);
                    }
                }

                meshes.CurrentItem.primitives.Add(primitive);
                meshes.CurrentItem.name = currentElement.Name;
            }

            currentElement = null;
            currentNode = null;
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
            isLink = true;

            Currentdocument = node.GetDocument();

            transformStack.Push(CurrentTransform.Multiply(linkTransformation));
            linkOriginalTranformation = new Transform(CurrentTransform);

            // We can either skip this instance or proceed with rendering it.
            return RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(LinkNode node)
        {
            isLink = false;
            // Note: This method is invoked even for instances that were skipped.
            transformStack.Pop();

            Currentdocument = ExternalApplication.RevitCollectorService.GetDocument();
        }

        public RenderNodeAction Begin(FaceNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnRPC(RPCNode node)
        {
            var meshes = GeometryUtils.GetMeshes(Currentdocument, currentElement);
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

                MaterialUtils.SetMaterial(Currentdocument, preferences, mesh, materials, true);

                GLTFExportUtils.AddOrUpdateCurrentItem(currentElement, currentGeometry, currentVertices, materials);

                for (int i = 0; i < triangles; i++)
                {
                    MeshTriangle triangle = mesh.get_Triangle(i);
                    if (triangle.Equals(null))
                    {
                        continue;
                    }

                    List<XYZ> pts = new List<XYZ> { 
                    triangle.get_Vertex(0),
                    triangle.get_Vertex(1),
                    triangle.get_Vertex(2)
                    };

                    List<XYZ> ptsTransformed = new List<XYZ>();
                    if(isLink)
                    {
                        ptsTransformed = pts.Select(p => linkOriginalTranformation.OfPoint(p)).ToList();
                    }
                    else
                    {
                        ptsTransformed = pts;
                    }

                    GLTFExportUtils.AddVerticesAndFaces(currentVertices.CurrentItem, currentGeometry.CurrentItem, ptsTransformed);

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
            currentFace = node.GetFace();
            return RenderNodeAction.Proceed;
        }
        public void OnFaceEnd(FaceNode node)
        {
            currentFace = null;
        }
    }
}