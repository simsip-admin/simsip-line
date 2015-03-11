// Base level minimum ambient light
float xAmbient;

// From our multiple render effect, our color map
Texture xColorMap;
sampler ColorMapSampler = sampler_state 
	{ 
		texture = <xColorMap> ; 
		magfilter = POINT; 
		minfilter = POINT; 
		mipfilter=LINEAR; 
		AddressU = mirror; 
		AddressV = mirror;
	};

// From our lights render effect, our built up shading map
Texture xShadingMap;
sampler ShadingMapSampler = sampler_state 
	{ 
		texture = <xShadingMap> ; 
		magfilter = POINT; 
		minfilter = POINT; 
		mipfilter=LINEAR; 
		AddressU = mirror; 
		AddressV = mirror;
	};

// Output to pixel shader
struct VertexToPixel
{
#if SM4
	float4 Position			: SV_Position;
#else
	float4 Position			: POSITION0;
#endif
	float2 TexCoord			: TEXCOORD0;
};

// Our final output to screen
struct PixelToFrame
{
	float4 Color			: COLOR0;
};

#if SM4
VertexToPixel CombineVertexShader(float4 inPos: SV_Position, 
                                  float2 texCoord: TEXCOORD0)
#else
VertexToPixel CombineVertexShader(float4 inPos: POSITION0,
  	                              float2 texCoord : TEXCOORD0)
#endif
{
	VertexToPixel Output = (VertexToPixel)0;
	
	// We are rendering to a pre-setup full screen vertex declaration,
	// so just pass through the values for position and texture coordinates.
	Output.Position = inPos;	
	Output.TexCoord = texCoord;

	return Output;
}

PixelToFrame CombinePixelShader(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	// Sample color from color map generated in multiple targets effect
	float4 color = tex2D(ColorMapSampler, PSIn.TexCoord);
	
	// Sample shading from shading map generated in shading targets effect
	// TODO Why is this just float?
	float shading = tex2D(ShadingMapSampler, PSIn.TexCoord);	
	
	// Adjust color by ambient light and shading amount
	Output.Color = color * (xAmbient + shading);
	
	return Output;
}

