using Cocos2D;

namespace Simsip.LineRunner.Utils
{
    public static class Cocos2DUtils
    {
        public static void ResizeSprite(CCSprite sprite,  float width, float height)
        {
            sprite.ScaleX = width / sprite.ContentSize.Width;
            sprite.ScaleY = height / sprite.ContentSize.Height;
        }
    }
}