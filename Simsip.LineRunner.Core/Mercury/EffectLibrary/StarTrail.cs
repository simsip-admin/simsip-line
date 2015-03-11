using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace Simsip.LineRunner.Mercury.EffectLibrary
{
    public class StarTrail : ParticleEffect
    {
        public StarTrail()
        {
            Name = "Star Trail";

            #region Blurs

            this.Add
            (
                new Emitter
                {
                    Name = "Blurs",
                    Budget = 20,
                    Term = 1f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(1f, 0.5019608f, 0.5019608f),
                        Variation = new Vector3(0.25f, 0.25f, 0.25f)
                    },
                    ReleaseOpacity = new VariableFloat
                    {
                        Value = 1f,
                        Variation = 0f
                    },
                    ReleaseScale = new VariableFloat
                    {
                        Value = 192f,
                        Variation = 0f
                    },
                    ReleaseRotation = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 3.14f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/LensFlare",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 1f,
                            MiddlePosition = 0.5f,
                            FinalOpacity = 0f
                        },
                        new RotationRateModifier
                        {
                            InitialRate = 1.57079637f,
                            FinalRate = 0f
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0.25f
                }
            );

            #endregion

            #region Stars

            this.Add
            (
                new Emitter
                {
                    Name = "Stars",
                    Budget = 5000,
                    Term = 1.5f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 10f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(1f, 0.5019608f, 0.5019608f),
                        Variation = new Vector3(0.25f, 0.25f, 0.25f)
                    },
                    ReleaseOpacity = new VariableFloat
                    {
                        Value = 1f,
                        Variation = 0f
                    },
                    ReleaseScale = new VariableFloat
                    {
                        Value = 48f,
                        Variation = 32f
                    },
                    ReleaseRotation = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 3.14f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/LensFlare",
                    Modifiers = new ModifierCollection
                    {
                        new DampingModifier
                        {
                            DampingCoefficient = -8f
                        },
                        new VelocityClampModifier
                        {
                            MaximumVelocity = 1.5f,
                        },
                        new ScaleInterpolatorModifier
                        {
                            InitialScale = 0f,
                            MiddleScale = 64f,
                            MiddlePosition = 0.5f,
                            FinalScale = 0f
                        }
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f
                }
            );

            #endregion

        }
    }
}