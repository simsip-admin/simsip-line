
#include "..\..\inc\Macros.fxh"

DECLARE_TEXTURE(NormalMap,               0);
DECLARE_TEXTURE(DepthMap,                1);
DECLARE_TEXTURE(ShadowMap,               2);
DECLARE_TEXTURE(PreviousShaderContents,  3);

BEGIN_CONSTANTS

// TODO: Check code on this one
// var viewProjInv = Matrix.Invert(this._view * this._projection);
float4x4 ViewProjectionInv   _vs(c0) _ps(c0) _cb(c0);

// A pre-determine view projection matrix for this light
// this._effect2Lights.Parameters["xLightViewProjection"].SetValue(light.ViewMatrix * light.ProjectionMatrix);
float4x4 LightViewProjection _vs(c4) _ps(c4) _cb(c4);

// Common
float3 LightPosition         _vs(c8) _ps(c8) _cb(c8);

// Directed
float3 LightDirection        _vs(c9) _ps(c9) _cb(c9);

// Point
// TODO: Falloff

// Spotlight
float LightStrength          _vs(c10) _ps(c10) _cb(c10);
float3 ConeDirection         _vs(c11) _ps(c11) _cb(c11);
float ConeAngle              _vs(c12) _ps(c12) _cb(c12);
float ConeDecay              _vs(c13) _ps(c13) _cb(c13);

// Support for converting float to unit [0,1]
float UnitConverter          _vs(c14) _ps(c14) _cb(c14);

END_CONSTANTS

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
	float3 normal = SAMPLE_TEXTURE(NormalMap, PSIn.TexCoord).rgb;
	normal = normal*2.0f-1.0f;
	normal = normalize(normal);
	
	// Sample depth from depth map and convert back from [0,1] range
	float4 depthColor = SAMPLE_TEXTURE(DepthMap, PSIn.TexCoord);
	float depthUnit = ColorToUnit32(depthColor);
	float depth = (depthUnit - 0.5f) * UnitConverter;
	
	// Create screen position
	float4 screenPos = float4(1, 1, 1, 1);
	screenPos.x = PSIn.TexCoord.x*2.0f - 1.0f;
	screenPos.y = -(PSIn.TexCoord.y *2.0f - 1.0f);
	screenPos.z = depth;
	screenPos.w = 1.0f;	
	
	// Transform to 3D position
	float4 worldPos = mul(screenPos, ViewProjectionInv);
	worldPos /= worldPos.w;		
	
	// Find screen position as seen by the light
	float4 lightScreenPos = mul(worldPos, LightViewProjection);
	// lightScreenPos /= lightScreenPos.w;
	
	// Find sample position in shadow map
	float2 lightSamplePos = float2(1,1);
	lightSamplePos.x = lightScreenPos.x/2.0f+0.5f;
	lightSamplePos.y = (-lightScreenPos.y/2.0f+0.5f);
	
	// Determine shadowing criteria
	float realDistanceToLight = lightScreenPos.z;	
	float4 distanceStoredInDepthMapColor = SAMPLE_TEXTURE(ShadowMap, lightSamplePos);
	float distanceStoredInDepthMapUnit = ColorToUnit32(distanceStoredInDepthMapColor);
	float distanceStoredInDepthMap = (depthUnit - 0.5f) * UnitConverter;
	
	// Calculate shading
	float shading = 0;
	bool inShadow =  distanceStoredInDepthMap <= (realDistanceToLight - 1.0f/100.0f);	
	if (!inShadow)
	{
		float3 lightDirection = normalize(LightDirection);		
		shading = dot(normal, -lightDirection);	
	}	

	float4 previous = SAMPLE_TEXTURE(PreviousShaderContents, PSIn.TexCoord);
	Output.Color = previous + shading;
	
	return Output;
}

PixelToFrame PointLightPixelShader(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	//sample normal from normal map
	float3 normal = SAMPLE_TEXTURE(NormalMap, PSIn.TexCoord).rgb;
	normal = normal*2.0f-1.0f;
	normal = normalize(normal);
	
	//sample depth from depth map
	float depth = SAMPLE_TEXTURE(DepthMap, PSIn.TexCoord).r;
	
	//create screen position
	float4 screenPos;
	screenPos.x = PSIn.TexCoord.x*2.0f-1.0f;
	screenPos.y = -(PSIn.TexCoord.y*2.0f-1.0f);
	screenPos.z = depth;
	screenPos.w = 1.0f;	
	
	//transform to 3D position
	float4 worldPos = mul(screenPos, ViewProjectionInv);
	worldPos /= worldPos.w;		
	
	//find screen position as seen by the light
	float4 lightScreenPos = mul(worldPos, LightViewProjection);
	lightScreenPos /= lightScreenPos.w;
	
	//find sample position in shadow map
	float2 lightSamplePos;
	lightSamplePos.x = lightScreenPos.x/2.0f+0.5f;
	lightSamplePos.y = (-lightScreenPos.y/2.0f+0.5f);
	
	//determine shadowing criteria
	float realDistanceToLight = lightScreenPos.z;	
	float distanceStoredInDepthMap = SAMPLE_TEXTURE(ShadowMap, lightSamplePos);
	bool shadowCondition =  distanceStoredInDepthMap <= realDistanceToLight - 1.0f/100.0f;	
	
	//calculate shading
	float shading = 0;
	if (!shadowCondition)
	{
		float3 lightDirection = worldPos - LightPosition;	
		float distance = length(lightDirection);
		
		lightDirection = normalize(lightDirection);
		
		shading = dot(normal, -lightDirection);	
		shading /= distance;
	}	

	float4 previous = SAMPLE_TEXTURE(PreviousShaderContents, PSIn.TexCoord);
	Output.Color = previous + shading;
	
	return Output;
}


PixelToFrame SpotLightPixelShader(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;

	//sample normal from normal map
	float3 normal = SAMPLE_TEXTURE(NormalMap, PSIn.TexCoord).rgb;
	normal = normal*2.0f - 1.0f;
	normal = normalize(normal);

	//sample depth from depth map
	float depth = SAMPLE_TEXTURE(DepthMap, PSIn.TexCoord).r;

	//create screen position
	float4 screenPos;
	screenPos.x = PSIn.TexCoord.x*2.0f - 1.0f;
	screenPos.y = -(PSIn.TexCoord.y*2.0f - 1.0f);
	screenPos.z = depth;
	screenPos.w = 1.0f;

	//transform to 3D position
	float4 worldPos = mul(screenPos, ViewProjectionInv);
		worldPos /= worldPos.w;

	//find screen position as seen by the light
	float4 lightScreenPos = mul(worldPos, LightViewProjection);
		lightScreenPos /= lightScreenPos.w;

	//find sample position in shadow map
	float2 lightSamplePos;
	lightSamplePos.x = lightScreenPos.x / 2.0f + 0.5f;
	lightSamplePos.y = (-lightScreenPos.y / 2.0f + 0.5f);

	//determine shadowing criteria
	float realDistanceToLight = lightScreenPos.z;
	float distanceStoredInDepthMap = SAMPLE_TEXTURE(ShadowMap, lightSamplePos);
	bool shadowCondition = distanceStoredInDepthMap <= realDistanceToLight - 1.0f / 100.0f;

	//determine cone criteria
	float3 lightDirection = normalize(worldPos - LightPosition);
    float coneDot = dot(lightDirection, normalize(ConeDirection));
	bool coneCondition = coneDot >= ConeAngle;

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

	float4 previous = SAMPLE_TEXTURE(PreviousShaderContents, PSIn.TexCoord);
	Output.Color = previous + shading;

	return Output;
}

// NOTE: The order of the techniques here are defined to match the indexing
//       in the C# Effect derived subclass that represents this effect.
TECHNIQUE( DeferredDirectedLight, MyVertexShader, DirectedLightPixelShader );
TECHNIQUE( DeferredPointLight,    MyVertexShader, PointLightPixelShader    );
TECHNIQUE( DeferredSpotLight,     MyVertexShader, SpotLightPixelShader     );
