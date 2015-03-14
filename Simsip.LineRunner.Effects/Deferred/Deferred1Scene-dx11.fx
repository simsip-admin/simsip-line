#include "Deferred1Scene-core.fx"

technique MultipleTargets
{
	pass Pass0
    {  
		VertexShader = compile vs_4_0_level_9_1 MultipleTargetsVertexShader();
		PixelShader = compile ps_4_0_level_9_1 MultipleTargetsPixelShader();
    }
}
