#include "Deferred3Final-core.fx"

technique CombineColorAndShading
{
	pass Pass0
    {  
    	VertexShader = compile vs_3_0 CombineVertexShader();
        PixelShader  = compile ps_3_0 CombinePixelShader();
    }
}