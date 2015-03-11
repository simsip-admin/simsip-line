#include "SkyDome-core.fx" 

 technique SkyDome
 {
     pass Pass0
     {
         VertexShader = compile vs_3_0 SkyDomeVertexShader();
         PixelShader = compile ps_3_0 SkyDomePixelShader();
     }
 }

 technique SkyStarDome
 {
     pass Pass0
     {
         VertexShader = compile vs_3_0 SkyStarDomeVertexShader();
         PixelShader = compile ps_3_0 SkyStarDomePixelShader();
     }
 }