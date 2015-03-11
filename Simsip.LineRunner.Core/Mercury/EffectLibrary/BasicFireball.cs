using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace Simsip.LineRunner.Mercury.EffectLibrary
{
    public class BasicFireball : ParticleEffect
    {
        public BasicFireball()
        {
            Name = "Basic Fireball";
            Description = "A simple fireball effect. Looks best in motion.";

            #region Smoke Trail

            this.Add
            (
                new CircleEmitter
                {
                    Name = "Smoke Trail",
                    Budget = 500,
                    Term = 2f,
                    ReleaseQuantity = 2,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 50f,
                        Variation = 0f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(0.5019608f, 0.5019608f, 0.5019608f),
                        Variation = new Vector3(0f, 0f, 0f)
                    },
                    ReleaseOpacity = new VariableFloat
                    {
                        Value = 1f,
                        Variation = 0f
                    },
                    ReleaseScale = new VariableFloat
                    {
                        Value = 16f,
                        Variation = 0f
                    },
                    ReleaseRotation = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/Cloud001",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityModifier
                        {
                            Initial = 0.2f,
                            Ultimate = 0f
                        },
                        new ScaleModifier
                        {
                            InitialScale = 48f,
                            UltimateScale = 255f
                        },
                        new DampingModifier
                        {
                            DampingCoefficient = 0.8f
                        },
                        new RotationRateModifier
                        {
                            InitialRate = 1f,
                            FinalRate = 0f
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f,
                    Radius = 50f,
                    Ring = true,
                    Radiate = true
                }
            );

            #endregion

            #region Flames

            this.Add
            (
                new CircleEmitter
                {
                    Name = "Flames",
                    Budget = 1000,
                    Term = 0.75f,
                    ReleaseQuantity = 8,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 50f,
                        Variation = 25f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(1f, 0.5019608f, 0f),
                        Variation = new Vector3(0.1f, 0f, 0f)
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
                        Value = 3.14f,
                        Variation = 3.14f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/Particle004",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityModifier
                        {
                            Initial = 0.5f,
                            Ultimate = 0f
                        },
                        new RotationModifier
                        {
                            RotationRate = 1f
                        }
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f,
                    Radius = 50f,
                    Ring = true,
                    Radiate = true
                }
            );

            #endregion

            #region Dying Flames

            this.Add
            (
                new CircleEmitter
                {
                    Name = "Dying Flames",
                    Budget = 1000,
                    Term = 1f,
                    ReleaseQuantity = 4,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 50f,
                        Variation = 25f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(1f, 0.5019608f, 0f),
                        Variation = new Vector3(0.3f, 0f, 0f)
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
                        Value = 3.14f,
                        Variation = 3.14f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/Cloud004",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 0.15f,
                            MiddlePosition = 0.7f,
                            FinalOpacity = 0f
                        },
                        new RotationRateModifier
                        {
                            InitialRate = 1f,
                            FinalRate = 0f
                        },
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f,
                    Radius = 50f,
                    Ring = true,
                    Radiate = true
                }
            );

            #endregion

        }
    }
}