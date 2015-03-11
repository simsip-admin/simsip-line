#include "Deferred3Final-core.fx"

technique CombineColorAndShading
{
	pass Pass0
    {  
		VertexShader = compile vs_4_0_level_9_1 CombineVertexShader();
		PixelShader = compile ps_4_0_level_9_1 CombinePixelShader();
    }
}