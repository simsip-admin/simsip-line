#include "Deferred1Scene-core.fx"

technique MultipleTargets
{
	pass Pass0
    {  
    	VertexShader = compile vs_3_0 MultipleTargetsVertexShader();
        PixelShader  = compile ps_3_0 MultipleTargetsPixelShader();
    }
}

technique SingleTargetColor
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 MultipleTargetsVertexShader();
		PixelShader = compile ps_3_0 SingleTargetColorPixelShader();
	}
}

technique SingleTargetNormal
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 MultipleTargetsVertexShader();
		PixelShader = compile ps_3_0 SingleTargetNormalPixelShader();
	}
}

technique SingleTargetDepth
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 MultipleTargetsVertexShader();
		PixelShader = compile ps_3_0 SingleTargetDepthPixelShader();
	}
}
