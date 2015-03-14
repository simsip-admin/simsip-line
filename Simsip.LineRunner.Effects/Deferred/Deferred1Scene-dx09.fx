#include "Deferred1Scene-core.fx"

technique MultipleTargets
{
	pass Pass0
    {  
    	VertexShader = compile vs_3_0 MultipleTargetsVertexShader();
        PixelShader  = compile ps_3_0 MultipleTargetsPixelShader();
    }
}
