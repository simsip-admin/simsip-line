using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace Simsip.LineRunner.Mercury.EffectLibrary
{
    public class MagicTrail : ParticleEffect
    {
        public MagicTrail()
        {
            Name = "Magic Trail";

            #region Big Stars

            this.Add
            (
                new Emitter
                {
                    Name = "Big Stars",
                    Budget = 15,
                    Term = 0.75f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 96f,
                        Variation = 64f
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
                        Value = 48f,
                        Variation = 24f
                    },
                    ReleaseRotation = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/Star",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityModifier
                        {
                            Initial = 1f,
                            Ultimate = 0f
                        },
                        new ColourModifier
                        {
                            InitialColour = new Vector3(1f, 0.5019608f, 1f),
                            UltimateColour = new Vector3(1f, 0.5019608f, 0f)
                        },
                        new ScaleInterpolatorModifier
                        {
                            InitialScale = 48f,
                            MiddleScale = 64f,
                            MiddlePosition = 0.4f,
                            FinalScale = 16f
                        },
                        new DampingModifier
                        {
                            DampingCoefficient = 5f
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0.05f
                }
            );

            #endregion

            #region Small Stars

            this.Add
            (
                new Emitter
                {
                    Name = "Small Stars",
                    Budget = 400,
                    Term = 1f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 96f,
                        Variation = 64f
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
                        Value = 16f,
                        Variation = 0f
                    },
                    ReleaseRotation = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 3.14f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/Star",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 1f,
                            MiddlePosition = 0.75f,
                            FinalOpacity = 0f
                        },
                        new DampingModifier 
                        {
                            DampingCoefficient = 3f
                        },
                        new ColourModifier
                        {
                            InitialColour = new Vector3(1f, 1f, 1f),
                            UltimateColour = new Vector3(0.7529412f, 0.7529412f, 1f)
                        },
                        new LinearGravityModifier
                        {
                            Gravity = new Vector2(0f, 150f)
                        }
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f

                }
            );

            #endregion

            #region Sparkles

            this.Add
            (
                new Emitter
                {
                    Name = "Sparkles",
                    Budget = 700,
                    Term = 1f,
                    ReleaseQuantity = 5,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 48f
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
                        Value = 16f,
                        Variation = 0f
                    },
                    ReleaseRotation = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/Star",
                    Modifiers = new ModifierCollection
                    {
                        new ColourModifier
                        {
                            InitialColour = new Vector3(1f, 0.5019608f, 1f),
                            UltimateColour = new Vector3(0.7529412f, 0.2509804f, 0f)
                        },
                        new OpacityModifier
                        {
                            Initial = 1f,
                            Ultimate = 0f
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