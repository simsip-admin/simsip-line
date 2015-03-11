using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace Simsip.LineRunner.Mercury.EffectLibrary
{
    public class FlowerBloom : ParticleEffect
    {
        public FlowerBloom()
        {
            Name = "Flower Bloom";

            #region Flower Petals

            this.Add
            (
                new Emitter
                {
                    Name = "Flower Petals",
                    Budget = 500,
                    Term = 2f,
                    ReleaseQuantity = 3,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 48f,
                        Variation = 16f
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
                        Value = 64f,
                        Variation = 0f
                    },
                    ReleaseRotation = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/Particle007",
                    Modifiers = new ModifierCollection
                    {
                        new TrajectoryRotationModifier
                        {
                            RotationOffset = 0f
                        }, 
                        new ScaleModifier
                        {
                            InitialScale = 16f,
                            UltimateScale = 128f
                        },
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 1f,
                            MiddleOpacity = 1f,
                            MiddlePosition = 0.8f,
                            FinalOpacity = 0f
                        },
                        new ColourInterpolatorModifier
                        {
                            InitialColour = new Vector3(0f, 0.5019608f, 0f),
                            MiddleColour = new Vector3(1f, 1f, 1f),
                            MiddlePosition = 0.5f,
                            FinalColour = new Vector3(1f, 0f, 1f)
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f
                }
            );

            #endregion

        }
    }
}