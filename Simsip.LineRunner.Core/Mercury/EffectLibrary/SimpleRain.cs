using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace Simsip.LineRunner.Mercury.EffectLibrary
{
    public class SimpleRain : ParticleEffect
    {
        public SimpleRain()
        {
            Name = "Simple Rain";

            #region Rain Drops

            this.Add
            (
                new LineEmitter
                {
                    Name = "Rain Drops",
                    Budget = 500,
                    Term = 3.5f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 150f,
                        Variation = 50f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(0.8745098f, 0.9254902f, 1f),
                        Variation = new Vector3(0f, 0f, 0f)
                    },
                    ReleaseOpacity = new VariableFloat
                    {
                        Value = 1f,
                        Variation = 0f
                    },
                    ReleaseScale = new VariableFloat
                    {
                        Value = 32f,
                        Variation = 16f
                    },
                    ReleaseRotation = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/BeamBlurred",
                    Modifiers = new ModifierCollection
                    {
                        new LinearGravityModifier
                        {
                            Gravity = new Vector2(0f, 250f)
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f,
                    Length = 800f,
                    Angle = -3.1415925f,
                    Rectilinear = true,
                    EmitBothWays = false
                }
            );

            #endregion

        }
    }
}