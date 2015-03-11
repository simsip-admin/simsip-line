using Cocos2D;
using Engine.Assets;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.Data.LineRunner;
using Simsip.LineRunner.Entities.LineRunner;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using Simsip.LineRunner.Physics;
using Simsip.LineRunner.Utils;
using System;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using BEPUphysicsDemos.AlternateMovement;
using Simsip.LineRunner.Effects.Deferred;
using ConversionHelper;
using Engine.Graphics;


namespace Simsip.LineRunner.GameObjects.Characters
{
    public class PlayerModel : CharacterModel
    {
        public PlayerModel()
        {
            this.Initialize();
        }

        #region Overrides

        public override void Initialize()
        {
            // Will pull in default services
            base.Initialize();

            this.GameObjType = GameObjectType.Player;
        }

        #endregion

    }
}

