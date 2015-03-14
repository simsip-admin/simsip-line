#include "ShadowMap-core.fx"

technique ShadowMap
{
	pass Pass0
    {  
    	VertexShader = compile vs_3_0 ShadowMapVertexShader();
        PixelShader  = compile ps_3_0 ShadowMapPixelShader();
    }
}
