// TODO: Check code on this one
// var viewProjInv = Matrix.Invert(this._view * this._projection);
float4x4 xViewProjectionInv;

// A pre-determine view projection matrix for this light
// this._effect2Lights.Parameters["xLightViewProjection"].SetValue(light.ViewMatrix * light.ProjectionMatrix);
float4x4 xLightViewProjection;

// Common
float3 xLightPosition;

// Directed
float3 xLightDirection;

// Point
// TODO: Falloff

// Spotlight
float xLightStrength;
float3 xConeDirection;
float xConeAngle;
float xConeDecay;

// Support for converting float to unit [0,1]
float xUnitConverter;

// From our multiple render effect, our normal map
Texture xNormalMap;
sampler NormalMapSampler = sampler_state 
	{ 
		texture = <xNormalMap> ; 
		magfilter = POINT; 
		minfilter = POINT; 
		mipfilter=LINEAR; 
		AddressU = mirror; 
		AddressV = mirror;
	};

// From our multiple render effect, our depth map
Texture xDepthMap;
sampler DepthMapSampler = sampler_state 
	{ 
		texture = <xDepthMap> ; 
		magfilter = POINT; 
		minfilter = POINT; 
		mipfilter=LINEAR; 
		AddressU = mirror; 
		AddressV = mirror;
	};

// From our shadow map effect, the shadow map for this light
Texture xShadowMap;
sampler ShadowMapSampler = sampler_state 
	{ 
		texture = <xShadowMap> ; 
		magfilter = LINEAR; 
		minfilter = LINEAR; 
		mipfilter=LINEAR; 
		AddressU = mirror; 
		AddressV = mirror;
	};

// If multiple lights, the previous shading map containing contributions for all lights up to this one
Texture xPreviousShadingContents;
sampler PreviousSampler = sampler_state 
	{ 
		texture = <xPreviousShadingContents> ; 
		magfilter = LINEAR; 
		minfilter = LINEAR; 
		mipfilter=LINEAR; 
		AddressU = mirror; 
		AddressV = mirror;
	};

// Output to pixel shader
struct VertexToPixel
	{
#if SM4
		float4 Position					: SV_Position;
#else
		float4 Position					: POSITION0;
#endif
		float2 TexCoord					: TEXCOORD0;
	};

// Output to render target
struct PixelToFrame
	{
		float4 Color			: COLOR0;
	};

// http://skytiger.wordpress.com/2010/12/01/packing-depth-into-color/
float ColorToUnit32(in float4 color) 
{
    const float4 factor = 1.0 / float4(1, 255, 65025, 16581375);
    return dot(color, factor);
}

#if SM4
VertexToPixel MyVertexShader(float4 inPos: SV_Position,
                             float2 texCoord: TEXCOORD0)
#else
VertexToPixel MyVertexShader(float4 inPos: POSITION0,
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

PixelToFrame DirectedLightPixelShader(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	// Sample normal from normal map and convert back from [0,1] range
	float3 normal = tex2D(NormalMapSampler, PSIn.TexCoord).rgb;
	normal = normal*2.0f-1.0f;
	normal = normalize(normal);
	
	// Sample depth from depth map and convert back from [0,1] range
	float4 depthColor = tex2D(DepthMapSampler, PSIn.TexCoord);
	float depthUnit = ColorToUnit32(depthColor);
	float depth = (depthUnit - 0.5f) * xUnitConverter;
	
	// Create screen position
	float4 screenPos = float4(1, 1, 1, 1);
	screenPos.x = PSIn.TexCoord.x*2.0f - 1.0f;
	screenPos.y = -(PSIn.TexCoord.y *2.0f - 1.0f);
	screenPos.z = depth;
	screenPos.w = 1.0f;	
	
	// Transform to 3D position
	float4 worldPos = mul(screenPos, xViewProjectionInv);
	worldPos /= worldPos.w;		
	
	// Find screen position as seen by the light
	float4 lightScreenPos = mul(worldPos, xLightViewProjection);
	// lightScreenPos /= lightScreenPos.w;
	
	// Find sample position in shadow map
	float2 lightSamplePos = float2(1,1);
	lightSamplePos.x = lightScreenPos.x/2.0f+0.5f;
	lightSamplePos.y = (-lightScreenPos.y/2.0f+0.5f);
	
	// Determine shadowing criteria
	float realDistanceToLight = lightScreenPos.z;	
	float4 distanceStoredInDepthMapColor = tex2D(ShadowMapSampler, lightSamplePos);
	float distanceStoredInDepthMapUnit = ColorToUnit32(distanceStoredInDepthMapColor);
	float distanceStoredInDepthMap = (depthUnit - 0.5f) * xUnitConverter;
	
	// Calculate shading
	float shading = 0;
	bool inShadow =  distanceStoredInDepthMap <= (realDistanceToLight - 1.0f/100.0f);	
	if (!inShadow)
	{
		float3 lightDirection = normalize(xLightDirection);		
		shading = dot(normal, -lightDirection);	
	}	

	float4 previous = tex2D(PreviousSampler, PSIn.TexCoord);
	Output.Color = previous + shading;
	
	return Output;
}

PixelToFrame PointLightPixelShader(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	//sample normal from normal map
	float3 normal = tex2D(NormalMapSampler, PSIn.TexCoord).rgb;
	normal = normal*2.0f-1.0f;
	normal = normalize(normal);
	
	//sample depth from depth map
	float depth = tex2D(DepthMapSampler, PSIn.TexCoord).r;
	
	//create screen position
	float4 screenPos;
	screenPos.x = PSIn.TexCoord.x*2.0f-1.0f;
	screenPos.y = -(PSIn.TexCoord.y*2.0f-1.0f);
	screenPos.z = depth;
	screenPos.w = 1.0f;	
	
	//transform to 3D position
	float4 worldPos = mul(screenPos, xViewProjectionInv);
	worldPos /= worldPos.w;		
	
	//find screen position as seen by the light
	float4 lightScreenPos = mul(worldPos, xLightViewProjection);
	lightScreenPos /= lightScreenPos.w;
	
	//find sample position in shadow map
	float2 lightSamplePos;
	lightSamplePos.x = lightScreenPos.x/2.0f+0.5f;
	lightSamplePos.y = (-lightScreenPos.y/2.0f+0.5f);
	
	//determine shadowing criteria
	float realDistanceToLight = lightScreenPos.z;	
	float distanceStoredInDepthMap = tex2D(ShadowMapSampler, lightSamplePos);	
	bool shadowCondition =  distanceStoredInDepthMap <= realDistanceToLight - 1.0f/100.0f;	
	
	//calculate shading
	float shading = 0;
	if (!shadowCondition)
	{
		float3 lightDirection = worldPos - xLightPosition;	
		float distance = length(lightDirection);
		
		lightDirection = normalize(lightDirection);
		
		shading = dot(normal, -lightDirection);	
		shading /= distance;
	}	

	float4 previous = tex2D(PreviousSampler, PSIn.TexCoord);
	Output.Color = previous + shading;
	
	return Output;
}


PixelToFrame SpotLightPixelShader(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;

	//sample normal from normal map
	float3 normal = tex2D(NormalMapSampler, PSIn.TexCoord).rgb;
	normal = normal*2.0f - 1.0f;
	normal = normalize(normal);

	//sample depth from depth map
	float depth = tex2D(DepthMapSampler, PSIn.TexCoord).r;

	//create screen position
	float4 screenPos;
	screenPos.x = PSIn.TexCoord.x*2.0f - 1.0f;
	screenPos.y = -(PSIn.TexCoord.y*2.0f - 1.0f);
	screenPos.z = depth;
	screenPos.w = 1.0f;

	//transform to 3D position
	float4 worldPos = mul(screenPos, xViewProjectionInv);
		worldPos /= worldPos.w;

	//find screen position as seen by the light
	float4 lightScreenPos = mul(worldPos, xLightViewProjection);
		lightScreenPos /= lightScreenPos.w;

	//find sample position in shadow map
	float2 lightSamplePos;
	lightSamplePos.x = lightScreenPos.x / 2.0f + 0.5f;
	lightSamplePos.y = (-lightScreenPos.y / 2.0f + 0.5f);

	//determine shadowing criteria
	float realDistanceToLight = lightScreenPos.z;
	float distanceStoredInDepthMap = tex2D(ShadowMapSampler, lightSamplePos);
	bool shadowCondition = distanceStoredInDepthMap <= realDistanceToLight - 1.0f / 100.0f;

	//determine cone criteria
	float3 lightDirection = normalize(worldPos - xLightPosition);
		float coneDot = dot(lightDirection, normalize(xConeDirection));
	bool coneCondition = coneDot >= xConeAngle;

	//calculate shading
	float shading = 0;
	// TODO: Had to take out temporarily due to
	// error X5608: Compiled shader code uses too many arithmetic instruction slots
	// Reference: http://quest3d.com/forum/index.php?topic=67944.0
	//
	//if (coneCondition && !shadowCondition)
	//{
	//float coneAttenuation = pow(coneDot, xConeDecay);
	//shading = dot(normal, -lightDirection);
	//shading *= xLightStrength;
	//shading *= coneAttenuation;
	//}

	float4 previous = tex2D(PreviousSampler, PSIn.TexCoord);
	Output.Color = previous + shading;

	return Output;
}

