
#include "..\..\inc\Macros.fxh"

BEGIN_CONSTANTS

float4x4 World       _vs(c0) _ps(c0) _cb(c0);
float4x4 View        _vs(c4) _ps(c4) _cb(c4);
float4x4 Projection  _vs(c8) _ps(c8) _cb(c8);

// Support for converting float to unit [0,1]
float UnitConverter  _vs(c12) _ps(c12) _cb(c12);

// Clip plane support
// Note: Had to use float instead of bool due to error on DX9 builds
// "IF src0 must have replicate swizzle"
// https://monogame.codeplex.com/discussions/391687
float IsClip         _vs(c13) _ps(c13) _cb(c13);
float4 ClippingPlane _vs(c14) _ps(c14) _cb(c14);

END_CONSTANTS

// Output to pixel shader
struct VertexToPixel
{
#if SM4
	float4 Position			: SV_Position;
#else
	float4 Position			: POSITION0;
#endif
	float4 ScreenPos		: TEXCOORD1;
	float4 Clipping         : TEXCOORD2;
};

// Output to render target
struct PixelToFrame
{
	float4 Color 			: COLOR0;
};

// http://skytiger.wordpress.com/2010/12/01/packing-depth-into-color/
float4 UnitToColor32(in float unit) {
    const float4 factor = float4(1, 255, 65025, 16581375);
    const float mask = 1.0 / 256.0;
    float4 color = unit * factor;
    color.gba = frac(color.gba);
    color.rgb -= color.gba * mask;
    return color;
}

// TODO: Why do we have normal here?
#if SM4
VertexToPixel ShadowMapVertexShader(float4 inPos: SV_Position, 
                                    float3 inNormal: NORMAL0)
#else
VertexToPixel ShadowMapVertexShader(float4 inPos: POSITION0,
									float3 inNormal : NORMAL0)
#endif
{
	VertexToPixel Output = (VertexToPixel)0;	
	
	// Our goal is to get the depth recorded of the nearest object to the light.
	// Hence the view/projection are for the light and the vertex is for the object
	// being rendered.
	//
	// Hence, we transform incoming position as usual to screen position 
	// in homogeneous coordinates
	float4x4 preViewProjection = mul(View, Projection);
	float4x4 preWorldViewProjection = mul(World, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);		
	
	// Store away screen position in homogeneous coordinates so we can grab it
	// in pixel shader
	Output.ScreenPos = Output.Position;
	
	// If we are clipping, clip in world coordinates
	// IMPORTANT: See commented out version below this snippet for version we would like to use.
	//            This is not working on Android as the xIsClip never evaluates to 1 when we want
	//            to flag we are clipping. The xIsClip is evaluated correctly in the Pixel Shader.
	//            Leaving the commented out portion in place for future version of MG2FX where this
	//            might be corrected.
	Output.Clipping = 0;
	float4 clp = mul(inPos, World);
	Output.Clipping.x = dot(clp, ClippingPlane);
	/*
	if (xIsClip != 0f)
	{
	float4 clp = mul(inPos, xWorld);
	Output.Clipping.x = dot(clp, xClippingPlane);
	}
	*/

	return Output;
}

PixelToFrame ShadowMapPixelShader(VertexToPixel PSIn) : COLOR0
{
	// Do we have a clipping plane to consider?
	if (IsClip != 0)
	{
		clip(PSIn.Clipping.x);
	}

	PixelToFrame Output = (PixelToFrame)0;		
	
	// Convert our Z position back from homogeneous coordinates
	float depth = PSIn.ScreenPos.z; //  / PSIn.ScreenPos.w;
	
	// Convert depth to unit [0,1]
	float depthUnit = (depth / UnitConverter) + 0.5f;

	// Encode modified Z position into a Color and record to render target Color0
	Output.Color = 	UnitToColor32(depthUnit);
	
	// Render target Color0, which will contain at the end of a full render pass
	// the depths from the lights perspective of the nearest objects.
	return Output;
}

// NOTE: The order of the techniques here are defined to match the indexing
//       in the C# Effect derived subclass that represents this effect.
TECHNIQUE( ShadowMap, ShadowMapVertexShader, ShadowMapPixelShader );
