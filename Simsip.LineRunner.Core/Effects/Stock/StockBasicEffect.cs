using Engine.Assets;
using Engine.Chunks;
using Engine.Debugging;
using Engine.Debugging.Graphs;
using Engine.Debugging.Ingame;
using Engine.Graphics;
using Engine.Interface;
using Engine.Sky;
using Engine.Universe;
using Engine.Water;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Characters;
using Simsip.LineRunner.GameObjects.Lines;
using Simsip.LineRunner.GameObjects.Obstacles;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Utils;
using System;
using System.IO;
using System.Collections.Generic;
using BEPUphysicsDrawer.Lines;
using Engine.Input;
using Simsip.LineRunner.Physics;
#if WINDOWS_PHONE || NETFX_CORE
using System.Threading.Tasks;
using Windows.Storage;
#endif


namespace Simsip.LineRunner.Effects.Stock
{
    /// <summary>
    /// Track which effect parameters need to be recomputed during the next OnApply.
    /// 
    /// IMPORTANT: We are treating this enum as a bitfield, hence see this link for important details
    ///            on setting values (power of 2), etc.
    ///            https://msdn.microsoft.com/query/dev12.query?appId=Dev12IDEF1&l=EN-US&k=k(System.FlagsAttribute);k(Flags);k(TargetFrameworkMoniker-MonoAndroid,Version%3Dv4.2);k(DevLang-csharp)&rd=true            
    /// </summary>
    [Flags]
    internal enum StockBasicEffectDirtyFlags
    {
        WorldViewProj = 1,
        World = 2,
        EyePosition = 4,
        MaterialColor = 8,
        Fog = 16,
        FogEnable = 32,
        AlphaTest = 64,
        ShaderIndex = 128,
        All = -1
    }

    public class StockBasicEffect : BasicEffect
    {
        private EffectParameter _isBottomClippedParam;
        private EffectParameter _bottomClippingPlaneParam;
        private EffectParameter _isTopClippedParam;
        private EffectParameter _topClippingPlaneParam;

#if WINDOWS_PHONE || NETFX_CORE
        public static async Task<StockBasicEffect> Initialize(GraphicsDevice device, string path)
        {
            var bytes = await StockBasicEffect.LoadEffectResource2Async(path);

            var effect = new StockBasicEffect(device, bytes);

            effect.CacheEffectParameters2();

            return effect;
        }

        private StockBasicEffect(GraphicsDevice device, byte[] bytes)
            : base(device, bytes)
        {
        }
#else
        public StockBasicEffect(GraphicsDevice device, string path)
            : base(device, LoadEffectResource2(path))
        {
            CacheEffectParameters2();
        }
#endif

        /// <summary>
        /// Gets or sets if a bottom clipping plane is to be used.
        /// 
        /// If true, then the clipping plane as defined by BottomClippingPlane will be used.
        /// </summary>
        public bool IsBottomClipped
        {
            get { return _isBottomClippedParam.GetValueBoolean(); }
            set { _isBottomClippedParam.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the bottom clipping plane to use.
        /// 
        /// Only used if IsBottomClipped is set to true;
        /// </summary>
        public Vector4 BottomClippingPlane
        {
            get { return _bottomClippingPlaneParam.GetValueVector4(); }
            set { _bottomClippingPlaneParam.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets if a top clipping plane is to be used.
        /// 
        /// If true, then the clipping plane as defined by TopClippingPlane will be used.
        /// </summary>
        public bool IsTopClipped
        {
            get { return _isTopClippedParam.GetValueBoolean(); }
            set { _isTopClippedParam.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the top clipping plane to use.
        /// 
        /// Only used if IsTopClipped is set to true;
        /// </summary>
        public Vector4 TopClippingPlane
        {
            get { return _topClippingPlaneParam.GetValueVector4(); }
            set { _topClippingPlaneParam.SetValue(value); }
        }

        private void CacheEffectParameters2()
        {
            this._isBottomClippedParam = Parameters["IsBottomClipped"];
            this._bottomClippingPlaneParam = Parameters["BottomClippingPlane"];
            this._isTopClippedParam = Parameters["IsTopClipped"];
            this._topClippingPlaneParam = Parameters["TopClippingPlane"];
        }

#if ANDROID || IOS || DESKTOP
        internal static byte[] LoadEffectResource2(string path)
        {
            byte[] bytecode = null;

#if ANDROID
            using (Stream stream = Program.SharedProgram.Assets.Open(path))
#elif DESKTOP || IOS
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
#endif
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    bytecode = ms.ToArray();
                }
            }

            return bytecode;
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        private static async Task<byte[]> LoadEffectResource2Async(string path)
        {
            byte[] bytecode = null;

#if WINDOWS_PHONE
            // http://chungkingmansions.com/blog/2013/08/adding-an-existing-sqlite-database-to-a-windows-phone-8-app/
            var assetsUri = new Uri(path, UriKind.Relative);
            using (var stream = System.Windows.Application.GetResourceStream(assetsUri).Stream)
#elif NETFX_CORE
            StorageFolder install = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await install.GetFileAsync(path);

            using (var stream = await file.OpenStreamForReadAsync())
#endif
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    bytecode = ms.ToArray();
                }
            }

            return bytecode;
        }
#endif

        // Couldn't get this one to work yet, copied from Monogame
        // Tried setting content to Embedded Resource
        /*
        internal static byte[] LoadEffectResource(string name)
        {
#if WINRT
            var assembly = typeof(Effect).GetTypeInfo().Assembly;
#else
            var assembly = typeof(Effect).Assembly;
#endif
            var stream = assembly.GetManifestResourceStream(name);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
        */
    }
}