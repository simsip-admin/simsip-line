 // Transformation matrices
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;

// Sky colors
float4 xHorizonColor;
float4 xSunColor;		
float4 xNightColor;

// Sky tints
float4 xMorningTint;		
float4 xEveningTint;	

float xTimeOfDay;

// Sky texture
Texture xTexture;
sampler TextureSampler = sampler_state 
	{ 
		texture = <xTexture> ; 
		magfilter = linear; 
		minfilter = linear; 
		mipfilter=linear; 
		AddressU = mirror; 
		AddressV = mirror;};

// TODO: What is this for?
Texture xTexture0;

// Output to pixel shader
 struct SkyDomeVertexToPixel
 {
#if SM4
     float4 Position         : SV_Position;
#else
	 float4 Position         : POSITION;
#endif
     float2 TextureCoords    : TEXCOORD0;
     float4 ObjectPosition    : TEXCOORD1;
 };
 
 // Output to screen
 struct SkyDomePixelToFrame
 {
     float4 Color : COLOR0;
 };
 
#if SM4
SkyDomeVertexToPixel SkyDomeVertexShader(float4 inPos : SV_Position, 
                                         float2 inTexCoords: TEXCOORD0)
#else
SkyDomeVertexToPixel SkyDomeVertexShader(float4 inPos : POSITION,
                                         float2 inTexCoords : TEXCOORD0)
#endif
 
 {   
	 // Transform vertex to homogeneous screen coordinate
     SkyDomeVertexToPixel Output = (SkyDomeVertexToPixel)0;
     float4x4 preViewProjection = mul (xView, xProjection);
     float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
     Output.Position = mul(inPos, preWorldViewProjection);
     
	 // Pass through texture coordinates
	 Output.TextureCoords = inTexCoords;

	 // Store original vertex position so it can be referenced in pixel shader
     Output.ObjectPosition = inPos;
     
	 // Pass output on to pixel shader
     return Output;    
 }
 
 SkyDomePixelToFrame SkyDomePixelShader(SkyDomeVertexToPixel PSIn)
 {
     SkyDomePixelToFrame Output = (SkyDomePixelToFrame)0;        
 
	 float4 topColor = xSunColor;
     float4 bottomColor = xHorizonColor;    
	 float4 nColor = xNightColor;

	 nColor *= (4 - PSIn.TextureCoords.y) * .125f;

     float4 cloudValue = tex2D(TextureSampler, PSIn.TextureCoords).r;

	 if(xTimeOfDay <= 12)
	 {
		bottomColor *= xTimeOfDay / 12;	
		topColor	*= xTimeOfDay / 12;	
		nColor		*= xTimeOfDay / 12;
		cloudValue	*= xTimeOfDay / 12;
	 }
	 else
	 {
		bottomColor *= (xTimeOfDay - 24) / -12;	
		topColor	*= (xTimeOfDay - 24) / -12;						
		nColor		*= (xTimeOfDay - 24) / -12;
		cloudValue	*= (xTimeOfDay - 24) / -12;
	 }

	 bottomColor += (xMorningTint * .05) * ((24 - xTimeOfDay)/24);
	 bottomColor += (xEveningTint * .05) * (xTimeOfDay / 24);	
	 topColor += nColor;
	 bottomColor += nColor;

     float4 baseColor = lerp(bottomColor, topColor, saturate((PSIn.ObjectPosition.y)/0.9f));
	 float4 outCloudValue = lerp(bottomColor, cloudValue, saturate((PSIn.ObjectPosition.y)/0.5f));

     Output.Color = lerp(baseColor, 1, outCloudValue);        
 
     return Output;
 }

 // IMPORTANT:
 // This appears to be the one that is being used

 SkyDomeVertexToPixel SkyStarDomeVertexShader(float4 inPos : POSITION, 
	                                          float2 inTexCoords: TEXCOORD0)
 {    
	 // Transform vertex to homogeneous screen coordinate
     SkyDomeVertexToPixel Output = (SkyDomeVertexToPixel)0;
     float4x4 preViewProjection = mul (xView, xProjection);
     float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
     Output.Position = mul(inPos, preWorldViewProjection);

	 // Pass through texture coordinates
	 Output.TextureCoords = inTexCoords;
     
	 // Store original vertex position so it can be referenced in pixel shader
	 Output.ObjectPosition = inPos;
     
	 // Pass output on to pixel shader
     return Output;    
 }

 SkyDomePixelToFrame SkyStarDomePixelShader(SkyDomeVertexToPixel PSIn)
 {
     SkyDomePixelToFrame Output = (SkyDomePixelToFrame)0;        
 
	 // Grab a copy of our sky colors for our own use
	 float4 topColor = xSunColor;
     float4 bottomColor = xHorizonColor;    
	 float4 nColor = xNightColor;

	 // Adjust our night color
	 // TODO: What is this doing?
	 nColor *= (4 - PSIn.TextureCoords.y) * .125f;

     float4 cloudValue = tex2D(TextureSampler, PSIn.TextureCoords).r;

	 if(xTimeOfDay <= 12)
	 {
		bottomColor *= xTimeOfDay / 12;	
		topColor	*= xTimeOfDay / 12;	
		nColor		*= xTimeOfDay / 12;
		cloudValue	*= (xTimeOfDay - 24) / -12;
	 }
	 else
	 {
		bottomColor *= (xTimeOfDay - 24) / -12;	
		topColor	*= (xTimeOfDay - 24) / -12;						
		nColor		*= (xTimeOfDay - 24) / -12;
		cloudValue	*= xTimeOfDay / 12;
	 }

	 bottomColor += (xMorningTint * .05) * ((24 - xTimeOfDay)/24);
	 bottomColor += (xEveningTint * .05) * (xTimeOfDay / 24);	
	 topColor += nColor;
	 bottomColor += nColor;

     float4 baseColor = lerp(bottomColor, topColor, saturate((PSIn.ObjectPosition.y)/0.9f));
	 float4 outCloudValue = lerp(bottomColor, cloudValue, saturate((PSIn.ObjectPosition.y)/0.5f));

     Output.Color = lerp(baseColor, 1, outCloudValue);        
 
     return Output;
 }

 