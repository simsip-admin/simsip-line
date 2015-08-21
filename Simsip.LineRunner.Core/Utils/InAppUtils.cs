using Engine.Assets;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Services.Inapp;
using System.Collections.Generic;
using System.Linq;


namespace Simsip.LineRunner.Utils
{
    public static class InAppUtils
    {
        private static IInappService _inAppService;
        private static IAssetManager _assetManager;
        private static IList<string> _tvFilenames;
        private static IDictionary<string, Texture2D> _tvSpritesheets;

        static InAppUtils()
        {
            InAppUtils._inAppService = (IInappService)TheGame.SharedGame.Services.GetService(typeof(IInappService));
            InAppUtils._assetManager = (IAssetManager)TheGame.SharedGame.Services.GetService(typeof(IAssetManager));

            InAppUtils._tvFilenames = new List<string>();
            InAppUtils._tvSpritesheets = new Dictionary<string, Texture2D>();
        }

        public static string GetPackProductIdSuffix(string productId)
        {
            return productId.Replace(InAppUtils._inAppService.LinerunnerPackPrefix, "");
        }

        public static string GetPackProductId(string productIdSuffix)
        {
            return InAppUtils._inAppService.LinerunnerPackPrefix + productIdSuffix;
        }

        // http://stackoverflow.com/questions/12914002/how-to-load-all-files-in-a-folder-with-xna
        public static Texture2D GetTvSpritesheet(int index, CustomContentManager customContentManager)
        {
            // Do we need to do our one-time initialization of filenames?
            if (InAppUtils._tvFilenames.Count == 0)
            {
                IList<string> filenames = FileUtils.GetFilenames(@"Images/Packs/Tv");
                foreach(string filename in filenames)
                {
                    InAppUtils._tvFilenames.Add(filename.Replace(".png", ""));
                }
            }

            // Ok to get the filename
            string selectedFilename = InAppUtils._tvFilenames[index];

            // See if we can grab a cached texture, otherwise, load and prep cache
            Texture2D selectedSpritesheet;
            if (!InAppUtils._tvSpritesheets.TryGetValue(selectedFilename, out selectedSpritesheet))
            {
                selectedSpritesheet = InAppUtils._assetManager.GetTexture(@"Images/Packs/Tv" + selectedFilename, customContentManager); 

                InAppUtils._tvSpritesheets[selectedFilename] = selectedSpritesheet;
            }

            return selectedSpritesheet;
        }
    }
}