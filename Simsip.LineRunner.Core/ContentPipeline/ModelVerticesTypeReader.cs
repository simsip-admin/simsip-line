using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simsip.LineRunner.ContentPipeline
{         
    public class ModelVerticesTypeReader : ContentTypeReader<ModelVertices>
    {
        protected override ModelVertices Read(ContentReader input, ModelVertices existingInstance)
        {
            var vertices = input.ReadObject<Vector3[]>();
            var indices = input.ReadObject<int[]>();

            var newVertices = new PhysicsModelVertices(vertices, indices);

            return newVertices;
        }
    }
}
