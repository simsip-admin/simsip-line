#include "GaussianBlur-core.fx"

technique GaussianBlur
{
    pass Pass1
    {
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
    }
}
