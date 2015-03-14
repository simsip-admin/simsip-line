//------- Constants --------
float4x4 xView;
float4x4 xReflectionView;
float4x4 xProjection;
float4x4 xWorld;
float3 xLightDirection;
float xAmbient;
float xWaveLength;
float xWaveHeight;
float3 xCamPos;
float xTime;
float xWindForce;
float3 xWindDirection;

sampler ReflectionSampler = sampler_state { texture = <xReflectionMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};Texture xRefractionMap;

sampler RefractionSampler = sampler_state { texture = <xRefractionMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};Texture xWaterBumpMap;

sampler WaterBumpMapSampler = sampler_state { texture = <xWaterBumpMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};


//------- Technique: Water --------
struct WVertexToPixel
{
#if SM4
    float4 Position                 : SV_Position;
#else
	float4 Position                 : POSITION;
#endif
    float4 ReflectionMapSamplingPos    : TEXCOORD1;
    float2 BumpMapSamplingPos        : TEXCOORD2;
    float4 RefractionMapSamplingPos : TEXCOORD3;
    float4 Position3D                : TEXCOORD4;
};

struct WPixelToFrame
{
    float4 Color : COLOR0;
};

#if SM4
WVertexToPixel WaterVS(float4 inPos : SV_Position, 
	                   float2 inTex: TEXCOORD)
#else
WVertexToPixel WaterVS(float4 inPos : POSITION,
                       float2 inTex : TEXCOORD)
#endif
{    
    WVertexToPixel Output = (WVertexToPixel)0;

    float4x4 preViewProjection = mul (xView, xProjection);
    float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    float4x4 preReflectionViewProjection = mul (xReflectionView, xProjection);
    float4x4 preWorldReflectionViewProjection = mul (xWorld, preReflectionViewProjection);

    Output.Position = mul(inPos, preWorldViewProjection);
    Output.ReflectionMapSamplingPos = mul(inPos, preWorldReflectionViewProjection);    
    Output.RefractionMapSamplingPos = mul(inPos, preWorldViewProjection);
    Output.Position3D = mul(inPos, xWorld);        
    
    float3 windDir = normalize(xWindDirection);    
    float3 perpDir = cross(xWindDirection, float3(0,1,0));
    float ydot = dot(inTex, xWindDirection.xz);
    float xdot = dot(inTex, perpDir.xz);
    float2 moveVector = float2(xdot, ydot);
    moveVector.y += xTime*xWindForce;    
    Output.BumpMapSamplingPos = moveVector/xWaveLength;    

    
    return Output;
}

WPixelToFrame WaterPS(WVertexToPixel PSIn)
{
    WPixelToFrame Output = (WPixelToFrame)0;        
    
    float4 bumpColor = tex2D(WaterBumpMapSampler, PSIn.BumpMapSamplingPos);
    float2 perturbation = xWaveHeight*(bumpColor.rg - 0.5f)*2.0f;
    
    float2 ProjectedTexCoords;
    ProjectedTexCoords.x = PSIn.ReflectionMapSamplingPos.x/PSIn.ReflectionMapSamplingPos.w/2.0f + 0.5f;
    ProjectedTexCoords.y = -PSIn.ReflectionMapSamplingPos.y/PSIn.ReflectionMapSamplingPos.w/2.0f + 0.5f;        
    float2 perturbatedTexCoords = ProjectedTexCoords + perturbation;
    float4 reflectiveColor = tex2D(ReflectionSampler, perturbatedTexCoords);
    
    float2 ProjectedRefrTexCoords;
    ProjectedRefrTexCoords.x = PSIn.RefractionMapSamplingPos.x/PSIn.RefractionMapSamplingPos.w/2.0f + 0.5f;
    ProjectedRefrTexCoords.y = -PSIn.RefractionMapSamplingPos.y/PSIn.RefractionMapSamplingPos.w/2.0f + 0.5f;    
    float2 perturbatedRefrTexCoords = ProjectedRefrTexCoords + perturbation;    
    float4 refractiveColor = tex2D(RefractionSampler, perturbatedRefrTexCoords);
    
    float3 eyeVector = normalize(xCamPos - PSIn.Position3D);
    float3 normalVector = (bumpColor.rbg-0.5f)*2.0f;
    
    float fresnelTerm = dot(eyeVector, normalVector);    
    float4 combinedColor = lerp(reflectiveColor, refractiveColor, fresnelTerm);
    
    float4 dullColor = float4(0.3f, 0.3f, 0.5f, 1.0f);
    
    Output.Color = lerp(combinedColor, dullColor, 0.2f);    
    
    float3 reflectionVector = -reflect(xLightDirection, normalVector);
    float specular = dot(normalize(reflectionVector), normalize(eyeVector));
    specular = pow(specular, 256);        
    Output.Color.rgb += specular;
    
    return Output;
}

