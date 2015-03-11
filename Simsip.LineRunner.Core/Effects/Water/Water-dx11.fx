#include "Water-core.fx"

technique Water
{
    pass Pass0
    {
		VertexShader = compile vs_4_0_level_9_1 WaterVS();
		PixelShader = compile ps_4_0_level_9_1 WaterPS();
    }
}
