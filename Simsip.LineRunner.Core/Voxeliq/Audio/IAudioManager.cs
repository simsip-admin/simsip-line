
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Engine.Common.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Simsip.LineRunner.GameFramework;

namespace Engine.Audio
{
    public interface IAudioManager
    {
        void SwitchState(GameState gameState);
    }
}