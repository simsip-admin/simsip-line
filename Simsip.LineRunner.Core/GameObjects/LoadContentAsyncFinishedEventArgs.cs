using Simsip.LineRunner.GameFramework;
using System;


namespace Simsip.LineRunner.GameObjects
{
    public class LoadContentAsyncFinishedEventArgs : EventArgs
    {
        private LoadContentAsyncType _loadContentAsyncType;
        private GameState _gameState;

        public LoadContentAsyncFinishedEventArgs(LoadContentAsyncType loadContentAsyncType, GameState gameState)
        {
            this._loadContentAsyncType = loadContentAsyncType;
            this._gameState = gameState;
        }

        public LoadContentAsyncType TheLoadContentAsyncType
        {
            get { return this._loadContentAsyncType; }
        }

        public GameState TheGameState
        {
            get { return this._gameState; }
        }

    }

}