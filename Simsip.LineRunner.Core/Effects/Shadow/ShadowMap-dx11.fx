#include "ShadowMap-core.fx"

technique ShadowMap
{
	pass Pass0
    {  
		VertexShader = compile vs_4_0_level_9_1 ShadowMapVertexShader();
		PixelShader = compile ps_4_0_level_9_1 ShadowMapPixelShader();
    }
}
