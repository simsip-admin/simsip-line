/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using Cocos2D;
using Engine.Common.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner;
using Simsip.LineRunner.Entities;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.IO;
#if WINDOWS_PHONE || NETFX_CORE
using System.Threading.Tasks;
using Windows.Storage;
#endif
#if WINDOWS_PHONE
using Simsip.LineRunner.Concurrent;
#else
using System.Collections.Concurrent;
using System.Diagnostics;
#endif



namespace Engine.Assets
{
    public class AssetManager : GameComponent, IAssetManager
    {
        // Dictionaries of resources
        private Dictionary<string, Texture2D> _thumbnails;
        private ConcurrentDictionary<string, Effect> _effects;
        private Dictionary<string, string> _sounds;
        private Dictionary<string, SpriteFont> _fonts;

        private static readonly Logger Logger = LogManager.CreateLogger(); 

        public AssetManager(Game game)
            : base(game)
        {
            // Export service   
            this.Game.Services.AddService(typeof(IAssetManager), this); 
        }

        #region GameComponent overrides
        
#if ANDROID || IOS || DESKTOP
        public override void Initialize()
        {
#elif  WINDOWS_PHONE ||NETFX_CORE
        public async Task Initialize()
        {
#endif
            // Initialize resource dictionaries
            this._thumbnails = new Dictionary<string, Texture2D>();
            // this._effects = new Dictionary<string, Effect>();
            this._effects = new ConcurrentDictionary<string, Effect>();
            this._sounds = new Dictionary<string, string>();
            this._fonts = new Dictionary<string, SpriteFont>();

            // Pre-load content we will need
#if WINDOWS_PHONE || NETFX_CORE
            // await this.LoadInitialContentAsync();
#else
            // this.LoadInitialContent();
#endif

            base.Initialize();
        }
        
        #endregion

        #region IAssetManager implementation

        public Effect GetEffect(string effectPath)
        {
            // Do we have a cached version?
            if (_effects.ContainsKey(effectPath))
            {
                var effect = _effects[effectPath];
                if (effect == null)
                {
                    throw new Exception("Null effect: " + effectPath);
                }

                return effect;
            }

#if ANDROID || IOS || DESKTOP
            // Ok, not in cache, let's create, store in cache and return it
            // IMPORTANT: Note how effect path is the location of the effect including the
            // "Content" directory prefix
            var effectToLoad = this.LoadEffectShader(effectPath);
            if (!this._effects.TryAdd(effectPath, effectToLoad))
            {
                // TODO: Throw exception
                System.Diagnostics.Debug.WriteLine("Remove");
            }

            return effectToLoad;
#elif WINDOWS_PHONE || NETFX_CORE
            // We preload all effects so that we don't pollute the code
            // with GetEffectAsync calls
            throw new Exception("Effect not loaded: " + effectPath);
#endif
        }

        public SpriteFont GetFont(string fontPath)
        {
            try
            {
                // Do we have a cached version?
                if (_fonts.ContainsKey(fontPath))
                {
                    return _fonts[fontPath];
                }

                // Ok, not in cache, let's create, store in cacha and return it
                // IMPORTANT: Note how font path is the location of the font without
                // the "Content" director prefix
                var font = Game.Content.Load<SpriteFont>(@"Fonts/Verdana");
                this._fonts.Add(fontPath, font);

                return font;
            }
            catch(Exception ex)
            {
                throw new Exception("Unable to load font: " + fontPath + ", " + ex);
            }
        }

        public Model GetModel(string modelName, ModelType modelType, CustomContentManager customContentManager=null, bool allowCached=true)
        {
            // Determine where to load from
            var path = string.Empty;
            switch (modelType)
            {
                case ModelType.Character:
                    {
                        path = @"Models/Characters/";
                        break;
                    }
                case ModelType.Line:
                    {
                        path = @"Models/Lines/";
                        break;
                    }
                case ModelType.Obstacle:
                    {
                        path = @"Models/Obstacles/";
                        break;
                    }
                case ModelType.Pad:
                    {
                        path = @"Models/Pads/";
                        break;
                    }
                case ModelType.Pane:
                    {
                        path = @"Models/Panes/";
                        break;
                    }
                case ModelType.Voxeliq:
                    {
                        path = @"Models/Voxeliq/";
                        break;
                    }
            }

            Model model;
            var modelPath = path + modelName + "-model";
            if (customContentManager == null)
            {
                // Default behavior - load new or cached version
                model = Game.Content.Load<Model>(modelPath);
            }
            else
            {
                //
                // CustomContentManager allows us to request non-cached versions
                // of an asset by exposing the protected ReadAsset(). 
                // This is important when we want to show an asset w/o
                // tampering with a core game asset (e.g., Options - pads, lines, etc.)
                // 
                // Also, CustomContentManager's can control lifetime of a set of assets
                // (e.g., inifinite streaming world).
                //

                // Ok to pull new or cached version?
                if (allowCached)
                {
                    model = customContentManager.Load<Model>(modelPath);
                }
                else
                {
                    // Ok, we only want a fresh version
                    model = customContentManager.ReadAsset<Model>(modelPath);
                }
            }

            return model;
        }

        public Texture2D GetModelTexture(string modelName, ModelType modelType, string textureName, CustomContentManager customContentManager=null, bool allowCached=true)
        {
            // Determine where to load from
            var path = string.Empty;
            switch (modelType)
            {
                case ModelType.Character:
                    {
                        path = @"Models/Characters/";
                        break;
                    }
                case ModelType.Line:
                    {
                        path = @"Models/Lines/";
                        break;
                    }
                case ModelType.Obstacle:
                    {
                        path = @"Models/Obstacles/";
                        break;
                    }
                case ModelType.Pad:
                    {
                        path = @"Models/Pads/";
                        break;
                    }
                case ModelType.Pane:
                    {
                        path = @"Models/Panes/";
                        break;
                    }
                case ModelType.Voxeliq:
                    {
                        path = @"Models/Voxeliq/";
                        break;
                    }
            }

            Texture2D modelTexture;
            var modelTexturePath = path + textureName;
            if (customContentManager == null)
            {
                // Default behavior - load new or cached version
                modelTexture = Game.Content.Load<Texture2D>(modelTexturePath);
            }
            else
            {
                //
                // CustomContentManager allows us to request non-cached versions
                // of an asset by exposing the protected ReadAsset(). 
                // This is important when we want to show an asset w/o
                // tampering with a core game asset (e.g., Options - pads, lines, etc.)
                // 
                // Also, CustomContentManager's can control lifetime of a set of assets
                // (e.g., inifinite streaming world).
                //

                // Ok to pull new or cached version?
                if (allowCached)
                {
                    modelTexture = customContentManager.Load<Texture2D>(modelTexturePath);
                }
                else
                {
                    // Ok, we only want a fresh version
                    modelTexture = customContentManager.ReadAsset<Texture2D>(modelTexturePath);
                }
            }

            return modelTexture;
        }

        public string GetSound(string soundFilename)
        {
            // Do we have a cached version?
            if (_sounds.ContainsKey(soundFilename))
            {
                return _sounds[soundFilename];
            }

            // Ok, not in cache, let's create, store in cacha and return it
            // IMPORTANT: Note how soundname is the filename of the sound and currently
            // is just a pass through. We might enhance for different quality sounds, etc.
            this._sounds[soundFilename] = soundFilename;

            return soundFilename;
        }

        public Texture2D GetTexture(string texturePath, CustomContentManager customContentManager=null, bool allowCached=true)
        {
            // IMPORTANT: Note how texture name is the location of the texture without
            // the "Content" director prefix

            Texture2D texture;
            if (customContentManager == null)
            {
                // Default behavior - load new or cached version
                texture = Game.Content.Load<Texture2D>(texturePath);
            }
            else
            {
                //
                // CustomContentManager allows us to request non-cached versions
                // of an asset by exposing the protected ReadAsset(). 
                // This is important when we want to show an asset w/o
                // tampering with a core game asset (e.g., Options - pads, lines, etc.)
                // 
                // Also, CustomContentManager's can control lifetime of a set of assets
                // (e.g., inifinite streaming world).
                //

                // Ok to pull new or cached version?
                if (allowCached)
                {
                    texture = customContentManager.Load<Texture2D>(texturePath);
                }
                else
                {
                    // Ok, we only want a fresh version
                    texture = customContentManager.ReadAsset<Texture2D>(texturePath);
                }
            }

            return texture;
        }

        public Texture2D GetThumbnail(string modelName, ModelType modelType)
        {
            // Do we have a cached version?
            if (_thumbnails.ContainsKey(modelName))
            {
                return _thumbnails[modelName];
            }

            // Ok, not in cache, let's create, store in cacha and return it
            var path = string.Empty;
            switch (modelType)
            {
                case ModelType.Character:
                    {
                        path = @"Models/Characters/";
                        break;
                    }
                case ModelType.Line:
                    {
                        path = @"Models/Lines/";
                        break;
                    }
                case ModelType.Obstacle:
                    {
                        path = @"Models/Obstacles/";
                        break;
                    }
                case ModelType.Pad:
                    {
                        path = @"Models/Pads/";
                        break;
                    }
                case ModelType.Pane:
                    {
                        path = @"Models/Panes/";
                        break;
                    }
                case ModelType.Voxeliq:
                    {
                        path = @"Models/Voxeliq/";
                        break;
                    }
            }

            var thumbnail = Game.Content.Load<Texture2D>(path + modelName + "-thumbnail");
            this._thumbnails.Add(modelName, thumbnail);

            return thumbnail;
        }
        
        public void SwitchState(GameState gameState)
        {
            switch(gameState)
            {
                // Currently we only refresh the resource pack
                case GameState.Refresh:
                    {
                        // Load new texture pack
                        var currentResourcePack = UserDefaults.SharedUserDefault.GetStringForKey(
                            GameConstants.USER_DEFAULT_KEY_CURRENT_RESOURCE_PACK,
                            GameConstants.USER_DEFAULT_INITIAL_CURRENT_RESOURCE_PACK);
                        Asset.BlockTextureAtlas = @"ResourcePacks/" + currentResourcePack + @"/Terrain";
                        var forceLoad = this.GetTexture(Asset.BlockTextureAtlas);

                        break;
                    }
            }
        }

        #endregion

        #region Helper methods

#if WINDOWS_PHONE || NETFX_CORE
        private async Task<Effect> GetEffectAsync(string effectPath)
        {
            // Do we have a cached version?
            if (_effects.ContainsKey(effectPath))
            {
                return _effects[effectPath];
            }

            // Ok, not in cache, let's create, store in cacha and return it
            // IMPORTANT: Note how effect path is the location of the effect including the
            // "Content" directory prefix
            var effect = await this.LoadEffectShaderAsync(effectPath);
            if (!this._effects.TryAdd(effectPath, effect))
            {
                // TODO: Throw exception
                System.Diagnostics.Debug.WriteLine("Remove");
            }

            return effect;
        }
#endif

#if ANDROID || IOS || DESKTOP
        private void LoadInitialContent()
        {
            //
            // We hard-code loading these resources up front as we expect they
            // will be needed right away
            //

            // Models
            var pad1Model = this.GetModel("Pad1", ModelType.Pad);
            var ledge1Model = this.GetModel("Line1", ModelType.Line);
            var heroModel = this.GetModel("Hero", ModelType.Character);
            var paneModel = this.GetModel("Pane1", ModelType.Pane);

            // Effects
            var deferred1SceneEffect = this.GetEffect(Asset.Deferred1SceneEffect);
            var deferred2LightsEffect = this.GetEffect(Asset.Deferred2LightsEffect);
            var deferred3FinalEffect = this.GetEffect(Asset.Deferred3FinalEffect);
            var shadowMapEffect = this.GetEffect(Asset.ShadowMapEffect);
            var skyDomeEffect = this.GetEffect(Asset.SkyDomeEffect);
            var perlinNoiseEffect = this.GetEffect(Asset.PerlinNoiseEffect);

            // Textures
            var blockTextureAtlas = this.GetTexture(Asset.BlockTextureAtlas);
            var cloudTexture = this.GetTexture(Asset.CloudTexture);

            // Sounds
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundHeroDieing));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundHeroHitting));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundHeroPoint));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundHeroSwooshing));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundHeroWinging));

            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep1));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep2));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep3));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep4));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep5));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep6));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep7));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundScribble1));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundScribble2));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundScribble3));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundSound10));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundSound12));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundSound35));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundSound46));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundSound88));

            // Fonts
            // None for now
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        private async Task LoadInitialContentAsync()
        {
            //
            // We hard-code loading these resources up front as we expect they
            // will be needed right away
            //

            // Models
            var pad1Model = this.GetModel("Pad1", ModelType.Pad);
            var ledge1Model = this.GetModel("Line1", ModelType.Line);
            var heroModel = this.GetModel("Hero", ModelType.Character);
            var paneModel = this.GetModel("Pane1", ModelType.Pane);

            // Effects
            var deferred1SceneEffect = await this.GetEffectAsync(Asset.Deferred1SceneEffect);
            var deferred2LightsEffect = await this.GetEffectAsync(Asset.Deferred2LightsEffect);
            var deferred3FinalEffect = await this.GetEffectAsync(Asset.Deferred3FinalEffect);
            var shadowMapEffect = await this.GetEffectAsync(Asset.ShadowMapEffect);
            var skyDomeEffect = await this.GetEffectAsync(Asset.SkyDomeEffect);
            var perlinNoiseEffect = await this.GetEffectAsync(Asset.PerlinNoiseEffect);

            // Textures
            var blockTextureAtlas = this.GetTexture(Asset.BlockTextureAtlas);
            var cloudTexture = this.GetTexture(Asset.CloudTexture);

            // Sounds
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundHeroDieing));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundHeroHitting));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundHeroPoint));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundHeroSwooshing));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundHeroWinging));

            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep1));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep2));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep3));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep4));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep5));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep6));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundBeep7));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundScribble1));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundScribble2));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundScribble3));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundSound10));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundSound12));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundSound35));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundSound46));
            SoundUtils.PreloadSoundEffect(this.GetSound(Asset.SoundSound88));

            // Fonts
            // None for now
        }

#endif

#if ANDROID || IOS || DESKTOP
        private Effect LoadEffectShader(string path)
        {
            Effect effect = null;
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

            effect = new Effect(this.Game.GraphicsDevice, bytecode);

            return effect;
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        private async Task<Effect> LoadEffectShaderAsync(string path)
        {
            Effect effect = null;
            try
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

                effect = new Effect(this.Game.GraphicsDevice, bytecode);
            }
            catch (Exception ex)
            {
                // TODO: Throw exception
                System.Diagnostics.Debug.WriteLine("Remove: " + ex);
            }

            return effect;
        }
#endif

        #endregion

    }
}