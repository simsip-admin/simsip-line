#include "PerlinNoise-core.fx"

technique PerlinNoise
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 PerlinVertexShader();
		PixelShader = compile ps_4_0 PerlinPixelShader();
    }
}