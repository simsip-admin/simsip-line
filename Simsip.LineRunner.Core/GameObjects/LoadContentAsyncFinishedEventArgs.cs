using System;


namespace Simsip.LineRunner.GameObjects
{
    public class LoadContentAsyncFinishedEventArgs : EventArgs
    {
        private LoadContentAsyncType _loadContentAsyncType;

        public LoadContentAsyncFinishedEventArgs(LoadContentAsyncType loadContentAsyncType)
        {
            this._loadContentAsyncType = loadContentAsyncType;
        }

        public LoadContentAsyncType TheLoadContentAsyncType
        {
            get { return this._loadContentAsyncType; }
        }
    }

}