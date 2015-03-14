#include "Deferred2Lights-core.fx"

technique DeferredDirectedLight
{
	pass Pass0
    {  
		VertexShader = compile vs_4_0_level_9_1 MyVertexShader();
		PixelShader = compile ps_4_0_level_9_1 DirectedLightPixelShader();
    }
}

technique DeferredPointLight
{
	pass Pass0
    {  
		VertexShader = compile vs_4_0_level_9_1 MyVertexShader();
		PixelShader = compile ps_4_0_level_9_1 PointLightPixelShader();
    }
}

technique DeferredSpotLight
{
	pass Pass0
    {  
		VertexShader = compile vs_4_0_level_9_1 MyVertexShader();
		PixelShader = compile ps_4_0_level_9_1 SpotLightPixelShader();
    }
}
