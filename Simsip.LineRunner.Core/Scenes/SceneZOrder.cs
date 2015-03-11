using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Simsip.LineRunner.Scenes
{
    public static class SceneZOrder
    {
        // Layer Z orders:
        // IMPORTANT: A child's overall Z order will be dependent on it's parent's Z order. That is,
        // all nodes attached to the ActionLayer are drawn before all nodes attached to the ui layer -
        // even if a child node in the ActionLayer has a larger Z order than a child node in the ui layer.
        public const int BackgroundLayer = 10;
        public const int ActionLayer = 20;
        public const int UILayer = 30;
    }
}