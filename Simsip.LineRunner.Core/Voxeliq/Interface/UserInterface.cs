/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using Engine.Assets;
using Engine.Common.Logging;
using Engine.Graphics;
using Engine.Input;
using Engine.Universe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;


namespace Engine.Interface
{
    public interface IUserInterface : IUpdateable, IDrawable
    {
        void Draw(Effect effect, EffectType type);
    }

    public class UserInterface : DrawableGameComponent, IUserInterface
    {
        private Texture2D _crosshairNormalTexture;
        private Texture2D _crosshairShovelTexture;
        private SpriteBatch _spriteBatch;
        
        private IInputManager _inputManager;
        private IAssetManager _assetManager;

        /// <summary>
        /// Logging facility.
        /// </summary>
        private static readonly Logger Logger = LogManager.CreateLogger();

        public UserInterface(Game game)
            : base(game)
        {
            // Export the service.
            this.Game.Services.AddService(typeof(IUserInterface), this); 
        }

        #region DrawableGameComponent overrides

        public override void Initialize()
        {
            Logger.Trace("init()");

            // Import required services.
            this._inputManager = (IInputManager) this.Game.Services.GetService(typeof (IInputManager));
            this._assetManager = (IAssetManager)this.Game.Services.GetService(typeof(IAssetManager));

            this._spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            // Does this cause it to be called twice?
            this.LoadContent();
        }

        protected override void LoadContent()
        {
            this._crosshairNormalTexture = this._assetManager.GetTexture(
                Asset.CrossHairNormalTexture,
                Engine.Core.Engine.Instance.TheCustomContentManager);
            this._crosshairShovelTexture = this._assetManager.GetTexture(
                Asset.CrossHairShovelTexture,
                Engine.Core.Engine.Instance.TheCustomContentManager);
        }

        public override void Draw(GameTime gameTime)
        {
            // Draw cross-hair.            
            var crosshairTexture = this._inputManager.ThePlayerControllerInput.AimedSolidBlock.HasValue
                                       ? _crosshairShovelTexture
                                       : _crosshairNormalTexture;

            // TODO: Will this effect our graphics state?
            this._spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            this._spriteBatch.Draw(crosshairTexture,
                              new Vector2((Game.GraphicsDevice.Viewport.Width/2) - 10,
                                          (Game.GraphicsDevice.Viewport.Height/2) - 10), Color.White);
            this._spriteBatch.End();
        }

        #endregion

        #region IUserInterface implementation
        
        public void Draw(Effect effect, EffectType type)
        {

        }

        #endregion
        
    }
}