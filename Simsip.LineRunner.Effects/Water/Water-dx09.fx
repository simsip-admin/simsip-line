#include "Water-core.fx"

technique Water
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 WaterVS();
        PixelShader = compile ps_3_0 WaterPS();
    }
}
