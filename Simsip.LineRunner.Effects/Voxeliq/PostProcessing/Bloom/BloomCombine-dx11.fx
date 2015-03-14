#include "BloomCombine-core.fx"

technique BloomCombine
{
    pass Pass1
    {
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
    }
}
