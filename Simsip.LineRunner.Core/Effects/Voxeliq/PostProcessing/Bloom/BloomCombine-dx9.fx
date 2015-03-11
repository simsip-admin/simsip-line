#include "BloomCombine-core.fx"

technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
