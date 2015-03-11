using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simsip.LineRunner.ContentPipeline;

namespace ModelImporters
{
    [ContentProcessor]
    class ModelVerticesProcessor : ModelProcessor
    {
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            ModelContent usualModel = base.Process(input, context);

            var vertices = new List<Vector3>();
            var indices = new List<int>();

            AddVerticesToLists(input, vertices, indices);

            usualModel.Tag = new ModelVertices(vertices.ToArray(), indices.ToArray());

            return usualModel;
        }

        private void AddVerticesToLists(NodeContent node, List<Vector3> vertexList, List<int> indexList)
        {
            int indexStart = indexList.Count;

            MeshContent mesh = node as MeshContent;
            if (mesh != null)
            {
                Matrix absTransform = mesh.AbsoluteTransform;

                foreach (GeometryContent geo in mesh.Geometry)
                {
                    foreach (int index in geo.Indices)
                    {
                        Vector3 vertex = geo.Vertices.Positions[index];
                        Vector3 transVertex = Vector3.Transform(vertex, absTransform);
                        vertexList.Add(transVertex);
                        indexList.Add(indexStart++);
                    }
                }
            }

            foreach (NodeContent child in node.Children)
            {
                AddVerticesToLists(child, vertexList, indexList);
            }
        }
    }

    [ContentTypeWriter]
    public class ModelVerticesTypeWriter : ContentTypeWriter<ModelVertices>
    {
        protected override void Write(ContentWriter output, ModelVertices value)
        {
            output.WriteObject<Vector3[]>(value.Vertices);
            output.WriteObject<int[]>(value.Indices);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Simsip.LineRunner.ContentPipeline.ModelVerticesTypeReader, SimsipLineRunner";
            // return typeof(VerticesTypeReader).AssemblyQualifiedName;
        }
    }

    /*
    public class VerticesTypeReader : ContentTypeReader<ModelVertices>
    {
        protected override ModelVertices Read(ContentReader input, ModelVertices existingInstance)
        {
            var vertices = input.ReadObject<Vector3[]>();
            var indices = input.ReadObject<int[]>();

            var newVertices = new ModelVertices(vertices, indices);

            return newVertices;
        }
    }
    */
}
