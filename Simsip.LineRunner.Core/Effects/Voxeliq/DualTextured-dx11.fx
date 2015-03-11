#include "DualTextured-core.fx"

technique BlockTechnique
{
    pass Pass1
    {
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
    }
}
