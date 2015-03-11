using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simsip.LineRunner.ContentPipeline
{
    public class PhysicsModelVertices : ModelVertices
    {
        private BEPUutilities.Vector3[] _physicsVertices;

        public PhysicsModelVertices(Vector3[] vertices, int[] indices) 
            : base(vertices, indices)
        {
            this._physicsVertices = ConversionHelper.MathConverter.Convert(vertices);
        }

        public BEPUutilities.Vector3[] PhysicsVertices 
        { 
            get 
            { 
                return this._physicsVertices; 
            } 
        }
    }
}
