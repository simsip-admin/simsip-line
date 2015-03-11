/*
 * Copyright (C) 2011 - 2013 Voxeliq Engine - http://www.voxeliq.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Simsip.LineRunner.Settings.Readers
{
    /// <summary>
    /// Class has been re-worked to avoid Nini http://nini.sourceforge.net/
    /// </summary>
    public sealed class AudioSettings // : SettingsReader
    {
        public AudioSettings()
        {
            Enabled = true;

        }
        /// <summary>
        /// Is audio enabled?
        /// </summary>
        public bool Enabled { get; set; }
        /*
        public bool Enabled
        {
            get { return this.GetBoolean("Enabled", true); }
            set { this.Set("Enabled", value); }
        }
        */

        /// <summary>
        /// Creates a new ScreenConfig instance.
        /// </summary>
        /*
        internal AudioSettings()
            : base("Audio")
        { }
        */
    }
}