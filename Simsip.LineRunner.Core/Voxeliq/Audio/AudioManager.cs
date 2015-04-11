/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Engine.Common.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Utils;
using Engine.Assets;
using Simsip.LineRunner;

namespace Engine.Audio
{
    enum AmbientMusic
    {
        // TODO: Removing music
        // forestAmbient,
        // frogsAmbient
    }

    public class AudioManager : GameComponent, IAudioManager
    {
        private Song _backgroundSong;
        private IEnumerable<AmbientMusic> _ambientMusicsNames;
        private SoundEffectInstance _currentAmbientMusic;
        private readonly Dictionary<AmbientMusic, SoundEffect> _ambientMusicSoundEffects = new Dictionary<AmbientMusic, SoundEffect>();

        private readonly Random _random = new Random();

        private GameState _currentGameState;
        private IAssetManager _assetManager;
        private int _soundScribbleId;

        private static readonly Logger Logger = LogManager.CreateLogger(); // the logger.

        public AudioManager(Game game)
            : base(game)
        {
            // Export service
            game.Services.AddService(typeof(IAudioManager), this);
        }

        public override void Initialize()
        {
            try
            {
                this._assetManager = (IAssetManager)TheGame.SharedGame.Services.GetService(typeof(IAssetManager));

                // Hopefully this represents an invalid sound effect id to start with
                this._soundScribbleId = -1;

                // TODO: Trimning out music
                // this._backgroundSong = Game.Content.Load<Song>(@"audio\music\funandrun");

                // load ambient musics.
                this._ambientMusicsNames = Enum.GetValues(typeof (AmbientMusic)).Cast<AmbientMusic>();

                foreach (var entry in this._ambientMusicsNames.ToArray())
                {
                    this._ambientMusicSoundEffects.Add(entry,Game.Content.Load<SoundEffect>(@"audio\music\ambient\" + entry.ToString()));
                }

                // TODO: Add back in when ready
                // this.PlayBackroundSong();
#if !NETFX_CORE
                // TODO: Add back in when ready
                /*
                var ambientMusicThread = new Thread(AmbientMusicLoop);
                ambientMusicThread.Name = "Ambient Music Thread";
                ambientMusicThread.Start();
                */
#endif
            }
            catch (Exception e)
            {
                Logger.FatalException(e, "AudioManager is offline due to unexpected exception.");
            }
        }

        public void SwitchState(GameState gameState)
        {
            this._currentGameState = gameState;

            if (this._currentGameState == GameState.Moving)
            {
                // Only have one looping scribble sound effect going at a time
                SoundUtils.StopSoundEffect(this._soundScribbleId);

                // Start looping the scribble sound effect
                this._soundScribbleId = SoundUtils.PlaySoundEffect(_assetManager.GetSound(Asset.SoundScribble3), loop: true);
            }
            else
            {
                // The scribbling sound effect is only valid fo the MOVING state
                SoundUtils.StopSoundEffect(this._soundScribbleId);
            }
        }

        private void AmbientMusicLoop()
        {
#if NETFX_CORE
            return;
#else
            if (!Core.Engine.Instance.Configuration.Audio.Enabled)
                return;

            while (true)
            {
                if (this._currentAmbientMusic == null || this._currentAmbientMusic.IsDisposed || this._currentAmbientMusic.State == SoundState.Playing)
                {
                    Thread.Sleep(1000);
                }

                this.PlayRandomAmbientMusic();
                Thread.Sleep(2000);
            }
#endif
        }

        private void PlayBackroundSong()
        {
            if (!Core.Engine.Instance.Configuration.Audio.Enabled)
                return;

            #if !MONOGAME
            Microsoft.Xna.Framework.Media.MediaPlayer.IsRepeating = true;
            Microsoft.Xna.Framework.Media.MediaPlayer.Play(this._backgroundSong);
            Microsoft.Xna.Framework.Media.MediaPlayer.Volume = 0.3f;
            #endif
        }

        private void PlayRandomAmbientMusic()
        {
            var randomAmbientMusic = this._ambientMusicsNames.ToArray()[_random.Next(this._ambientMusicsNames.ToArray().Length)];
            this._currentAmbientMusic = this._ambientMusicSoundEffects[randomAmbientMusic].CreateInstance();

            fadeOut();
            this._currentAmbientMusic.Play();
            fadeIn();
        }

        private void fadeOut()
        {
#if !NETFX_CORE
            if (this._currentAmbientMusic.State == SoundState.Stopped || this._currentAmbientMusic.State == SoundState.Paused) return;

            for (float f = this._currentAmbientMusic.Volume; f > 0f; f -= 0.05f)
            {
                Thread.Sleep(10);
                this._currentAmbientMusic.Volume -= f;
            }
#endif
        }

        private void fadeIn()
        {
#if !NETFX_CORE
            for (float f = this._currentAmbientMusic.Volume; f < 1f; f += 0.05f)
            {
                Thread.Sleep(10);
                this._currentAmbientMusic.Volume += f;
            }
#endif
        }
    }
}