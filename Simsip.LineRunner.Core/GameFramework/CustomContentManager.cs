
using System;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Simsip.LineRunner.GameFramework
{
    /// <summary>
    /// CustomContentManager allows us to request non-cached versions
    /// of an asset by exposing the protected ReadAsset(). 
    /// This is important when we want to show an asset w/o
    /// tampering with a core game asset (e.g., Options - pads, lines, etc.)
    /// 
    /// Also, CustomContentManager's can control lifetime of a set of assets
    /// (e.g., inifinite streaming world).
    /// 
    /// References
    /// http://blogs.msdn.com/b/shawnhar/archive/2007/03/09/contentmanager-readasset.aspx
    /// http://xboxforums.create.msdn.com/forums/p/1472/7301.aspx#7301
    /// </summary>
    public class CustomContentManager : ContentManager
    {
        private string _tag;

        public CustomContentManager(IServiceProvider serviceProvider, string rootDirectory /*Debug: , string tag*/ )
            : base(serviceProvider, rootDirectory)
        {
            /* Debug
            this._tag = tag;
            Debug.WriteLine("Creating: " + tag);
            */

            OriginalEffectsDictionary = new Dictionary<string, IList<BasicEffect>>();
        }

        /* Debug
        public void Unload()
        {
            Debug.WriteLine("Unloading: " + this._tag);

            base.Unload();
        }
        public void Dispose()
        {
            Debug.WriteLine("Disposing: " + this._tag);

            base.Dispose();
        }
        */

        public  T ReadAsset<T>(string assetName)
        {
            return base.ReadAsset<T>(assetName, null);
        }

        /// <summary>
        /// Allows us to remove a set of original effects for when we are
        /// refreshing.
        /// </summary>
        public Dictionary<string, IList<BasicEffect>> OriginalEffectsDictionary { get; private set; }
    }
}