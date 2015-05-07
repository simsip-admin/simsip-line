

namespace Simsip.LineRunner.GameObjects.Lines
{
    /// <summary>
    /// The function signature to use when you are subscribing to be notified of line hits.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void LineHitEventHandler(object sender, LineHitEventArgs e);
}