using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace Simsip.LineRunner.Mercury.EffectLibrary
{
    public class WaterJet : ParticleEffect
    {
        public WaterJet()
        {
            #region Mist1

            this.Add
            (
                new ConeEmitter
                {
                    Name = "Mist1",
                    Budget = 1100,
                    Term = 3f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 160f,
                        Variation = 32f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(0.8666667f, 0.945098042f, 1f),
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
                        Variation = 3.14f
                    },
                    ReleaseImpulse = new Vector2(0f,  0f),
                    ParticleTextureAssetName = "Images/Particles/Water001",
                    Modifiers = new ModifierCollection 
                    { 
                        new LinearGravityModifier
                        {
                            Gravity = new Vector2(0f,100f)
                        },
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 0.2f,
                            MiddlePosition = 0.8f,
                            FinalOpacity = 0f
                        },
                        new RotationModifier
                        {
                            RotationRate = 1.57079637f
                        },
                        new ColourModifier
                        {
                            InitialColour = new Vector3(0.8745098f, 1f, 0.996078432f),
                            UltimateColour = new Vector3(0.819607854f, 0.8509804f, 0.8784314f)

                        },
                        new ScaleInterpolatorModifier
                        {
                            InitialScale = 0f,
                            MiddleScale = 64f,
                            MiddlePosition = 0f,
                            FinalScale = 320f
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f,
                    Direction = 3.514758f,
                    ConeAngle = 0.137341261f
                }
            );

            #endregion

            #region Spray

            this.Add
            (
                new ConeEmitter
                {
                    Name = "Spray",
                    Budget = 1100,
                    Term = 3f,
                    ReleaseQuantity = 3,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 160f,
                        Variation = 32
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(0.8666667f, 0.945098042f, 1f),
                        Variation = new Vector3(0f,0f,0f)
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
                    ParticleTextureAssetName = "Images/Particles/Spray001",
                    Modifiers = new ModifierCollection
                    {
                        new LinearGravityModifier
                        {
                            Gravity = new Vector2(0f, 100f)
                        },
                        new TrajectoryRotationModifier
                        {
                            RotationOffset = 3.14f
                        },
                        new ScaleModifier
                        {
                            InitialScale = 32f,
                            UltimateScale = 160f
                        },
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0.5f,
                            MiddleOpacity = 0.5f,
                            MiddlePosition = 0.5f,
                            FinalOpacity = 0f
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f,
                    Direction = 3.500859f,
                    ConeAngle = 0.225750238f,
                }
            );

            #endregion

            #region Mist2

            this.Add
            (
                new ConeEmitter
                {
                    Name = "Mist2",
                    Budget = 1100,
                    Term = 3f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 160f,
                        Variation = 32f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(0.8666667f, 0.945098042f, 1f),
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
                        Variation = 3.14f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/Water001",
                    Modifiers = new ModifierCollection
                    {
                        new LinearGravityModifier
                        {
                            Gravity = new Vector2(0f, 100f)
                        },
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 0.1f,
                            MiddlePosition = 0.8f,
                            FinalOpacity = 0f
                        },
                        new RotationModifier
                        {
                            RotationRate = -1.57079637f
                        },
                        new ColourModifier
                        {
                            InitialColour = new Vector3(0.8745098f, 1f, 0.996078432f),
                            UltimateColour = new Vector3(0.819607854f, 0.8509804f, 0.8784314f)
                        },
                        new ScaleInterpolatorModifier
                        {
                            InitialScale = 0f,
                            MiddleScale = 64f,
                            MiddlePosition = 0.5f,
                            FinalScale = 320f
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f,
                    Direction = 3.514758f,
                    ConeAngle = 0.137341261f
                }
            );

            #endregion
        }
    }
}