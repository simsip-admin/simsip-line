using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// A second variation on the common effect light rendering parameters.
    /// </summary>
	public interface IEffectLights2
	{
        /// <summary>
        /// The floating point ambient light color.
        /// </summary>
		Vector3 AmbientLightColor { get; set; }

        /// <summary>
        /// Returns the directional light.
        /// </summary>
		DirectionalLight DirectionalLight { get; }
	}
}

