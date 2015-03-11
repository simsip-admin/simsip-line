#include "PerlinNoise-core.fx"

technique PerlinNoise
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 PerlinVertexShader();
        PixelShader = compile ps_3_0 PerlinPixelShader();
    }
}