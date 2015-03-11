using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace Simsip.LineRunner.Mercury.EffectLibrary
{
    public class BasicSmokePlume : ParticleEffect
    {
        public BasicSmokePlume()
        {
            Name = "BasicSmokePlume";

            #region Smoke 1

            this.Add
            (
                new ConeEmitter
                {
                    Name = "Smoke 1",
                    Budget = 500,
                    Term = 3f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 250f,
                        Variation = 100f
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
                        Variation = 3.14f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/Cloud001",
                    Modifiers = new ModifierCollection
                    {
                        new DampingModifier
                        {
                            DampingCoefficient = 0.95f
                        },
                        new ScaleModifier
                        {
                            InitialScale = 64f,
                            UltimateScale = 256f
                        },
                        new RotationRateModifier
                        {
                            InitialRate = 1.57079637f,
                            FinalRate = 0f
                        },
                        new LinearGravityModifier
                        {
                            Gravity = new Vector2(150f, 0f)
                        },
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 0.5f,
                            MiddlePosition = 0.1f,
                            FinalOpacity = 0f
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0.05f,
                    Direction = -1.57f,
                    ConeAngle = 0.6f
                }
            );

            #endregion

            #region Smoke 2

            this.Add
            (
                new ConeEmitter
                {
                    Name = "Smoke 2",
                    Budget = 500,
                    Term = 3f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 250,
                        Variation = 100
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
                        Variation = 3.14f
                    },
                    ReleaseImpulse = new Vector2(0f, 0f),
                    ParticleTextureAssetName = "Images/Particles/Cloud001",
                    Modifiers = new ModifierCollection
                    {
                        new DampingModifier
                        {
                            DampingCoefficient = 0.95f
                        },
                        new ScaleModifier
                        {
                            InitialScale = 64f,
                            UltimateScale = 256f
                        },
                        new RotationRateModifier
                        {
                            InitialRate = -1.57079637f,
                            FinalRate = 0f
                        },
                        new LinearGravityModifier 
                        {
                            Gravity = new Vector2(150f, 0)
                        },
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 0.5f,
                            MiddlePosition = 0.1f,
                            FinalOpacity = 0f
                        }
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0.05f,
                    Direction = -1.57f,
                    ConeAngle = 0.6f
                }
            );
            
            #endregion

           
        }
    }
}