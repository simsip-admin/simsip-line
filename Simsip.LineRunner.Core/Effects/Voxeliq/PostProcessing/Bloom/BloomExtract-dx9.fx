#include "BloomExtract-core.fx"

technique BloomExtract
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
