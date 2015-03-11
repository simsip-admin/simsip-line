#include "SkyDome-core.fx" 

 technique SkyDome
 {
     pass Pass0
     {
		 VertexShader = compile vs_4_0_level_9_1 SkyDomeVertexShader();
		 PixelShader = compile ps_4_0_level_9_1 SkyDomePixelShader();
     }
 }

 technique SkyStarDome
 {
     pass Pass0
     {
		 VertexShader = compile vs_4_0_level_9_1 SkyStarDomeVertexShader();
		 PixelShader = compile ps_4_0_level_9_1 SkyStarDomePixelShader();
     }
 }