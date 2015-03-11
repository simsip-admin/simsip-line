using Cocos2D;
using Microsoft.Xna.Framework.Graphics;


namespace Simsip.LineRunner.SneakyJoystick
{
    public class ColoredCircleSprite : CCSprite
    {
        private int _numberOfSegments;
        private CCPoint[] _circleVertices;

        public ColoredCircleSprite(CCColor4B color, float radius)
        {
            // Need these defined before setting radious
            this._numberOfSegments = 36;
            this._circleVertices = new CCPoint[this._numberOfSegments];
            
            this.Radius = radius;
            
            base.ContentSize = new CCSize(
                2f * this.Radius,
                2f * this.Radius);

            this.Color = new CCColor3B(color.R, color.G, color.B);
            this.Opacity = color.A;
        }

        #region Properties

        public float _radius;
        public float Radius
        {
            get
            {
                return _radius;
            }

            private set
            {
                this._radius = value;

                float theta_inc = 2.0f * 3.14159265359f / _numberOfSegments;

                float theta = 0.0f;

                for (int i = 0; i < this._numberOfSegments; i++)
                {
                    float j = _radius * CCMathHelper.Cos(theta) + Position.X;
                    float k = _radius * CCMathHelper.Sin(theta) + Position.Y;
                    this._circleVertices[i] = new CCPoint(j, k);

                    theta += theta_inc;
                }
            }
        }

        #endregion

        #region Cocos2D overrides

        public override void Draw()
        {
            base.Draw();

            TheGame.SharedGame.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            TheGame.SharedGame.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            TheGame.SharedGame.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            TheGame.SharedGame.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            CCDrawingPrimitives.Begin();
            CCDrawingPrimitives.DrawSolidPoly(
                this._circleVertices, 
                this._numberOfSegments, 
                new CCColor4B(Color.R, Color.G, Color.B));
            CCDrawingPrimitives.End();
        }

        #endregion

        #region Api

        public bool ContainsPoint(CCPoint point)
        {
            float dSq = point.X * point.X + point.Y * point.Y;
            float rSq = this._radius * this._radius;
            return (dSq <= rSq);
        }

        #endregion
    }
}
