#include "GaussianBlur-core.fx"

technique GaussianBlur
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
