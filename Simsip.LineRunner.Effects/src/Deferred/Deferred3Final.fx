
#include "..\..\inc\Macros.fxh"

DECLARE_TEXTURE(ColorMap,   0);	// From our multiple render effect, our color map
DECLARE_TEXTURE(ShadingMap, 1);	// From our lights render effect, our built up shading map

BEGIN_CONSTANTS

// Base level minimum ambient light
float Ambient _vs(c0) _ps(c0) _cb(c0);

END_CONSTANTS

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
	float4 color = SAMPLE_TEXTURE(ColorMap, PSIn.TexCoord);
	
	// Sample shading from shading map generated in shading targets effect
	// TODO Why is this just float?
	float shading = SAMPLE_TEXTURE(ShadingMap, PSIn.TexCoord);
	
	// Adjust color by ambient light and shading amount
	Output.Color = color * (Ambient + shading);
	
	return Output;
}

// NOTE: The order of the techniques here are defined to match the indexing
//       in the C# Effect derived subclass that represents this effect.
TECHNIQUE( CombineColorAndShading, CombineVertexShader, CombinePixelShader );
