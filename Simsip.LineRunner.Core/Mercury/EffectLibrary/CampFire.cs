using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace Simsip.LineRunner.Mercury.EffectLibrary
{
    public class CampFire : ParticleEffect
    {
        public CampFire()
        {
            Name = "Camp Fire";

            #region Smoke

            this.Add
            (
                new LineEmitter
                {
                    Name = "Smoke",
                    Budget = 100,
                    Term = 3f,
                    ReleaseQuantity = 1,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 128f,
                        Variation = 64f
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
                        Value = 200f,
                        Variation = 100f
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
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 0.375f,
                            MiddlePosition = 0.5f,
                            FinalOpacity = 0f
                        },
                        new RotationModifier
                        {
                            RotationRate = 0.5f
                        },
                        new ColourModifier
                        {
                            InitialColour = new Vector3(0.2509804f, 0.184313729f, 0.156862751f),
                            UltimateColour = new Vector3(0.2509804f, 0.2509804f, 0.2509804f)
                        },
                    },
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0f, -30f),
                    MinimumTriggerPeriod = 0.1f,
                    Length = 150f,
                    Angle = 0f,
                    Rectilinear = true,
                    EmitBothWays = false
                }
            );

            #endregion

            #region Embers

            this.Add
            (
                new LineEmitter
                {
                    Name = "Embers",
                    Budget = 100,
                    Term = 3f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 192f,
                        Variation = 64f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(1f, 0.5019608f, 0f),
                        Variation = new Vector3(0.25f, 0f, 0f)
                    },
                    ReleaseOpacity = new VariableFloat
                    {
                        Value = 1f,
                        Variation = 0f
                    },
                    ReleaseScale = new VariableFloat
                    {
                        Value = 4f,
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
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 1f,
                            MiddlePosition = 0.5f,
                            FinalOpacity = 0f
                        },
                        new DampingModifier
                        {
                            DampingCoefficient = 0.8f
                        },
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0.15f,
                    Length = 150f,
                    Angle = 0f,
                    Rectilinear = true,
                    EmitBothWays = false
                }
            );

            #endregion

            #region Flames1

            this.Add
            (
                new LineEmitter
                {
                    Name = "Flames1",
                    Budget = 1000,
                    Term = 1f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 32f,
                        Variation = 32f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(0.7529412f, 0.2509804f, 0f),
                        Variation = new Vector3(0.25f, 0f, 0f)
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
                    ParticleTextureAssetName = "Images/Particles/Flame",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 0.65f,
                            MiddlePosition = 0.2f,
                            FinalOpacity = 0f
                        },
                        new LinearGravityModifier
                        {
                            Gravity = new Vector2(0f, -150f)
                        },
                        new ScaleModifier
                        {
                            InitialScale = 64f,
                            UltimateScale = 32f
                        },
                        new RotationModifier
                        {
                            RotationRate = 1f
                        }
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f,
                    Length = 150f,
                    Angle = 0f,
                    Rectilinear = true,
                    EmitBothWays = false
                }
            );
      
            #endregion

            #region Flames2

            this.Add
            (
                new LineEmitter
                {
                    Name = "Flames2",
                    Budget = 1000,
                    Term = 1f,
                    ReleaseQuantity = 1,
                    Enabled = true,
                    ReleaseSpeed = new VariableFloat
                    {
                        Value = 32f,
                        Variation = 32f
                    },
                    ReleaseColour = new VariableFloat3
                    {
                        Value = new Vector3(0.7529412f, 0.2509804f, 0f),
                        Variation = new Vector3(0.25f, 0f, 0f)
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
                    ParticleTextureAssetName = "Images/Particles/Flame",
                    Modifiers = new ModifierCollection
                    {
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0f,
                            MiddleOpacity = 0.65f,
                            MiddlePosition = 0.2f,
                            FinalOpacity = 0f
                        },
                        new LinearGravityModifier
                        {
                            Gravity = new Vector2(0f, -150f)
                        },
                        new ScaleModifier
                        {
                            InitialScale = 64f,
                            UltimateScale = 32f
                        },
                        new RotationModifier
                        {
                            RotationRate = -1f
                        }
                    },
                    BlendMode = EmitterBlendMode.Add,
                    TriggerOffset = new Vector2(0f, 0f),
                    MinimumTriggerPeriod = 0f,
                    Length = 150f,
                    Angle = 0f,
                    Rectilinear = true,
                    EmitBothWays = false
                }
            );

            #endregion

        }
    }
}