using BEPUutilities.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using Microsoft.Xna.Framework;
using ConversionHelper;
using Simsip.LineRunner.Utils;

namespace BEPUphysicsDrawer.Lines
{
    /// <summary>
    /// Renders bounding boxes of entities.
    /// </summary>
    public class BoundingBoxDrawer
    {
        Game game;
        public BoundingBoxDrawer(Game game)
        {
            this.game = game;
        }

        RawList<VertexPositionColor> boundingBoxLines = new RawList<VertexPositionColor>();

        public void Draw(Effect effect, Space space)
        {
            XNAUtils.DefaultDrawState();

            if (space.Entities.Count > 0)
            {

                foreach (var e in space.Entities)
                {
                    Vector3[] boundingBoxCorners = MathConverter.Convert(e.CollisionInformation.BoundingBox.GetCorners());
                    var color = e.ActivityInformation.IsActive ? Color.DarkRed : new Color(150, 100, 100);
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[0], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[1], color));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[0], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[3], color));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[0], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[4], color));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[1], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[2], color));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[1], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[5], color));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[2], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[3], color));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[2], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[6], color));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[3], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[7], color));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[4], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[5], color));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[4], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[7], color));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[5], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[6], color));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[6], color));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[7], color));
                }
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    game.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, boundingBoxLines.Elements, 0, space.Entities.Count * 12, VertexPositionColor.VertexDeclaration);
                }
                boundingBoxLines.Clear();
            }
        }
    }
}
