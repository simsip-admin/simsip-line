#include "Deferred2Lights-core.fx"

technique DeferredDirectedLight
{
	pass Pass0
    {  
    	VertexShader = compile vs_3_0 MyVertexShader();
        PixelShader  = compile ps_3_0 DirectedLightPixelShader();
    }
}

technique DeferredPointLight
{
	pass Pass0
    {  
    	VertexShader = compile vs_3_0 MyVertexShader();
        PixelShader  = compile ps_3_0 PointLightPixelShader();
    }
}

technique DeferredSpotLight
{
	pass Pass0
    {  
    	VertexShader = compile vs_3_0 MyVertexShader();
        PixelShader  = compile ps_3_0 SpotLightPixelShader();
    }
}

