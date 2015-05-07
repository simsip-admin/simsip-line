

using System;


namespace Simsip.LineRunner.GameObjects.Lines
{
    /// <summary>
    /// The value class that will be passed to subscribers of line hits.
    /// </summary>
    public class LineHitEventArgs : EventArgs
    {
        private LineModel _lineModel;

        public LineHitEventArgs(LineModel lineModel)
        {
            this._lineModel = lineModel;
        }

        /// <summary>
        /// The line model that was hit.
        /// </summary>
        public LineModel TheLineModel
        {
            get { return this._lineModel; }
        }
    }
}