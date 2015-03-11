/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using Cocos2D;
using System;
using Engine.Chunks;
using Engine.Common.Logging;
using Engine.Common.Vector;
using Engine.Debugging.Console;
using Engine.Debugging.Ingame;
using Engine.Graphics;
using Engine.Graphics.Effects.PostProcessing.Bloom;
using Engine.Universe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.SneakyJoystick;
using BEPUphysicsDemos;
using BEPUphysicsDemos.AlternateMovement;


namespace Engine.Input
{
    /// <summary>
    /// Interface that allows interracting with the input manager.
    /// </summary>
    public interface IInputManager : IUpdateable
    {
        Camera CurrentCamera { get; }
        Camera LineRunnerCamera { get; }
        Camera PlayerCamera { get; }
        StationaryCamera TheStationaryCamera { get; }

        IControllerInput CurrentControllerInput { get; }
        LineRunnerControllerInput TheLineRunnerControllerInput { get; }
        PlayerControllerInput ThePlayerControllerInput { get; }

        void SwitchState(GameState gameState);

        void ButtonStartEndEvent(object sender, EventCustom e);

        void StickStartEndEvent(object sender, EventCustom e);

        /// <summary>
        /// Should the game capture mouse?
        /// </summary>
        bool CaptureMouse { get; }

        /// <summary>
        /// Should the mouse cursor centered on screen?
        /// </summary>
        bool CursorCentered { get; }

        event InputManager.KeyEventHandler KeyDown;
    }
}