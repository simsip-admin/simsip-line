using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace Simsip.LineRunner.Mercury.EffectLibrary
{
    public class BeamMeUp : ParticleEffect
    {
        public BeamMeUp()
        {
            Name = "BeamMeUp";

            #region Particles

            this.Add
            (
                new RectEmitter
                {
                    Name = "Particles",
                    Budget = 500,
                    Term = 1f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 25f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(0f, 0.7529412f, 0.7529412f),
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
                    ParticleTextureAssetName = "Images/Particles/LensFlare",
                    Modifiers = new ModifierCollection
                    {
                        new LinearGravityModifier
                        {
                            Gravity = new Vector2(0f, -150f)
                        },
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 1f,
                            MiddlePosition = 0.5f,
                            FinalOpacity = 0f
                        },
                        new ScaleInterpolatorModifier
                        {
                            InitialScale = 16f,
                            MiddleScale = 32f,
                            MiddlePosition = 0.5f,
                            FinalScale = 16f
                        }
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f,
                    Width = 100f,
                    Height = 250f,
                    Rotation = 0f,
                    Frame = false
                }
            );

            #endregion

            #region Fast Beams

            this.Add
            (
                new RectEmitter
                {
                    Name = "Fast Beams",
                    Budget = 500,
                    Term = 0.5f,
                    ReleaseQuantity = 3,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(0f, 1f, 1f),
                        Variation = new Vector3(0.1f, 0f, 0.1f)
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
                    ReleaseImpulse = new Vector2(0f, -50f),
                    ParticleTextureAssetName = "Images/Particles/Beam",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 0.1f,
                            MiddlePosition = 0.5f,
                            FinalOpacity = 0f
                        }
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0.05f,
                    Width = 100f,
                    Height = 100f,
                    Rotation = 0f,
                    Frame = false
                }
            );

            #endregion Slow Beams

            #region Slow Beams

            this.Add
            (
                new RectEmitter
                {
                    Name = "Slow Beams",
                    Budget = 100,
                    Term = 1.5f,
                    ReleaseQuantity = 3,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(0f, 0.5019608f, 0.5019608f),
                        Variation = new Vector3(0f, 0f, 0.1f)
                    },
                    ReleaseOpacity = new VariableFloat
                    {
                        Value = 1f,
                        Variation = 0f
                    },
                    ReleaseScale = new VariableFloat
                    {
                        Value = 480f,
                        Variation = 0f
                    },
                    ReleaseRotation = new VariableFloat
                    {
                        Value = 0f,
                        Variation = 0f
                    },
                    ReleaseImpulse = new Vector2(0f, -50f),
                    ParticleTextureAssetName = "Images/Particles/BeamBlurred",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 0.3f,
                            MiddlePosition = 0.5f,
                            FinalOpacity = 0f
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0.25f,
                    Width = 120f,
                    Height = 100f,
                    Rotation = 0f,
                    Frame = false

                }
            );

            #endregion
        }
    }
}