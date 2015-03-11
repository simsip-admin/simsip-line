using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace Simsip.LineRunner.Mercury.EffectLibrary
{
    public class BasicExplosion : ParticleEffect
    {
        public BasicExplosion()
        {
            Name = "Basic Explosion";

            #region Smoke Trail

            this.Add
            (
                new CircleEmitter
                {
                    Name = "Smoke Trail",
                    Budget = 500,
                    Term = 2.5f,
                    ReleaseQuantity = 16,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 128f
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
                            UltimateScale = 255
                        },
                        new DampingModifier
                        {
                            DampingCoefficient = 1f
                        },
                        new RotationRateModifier
                        {
                            InitialRate = 1f,
                            FinalRate = 0f
                        },
                        new ColourModifier
                        {
                            InitialColour = new Vector3(0.305882365f, 0.254901975f, 0.211764708f),
                            UltimateColour = new Vector3(0.5019608f, 0.5019608f, 0.5019608f)
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0.25f,
                    Radius = 1f,
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
                    Term = 1f,
                    ReleaseQuantity = 64,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 100f
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
                        },
                        new DampingModifier
                        {
                            DampingCoefficient = 1f
                        }
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0.25f,
                    Radius = 1f,
                    Ring = true,
                    Radiate = true
                }
            );

            #endregion

            #region Sparks

            this.Add
            (
                new Emitter
                {
                    Name = "Sparks",
                    Budget = 250,
                    Term = 0.75f,
                    ReleaseQuantity = 35,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 250f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(1f, 0.8784314f, 0.7529412f),
                        Variation = new Vector3(0f, 0f, 0f)
                    },
                    ReleaseOpacity = new VariableFloat
                    {
                        Value = 1f,
                        Variation = 0f
                    },
                    ReleaseScale = new VariableFloat
                    {
                        Value = 5f,
                        Variation = 2f
                    },
                    ReleaseRotation = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/Particle005",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityModifier
                        {
                            Initial = 1f,
                            Ultimate = 0f
                        },
                        new DampingModifier
                        {
                            DampingCoefficient = 2f
                        }
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0.25f
                }
            );

            #endregion

            #region Flash

            this.Add
            (
                new Emitter
                {
                    Name = "Falsh",
                    Budget = 32,
                    Term = 0.1f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 50,
                        Variation = 0f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(1f, 1f, 1f),
                        Variation = new Vector3(0f, 0f, 0f)
                    },
                    ReleaseOpacity = new VariableFloat
                    {
                        Value = 0.5f,
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
                        Variation = 0f
                    },
                    ReleaseImpulse = new Vector2(0, 0),
                    ParticleTextureAssetName = "Images/Particles/Particle005",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityModifier
                        {
                            Initial = 1f,
                            Ultimate = 0f
                        }
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0.25f
                }
            );

            #endregion

        }
    }
}