float xTime;
float xOvercast;

// Static cloud map
Texture xTexture;
sampler TextureSampler = sampler_state 
	{ 
		texture = <xTexture> ; 
		magfilter = linear; 
		minfilter = linear; 
		mipfilter=linear;
		AddressU = mirror; 
		AddressV = mirror;};

// Output to pixel shader
struct PerlinVertexToPixel
{
#if SM4
    float4 Position         : SV_Position;
#else
	float4 Position         : POSITION;
#endif
	float2 TextureCoords    : TEXCOORD0;
};

// Output to screen
struct PerlinPixelToFrame
{
    float4 Color : COLOR0;
};

#if SM4
PerlinVertexToPixel PerlinVertexShader(float4 inPos : SV_Position, 
	                                   float2 inTexCoords: TEXCOORD)
#else
PerlinVertexToPixel PerlinVertexShader(float4 inPos : POSITION,
									   float2 inTexCoords : TEXCOORD)
#endif
{    
    PerlinVertexToPixel Output = (PerlinVertexToPixel)0;
    
	// Pass-through both vertex position and texture coordinate
    Output.Position = inPos;
    Output.TextureCoords = inTexCoords;
    
	// Pass output on to pixel shader
    return Output;    
}

PerlinPixelToFrame PerlinPixelShader(PerlinVertexToPixel PSIn)
{
    PerlinPixelToFrame Output = (PerlinPixelToFrame)0;    
    
    float2 move = float2(0,1);
    float4 perlin = tex2D(TextureSampler, (PSIn.TextureCoords)+xTime*move)/2;
    perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*2+xTime*move)/4;
    perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*4+xTime*move)/8;
    perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*8+xTime*move)/16;
    perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*16+xTime*move)/32;
    perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*32+xTime*move)/32;    
    
    Output.Color.rgb = 1.0f-pow(perlin.r, xOvercast)*2.0f;

    Output.Color.a = 1;

    return Output;
}

