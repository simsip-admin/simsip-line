using Cocos2D;


namespace Simsip.LineRunner.Scenes
{
    public class UILayer : GameLayer
    {
        public const int DEFAULT_OPACITY = 128;
        public CCColor3B DEFAULT_COLOR = CCColor3B.Black;

        public UILayer()
        {
            this.Color = DEFAULT_COLOR;
            this.Opacity = DEFAULT_OPACITY;

            // Not sure why we need to do this for Android
            for (int i = 0; i < m_pSquareVertices.Length; i++)
            {
                m_pSquareVertices[i].Position.Z = -1.0f;
            }
        }
    }
}