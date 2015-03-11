using Cocos2D;


namespace Simsip.LineRunner.Scenes
{
    public class GameScene : CCScene
    {
        /// <summary>
        /// GameManager will call this after replacing a scene.
        /// 
        /// Derived scene classes should override this to delegate unloading content to their layers to unload content for their scene.
        /// </summary>
        public virtual void UnloadContent()
        {
        }
    }
}