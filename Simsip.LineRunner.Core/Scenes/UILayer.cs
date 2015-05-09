using Cocos2D;


namespace Simsip.LineRunner.Scenes
{
    public class UILayer : GameLayer
    {
        public UILayer()
        {
            this.Color = CCColor3B.Black;
            this.Opacity = 128;

            // Not sure why we need to do this for Android
            for (int i = 0; i < m_pSquareVertices.Length; i++)
            {
                m_pSquareVertices[i].Position.Z = -1.0f;
            }
        }
    }
}