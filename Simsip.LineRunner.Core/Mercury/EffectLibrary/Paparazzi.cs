using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace Simsip.LineRunner.Mercury.EffectLibrary
{
    public class Paparazzi : ParticleEffect
    {
        public Paparazzi()
        {
            Name = "Paparazzi";

            #region Lens Flares

            this.Add
            (
                new CircleEmitter
                {
                    Name = "Lens Flares",
                    Budget = 100,
                    Term = 0.1f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(1f, 1f, 1f),
                        Variation = new Vector3(0f, 0f, 0f)
                    },
                    ReleaseOpacity = new VariableFloat
                    {
                        Value = 1f,
                        Variation = 0f
                    },
                    ReleaseScale = new VariableFloat
                    {
                        Value = 256f,
                        Variation = 0f
                    },
                    ReleaseRotation = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/LensFlare",
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f,
                    Radius = 250f,
                    Ring = false,
                    Radiate = false
                }
            );
            
            #endregion

        }
    }
}