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
using Common_glTF_Exporter.EportUtils;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using Common_glTF_Exporter.UVs;

namespace Common_glTF_Exporter.Core
{
    /// <summary>
    /// GLTF Export Content class.
    /// </summary>
    public class GLTFExportContext : IExportContext
    {
        public static bool cancelation { get; set; } = false;

        public Autodesk.Revit.DB.Transform linkTransformation { get; private set; }

        public Autodesk.Revit.DB.Transform linkOriginalTranformation { get; private set; }

        public bool isLink { get; private set; }

        private Preferences preferences;

        private Document currentDocument;
        private View currentView;
        private GLTFNode currentNode;
        private Element currentElement;
        private Face currentFace;
        private GLTFMaterial currentMaterial;
        private Stack<Autodesk.Revit.DB.Transform> transformStack = new Stack<Autodesk.Revit.DB.Transform>();

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

        /// <summary>
        /// Reference to the rootNode to add children.
        /// </summary>
        private GLTFNode rootNode;

        public IndexedDictionary<GLTFNode> nodes = new IndexedDictionary<GLTFNode>();

        public IndexedDictionary<GLTFMaterial> materials = new IndexedDictionary<GLTFMaterial>();
        public List<GLTFTexture> textures { get; } = new List<GLTFTexture>();
        public List<GLTFImage> images { get; } = new List<GLTFImage>();
        public List<GLTFScene> scenes { get; } = new List<GLTFScene>();
        public IndexedDictionary<GLTFMesh> meshes { get; } = new IndexedDictionary<GLTFMesh>();
        public List<GLTFBuffer> buffers { get; } = new List<GLTFBuffer>();
        public List<GLTFBufferView> bufferViews { get; } = new List<GLTFBufferView>();
        public List<GLTFAccessor> accessors { get; } = new List<GLTFAccessor>();
        public List<GLTFBinaryData> binaryFileData { get; } = new List<GLTFBinaryData>();

        private Autodesk.Revit.DB.Transform CurrentTransform
        {
            get
            {
                return transformStack.Peek();
            }
        }

        public GLTFExportContext(Document doc)
        {
            currentDocument = doc;
            currentView = doc.ActiveView;
        }

        /// <summary>
        /// Runs once at beginning of export.
        /// </summary>
        /// <returns>TRUE if starded.</returns>
        public bool Start()
        {
            preferences = Common_glTF_Exporter.Windows.MainWindow.Settings.GetInfo();

            cancelation = false;
            transformStack.Push(Autodesk.Revit.DB.Transform.Identity);

            // Creation Root Node
            rootNode = new GLTFNode();
            rootNode.name = "rootNode";
            rootNode.rotation = ModelRotation.Get(preferences.flipAxis);
            rootNode.scale = ModelScale.Get(preferences);
            rootNode.translation = ModelTraslation.GetPointToRelocate(currentDocument, 
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
                RevitGrids.Export(currentDocument, ref nodes, ref rootNode, preferences);
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
            currentElement = currentDocument.GetElement(elementId);

            if (ElementValidations.ShouldSkipElement(currentElement, currentView, currentDocument, preferences, nodes))
            {
                currentElement = null;
                return RenderNodeAction.Skip;
            }

            linkTransformation = (currentElement as RevitLinkInstance)?.GetTransform();

            if (!isLink)
            {
                if (!currentElement.IsHidden(currentView) &&
                    currentView.IsElementVisibleInTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate, 
                    elementId))
                {
                    ProgressBarWindow.ViewModel.ProgressBarValue++;
                }
            }

            currentNode = GLTFNodeActions.CreateGLTFNodeFromElement(currentElement, preferences);
            currentGeometry.Reset();
            currentVertices.Reset();

            return RenderNodeAction.Proceed;
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
            if (ElementValidations.ShouldOmitElement(currentElement,
                currentVertices, currentView, currentDocument, elementId))
            {
                return;
            }

            nodes.AddOrUpdateCurrent(currentElement.UniqueId, currentNode);
            rootNode.children.Add(nodes.CurrentIndex);

            GLTFMesh newMesh = new GLTFMesh
            {
                name = currentElement.Name,
                primitives = new List<GLTFMeshPrimitive>()
            };
            meshes.AddOrUpdateCurrent(currentElement.UniqueId, newMesh);
            nodes.CurrentItem.mesh = meshes.CurrentIndex;

            // Convert _currentGeometry objects into glTFMeshPrimitives
            foreach (KeyValuePair<string, GeometryDataObject> kvp in currentGeometry.Dict)
            {
                string material_key = kvp.Key.Split(UNDERSCORE)[1];
                GLTFMaterial mat = materials.GetElement(material_key);

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
                if (node.MaterialId == ElementId.InvalidElementId)
                {
                    currentMaterial = GLTFExportUtils.GetGLTFMaterial(materials, node.Transparency, false);
                }
                else 
                {
                    currentMaterial = RevitMaterials.Export(node, ref materials, preferences, currentDocument);
                }

                materials.AddOrUpdateCurrentMaterial(currentMaterial.UniqueId, currentMaterial, false);
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
            GLTFExportUtils.AddOrUpdateCurrentItem(currentElement, currentGeometry, currentVertices, currentMaterial);

            var geomItem = currentGeometry.CurrentItem;
            var vertItem = currentVertices.CurrentItem;

            IList<XYZ> pts = polymesh.GetPoints();
            for (int i = 0; i < pts.Count; i++)
            {
                pts[i] = CurrentTransform.OfPoint(pts[i]);
            }

            foreach (PolymeshFacet facet in polymesh.GetFacets())
            {
                foreach (int index in facet.GetVertices())
                {
                    XYZ vertex = pts[index];
                    int vertexIndex = vertItem.AddVertexAndFlatten(
                                         new PointIntObject(vertex), geomItem.Vertices);
                    geomItem.Faces.Add(vertexIndex);

                    VertexUvs.AddUvToVertex(vertex, geomItem, currentMaterial, preferences, currentFace);
                }
            }

            if (preferences.normals)
            {
                GLTFExportUtils.AddNormals(CurrentTransform, polymesh, geomItem.Normals);
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
            isLink = true;

            currentDocument = node.GetDocument();

            transformStack.Push(CurrentTransform.Multiply(linkTransformation));
            linkOriginalTranformation = new Autodesk.Revit.DB.Transform(CurrentTransform);

            // We can either skip this instance or proceed with rendering it.
            return RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(LinkNode node)
        {
            isLink = false;
            // Note: This method is invoked even for instances that were skipped.
            transformStack.Pop();

            currentDocument = ExternalApplication.RevitCollectorService.GetDocument();
        }

        public RenderNodeAction Begin(FaceNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnRPC(RPCNode node)
        {
            var meshes = GeometryUtils.GetMeshes(currentDocument, currentElement);
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

                currentMaterial = MaterialUtils.GetGltfMeshMaterial(currentDocument, preferences, mesh, materials, true);

                materials.AddOrUpdateCurrentMaterial(currentMaterial.UniqueId, currentMaterial, true);

                GLTFExportUtils.AddOrUpdateCurrentItem(currentElement, currentGeometry, currentVertices, currentMaterial);

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