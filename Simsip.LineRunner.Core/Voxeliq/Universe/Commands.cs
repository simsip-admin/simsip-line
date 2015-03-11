/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using Engine.Debugging.Console;
using Engine.Graphics;
using Engine.Input;

namespace Engine.Universe
{
    [Command("fly", "Sets the flying mode.\nusage: fly [on|off]")]
    public class FlyCommand : Command
    {
        private readonly IInputManager _inputManager;

        public FlyCommand()
        {
            this._inputManager = (IInputManager)Core.Engine.Instance.Game.Services.GetService(typeof(IInputManager));
        }

        [DefaultCommand]
        public string Default(string[] @params)
        {
            return string.Format("Fly mode is currently {0}.\nusage: fly [on|off].",
                                 this._inputManager.ThePlayerControllerInput.FlyingEnabled
                                     ? "on"
                                     : "off");
        }

        [Subcommand("on", "Sets flying mode on.")]
        public string On(string[] @params)
        {
            this._inputManager.ThePlayerControllerInput.FlyingEnabled = true;
            return "Fly mode on.";
        }

        [Subcommand("off", "Sets flying off.")]
        public string Off(string[] @params)
        {
            this._inputManager.ThePlayerControllerInput.FlyingEnabled = false;
            return "Fly mode off.";
        }
    }

    [Command("infinitive", "Sets the infinitive mode.\nusage: infinitive [on|off]")]
    public class InfinitiveCommand : Command
    {
        [DefaultCommand]
        public string Default(string[] @params)
        {
            return string.Format("Infinitive mode is currently {0}.\nusage: infinitive [on|off].",
                                 Core.Engine.Instance.Configuration.World.IsInfinitive
                                     ? "on"
                                     : "off");
        }

        [Subcommand("on", "Sets infinitive mode on.")]
        public string On(string[] @params)
        {
            Core.Engine.Instance.Configuration.World.IsInfinitive = true;
            return "Infinitive on.";
        }

        [Subcommand("off", "Sets infinitive off.")]
        public string Off(string[] @params)
        {
            Core.Engine.Instance.Configuration.World.IsInfinitive = false;
            return "Infinitive off.";
        }
    }
}
