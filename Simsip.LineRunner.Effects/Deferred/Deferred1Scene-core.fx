
// Standard transform matrices
float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

// Standard texture setup
Texture xTexture;
sampler TextureSampler = sampler_state 
	{ 
		texture = <xTexture> ; 
		magfilter = LINEAR; 
		minfilter = LINEAR; 
		mipfilter=LINEAR; 
		AddressU = wrap; 
		AddressV = wrap;
	};

// Support for converting float to unit [0,1]
float xUnitConverter;

// Clip plane support
// Note: Had to use float instead of bool due to error on DX9 builds
// "IF src0 must have replicate swizzle"
// https://monogame.codeplex.com/discussions/391687
float xIsClip;
float4 xClippingPlane;

// Output to pixel shader
struct VertexToPixel
	{
#if SM4
		float4 Position			: SV_Position;
#else
		float4 Position			: POSITION0;
#endif
		float3 Normal			: TEXCOORD0;
		float4 ScreenPos		: TEXCOORD1;
		float2 TexCoords		: TEXCOORD2;
		float4 Clipping         : TEXCOORD3;
	};

// Output to render targets
struct PixelToFrame
	{
		float4 Color 			: COLOR0;
		float4 Normal 			: COLOR1;
		float4 Depth 			: COLOR2;
	};
	
// Our final output to screen - single pass implementation
struct PixelToFrameSingleColor
{
	float4 Color			: COLOR0;
};
struct PixelToFrameSingleNormal
{
	float4 Normal			: COLOR0;
};
struct PixelToFrameSingleDepth
{
	float4 Depth			: COLOR0;
};

// http://skytiger.wordpress.com/2010/12/01/packing-depth-into-color/
float4 UnitToColor32(in float unit) 
{
    const float4 factor = float4(1, 255, 65025, 16581375);
    const float mask = 1.0 / 256.0;
    float4 color = unit * factor;
    color.gba = frac(color.gba);
    color.rgb -= color.gba * mask;
    return color;
}

#if SM4
VertexToPixel MultipleTargetsVertexShader(float4 inPos: SV_Position, 
                                          float3 inNormal: NORMAL0, 
										  float2 inTexCoords: TEXCOORD0)
#else
VertexToPixel MultipleTargetsVertexShader(float4 inPos: POSITION0,
	float3 inNormal : NORMAL0,
	float2 inTexCoords : TEXCOORD0)
#endif
{
	VertexToPixel Output = (VertexToPixel)0;

	// Transform incoming position to screen position in homogeneous coordinates
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);		
	
	// Transform incoming normal for this vertex by world rotation only
	float3x3 rotMatrix = (float3x3)xWorld;
	float3 rotNormal = mul(inNormal, rotMatrix);
	Output.Normal = rotNormal;
	
	// Store away screen position in homogeneous coordinates so we can grab it
	// in pixel shader
	Output.ScreenPos = Output.Position;
	
	// Pass texture coordinates for this vertex through without modification
	Output.TexCoords = inTexCoords;

	// If we are clipping, clip in world coordinates
	// IMPORTANT: See commented out version below this snippet for version we would like to use.
	//            This is not working on Android as the xIsClip never evaluates to 1 when we want
	//            to flag we are clipping. The xIsClip is evaluated correctly in the Pixel Shader.
	//            Leaving the commented out portion in place for future version of MG2FX where this
	//            might be corrected.
	Output.Clipping = 0;
	float4 clp = mul(inPos, xWorld);
	Output.Clipping.x = dot(clp, xClippingPlane);
	/*
	if (xIsClip != 0f)
	{
		float4 clp = mul(inPos, xWorld);
		Output.Clipping.x = dot(clp, xClippingPlane);
	}
	*/

	return Output;
}

PixelToFrame MultipleTargetsPixelShader(VertexToPixel PSIn)
{

	PixelToFrame Output = (PixelToFrame)0;	

	// Do we have a clipping plane to consider?
	if (xIsClip != 0)
	{
		clip(PSIn.Clipping.x);
	}

	// Record color to render target Color0
	Output.Color.rgb = tex2D(TextureSampler, PSIn.TexCoords);
	
	// Modify normal to [0,1] range and record to render target Color1
	Output.Normal.xyz = PSIn.Normal/2.0f+0.5f;
	
	// Convert our Z position back from homogeneous coordinates
	float depth = PSIn.ScreenPos.z; // PSIn.ScreenPos.z/PSIn.ScreenPos.w;
	
	// Convert depth to unit [0,1]
	float depthUnit = (depth / xUnitConverter) + 0.5f;
	
	// Encode modified Z position into a Color and record to render target Color2
	Output.Depth = 	UnitToColor32(depthUnit);
	 
	// Bring depth value into the [0,1] range
	// projection_limit can be passed in from c# code
	// depth needs to be first clamped to [-projection_limit, projection_limit]
	// Output.Depth.r = (depth/2 + projection_limit/2)/projection_limit
	// 
	// Bring from the [0,1] range back to its original value
	// float3 normal = tex2D(NormalMapSampler, PSIn.TexCoord).rgb;
	// normal = normal*2.0f-1.0f;
	//
	// float depth = tex2D(DepthMapSampler, PSIn.TexCoord2).r
	// depth = (depth*projection_limit*2f) - projection_limit
	// 
	// Try same for shadow mapping, otherwise use id-based shadow mapping
	// How to setup id's?
	// Category base id's for stamping Box2DModel.Id?
	// Output.Depth.r = 	depth;
	
	// Render multiple targets Color0, Color1 and Color2
	return Output;
}

PixelToFrameSingleColor SingleTargetColorPixelShader(VertexToPixel PSIn) : COLOR0
{
	PixelToFrameSingleColor Output = (PixelToFrameSingleColor)0;

	// Do we have a clipping plane to consider?
	if (xIsClip != 0)
	{
		clip(PSIn.Clipping.x);
	}

	// Record color to render target Color0
	Output.Color.rgb = tex2D(TextureSampler, PSIn.TexCoords);

	// Render single target Color0
	return Output;
}

PixelToFrameSingleNormal SingleTargetNormalPixelShader(VertexToPixel PSIn) : COLOR0
{

	PixelToFrameSingleNormal Output = (PixelToFrameSingleNormal)0;

	// Do we have a clipping plane to consider?
	if (xIsClip != 0)
	{
		clip(PSIn.Clipping.x);
	}

	// Modify normal to [0,1] range and record to render target Color1
	Output.Normal.xyz = PSIn.Normal / 2.0f + 0.5f;

	// Render single target Color0
	return Output;
}

PixelToFrameSingleDepth SingleTargetDepthPixelShader(VertexToPixel PSIn) : COLOR0
{
	PixelToFrameSingleDepth Output = (PixelToFrameSingleDepth)0;

	// Do we have a clipping plane to consider?
	if (xIsClip != 0)
	{
		clip(PSIn.Clipping.x);
	}

	// Convert our Z position back from homogeneous coordinates
	float depth = PSIn.ScreenPos.z; // PSIn.ScreenPos.z/PSIn.ScreenPos.w;

	// Convert depth to unit [0,1]
	float depthUnit = (depth / xUnitConverter) + 0.5f;

	// Encode modified Z position into a Color and record to render target Color2
	Output.Depth = UnitToColor32(depthUnit);

	// Bring depth value into the [0,1] range
	// projection_limit can be passed in from c# code
	// depth needs to be first clamped to [-projection_limit, projection_limit]
	// Output.Depth.r = (depth/2 + projection_limit/2)/projection_limit
	// 
	// Bring from the [0,1] range back to its original value
	// float3 normal = tex2D(NormalMapSampler, PSIn.TexCoord).rgb;
	// normal = normal*2.0f-1.0f;
	//
	// float depth = tex2D(DepthMapSampler, PSIn.TexCoord2).r
	// depth = (depth*projection_limit*2f) - projection_limit
	// 
	// Try same for shadow mapping, otherwise use id-based shadow mapping
	// How to setup id's?
	// Category base id's for stamping Box2DModel.Id?
	// Output.Depth.r = 	depth;

	// Render single target Color0
	return Output;
}

