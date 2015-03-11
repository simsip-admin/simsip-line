using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Entities;
using Simsip.LineRunner.GameFramework;


namespace Engine.Assets
{
    public interface IAssetManager
    {
        // TODO: Create new and purge capabilities for resource dictionaries

        Effect GetEffect(string effectPath);
        SpriteFont GetFont(string fontPath);
        Model GetModel(string modelName, ModelType modelType, CustomContentManager customContentManager=null, bool allowCached=true);
        Texture2D GetModelTexture(string modelName, ModelType modelType, string textureName, CustomContentManager customContentManager=null, bool allowCached=true);
        string GetSound(string soundFilename);
        Texture2D GetTexture(string texturePath, CustomContentManager customContentManager=null, bool allowCached=true);
        Texture2D GetThumbnail(string modelName, ModelType modelType);

        void SwitchState(GameState gameState);
    }
}