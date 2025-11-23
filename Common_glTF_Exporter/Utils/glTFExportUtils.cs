namespace Common_glTF_Exporter.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Model;
    using Common_glTF_Exporter.Windows.MainWindow;
    using glTF.Manipulator.Schema;
    using Revit_glTF_Exporter;
    using Buffer = glTF.Manipulator.Schema.Buffer;
    public class GLTFExportUtils
        {
            const int DEF_COLOR = 250;
            const string DEF_MATERIAL_NAME = "default"; 
            const string DEF_UNIQUEL_ID = "8a3c94b3-d9e2-4e57-9189-f9bb6a9a54a4";

            public static BaseMaterial GetGLTFMaterial(IndexedDictionary<BaseMaterial> Materials, double opacity, bool doubleSided)
            {

                if (Materials.Dict.ContainsKey(DEF_UNIQUEL_ID))
                {
                    return Materials.GetElement(DEF_UNIQUEL_ID);
                }
                else
                {
                    return (CreateDefaultGLTFMaterial((int)opacity, doubleSided));
                }
            }

            public static BaseMaterial CreateDefaultGLTFMaterial(int materialOpacity, bool doubleSided)
            {
                BaseMaterial baseMaterial = new BaseMaterial();
                baseMaterial.doubleSided = doubleSided;
                float opacity = 1 - (float)materialOpacity;
                baseMaterial.name = DEF_MATERIAL_NAME;
                baseMaterial.baseColorFactor = new List<float>(4) { 1f, 1f, 1f, opacity };
                baseMaterial.metallicFactor = 0f;
                baseMaterial.roughnessFactor = 1f;
                baseMaterial.uuid = DEF_UNIQUEL_ID;

                return baseMaterial;
            }

            public static void AddVerticesAndFaces(
                VertexLookupIntObject vertexLookup,
                GeometryDataObject geometryDataObject,
                List<XYZ> pts)
            {
                foreach (var pt in pts)
                {
                    var point = new PointIntObject(pt);
                    var index = vertexLookup.AddVertexAndFlatten(point, geometryDataObject.Vertices);
                    geometryDataObject.Faces.Add(index);
                }
            }

            const string UNDERSCORE = "_";

            public static void AddOrUpdateCurrentItem(
                Element element,
                IndexedDictionary<GeometryDataObject> geomDataObj,
                IndexedDictionary<VertexLookupIntObject> vertexIntObj,
                BaseMaterial material)
            {
                string vertex_key = element.Id.ToString() + "_" + material.uuid;
                geomDataObj.AddOrUpdateCurrent(vertex_key, new GeometryDataObject());
                geomDataObj.CurrentItem.MaterialInfo = new MaterialInfo 
                { 
                    uuid = material.uuid
                };
                vertexIntObj.AddOrUpdateCurrent(vertex_key, new VertexLookupIntObject());
            }

            public static void AddRPCNormals(Preferences preferences, MeshTriangle triangle, GeometryDataObject geomDataObj)
            {
                XYZ normal = GeometryUtils.GetNormal(triangle);

                for (int j = 0; j < 3; j++)
                {
                    geomDataObj.Normals.Add(normal.X);
                    geomDataObj.Normals.Add(normal.Y);
                    geomDataObj.Normals.Add(normal.Z);
                }
            }


        public static void AddNormals(Transform transform, PolymeshTopology polymesh, List<double> normals)
            {
                IList<XYZ> polymeshNormals = polymesh.GetNormals();

                switch (polymesh.DistributionOfNormals)
                {
                    case DistributionOfNormals.AtEachPoint:
                    {
                        foreach (PolymeshFacet facet in polymesh.GetFacets())
                        {
                            List<XYZ> normalPoints = new List<XYZ>
                            {
                                transform.OfVector(polymeshNormals[facet.V1]).Normalize(),
                                transform.OfVector(polymeshNormals[facet.V2]).Normalize(),
                                transform.OfVector(polymeshNormals[facet.V3]).Normalize(),
                            };

                            foreach (var normalPoint in normalPoints)
                            {
                                normals.Add(normalPoint.X);
                                normals.Add(normalPoint.Y);
                                normals.Add(normalPoint.Z);
                            }
                        }

                        break;
                    }

                    case DistributionOfNormals.OnePerFace:
                    {
                        foreach (var facet in polymesh.GetFacets())
                        {
                            foreach (var normal in polymesh.GetNormals())
                            {
                                var newNormal = transform.OfVector(normal).Normalize();

                                for (int j = 0; j < 3; j++)
                                {
                                    normals.Add(newNormal.X);
                                    normals.Add(newNormal.Y);
                                    normals.Add(newNormal.Z);
                                }
                            }
                        }

                        break;
                    }

                    case DistributionOfNormals.OnEachFacet:
                    {
                        foreach (XYZ normal in polymeshNormals)
                        {
                            var newNormal = transform.OfVector(normal).Normalize();

                            for (int j = 0; j < 3; j++)
                            {
                                normals.Add(newNormal.X);
                                normals.Add(newNormal.Y);
                                normals.Add(newNormal.Z);
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
