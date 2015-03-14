// Transform matrices for light we are determining distances
// to objects in scene for
float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

// Support for converting float to unit [0,1]
float xUnitConverter;

// Clip plane support
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
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
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

PixelToFrame ShadowMapPixelShader(VertexToPixel PSIn) : COLOR0
{
	// Do we have a clipping plane to consider?
	if (xIsClip != 0)
	{
		clip(PSIn.Clipping.x);
	}

	PixelToFrame Output = (PixelToFrame)0;		
	
	// Convert our Z position back from homogeneous coordinates
	float depth = PSIn.ScreenPos.z; //  / PSIn.ScreenPos.w;
	
	// Convert depth to unit [0,1]
	float depthUnit = (depth / xUnitConverter) + 0.5f;

	// Encode modified Z position into a Color and record to render target Color0
	Output.Color = 	UnitToColor32(depthUnit);
	
	// Render target Color0, which will contain at the end of a full render pass
	// the depths from the lights perspective of the nearest objects.
	return Output;
}

