//
// Fragment shader for Ward BRDF reflection
//
// Author: Randi Rost
//
// Copyright (c) 2003-2006: 3Dlabs, Inc.
//
// See 3Dlabs-License.txt for license information
//

/*Values of P and A from Gregory J. Ward's paper: 
"Measuring and Modeling Anisotropic Reflection"

	Material						 Px   Py   Ax    Ay	
rolled brass					:	.10  .33  .050  .16
rolled aluminium				:	.1   .21  .04   .09
lightly brushed aluminium		:	.15  .19  .088  .13
varnished plywood				:	.33  .025 .04   .11
enamel finished metal			:	.25  .047 .080  .096
painted cardboard box			:	.19  .043 .076  .085
white ceramic tile				:	.70  .050 .071  .071
glossy graey paper				:	.29  .083 .082  .082
ivor computer plastic			:	.45  .043 .13   .13
plastic laminate				:	.67  .070 .092  .092

*/

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

const float PI = 3.14159;
const float ONE_OVER_PI = 1.0 / PI;

uniform vec4 SurfaceColor; // Base color of surface
uniform vec2 P;            // Diffuse (x) and specular reflectance (y)
uniform vec2 A;            // Slope distribution in x and y
uniform vec3 Scale;        // Scale factors for intensity computation

varying vec3 N, L, H, R, T, B;

void main()
{
    float e1, e2, E, cosThetaI, cosThetaR, brdf, intensity;   
     
    e1 = dot(H, T) / A.x;
    e2 = dot(H, B) / A.y;
    E = -2.0 * ((e1 * e1 + e2 * e2) / (1.0 + dot(H, N)));
    
    cosThetaI = dot(N, L);
    cosThetaR = dot(N, R);
    
    brdf = P.x * ONE_OVER_PI + 
           P.y * (1.0 / sqrt(cosThetaI * cosThetaR)) *
           (1.0 / (4.0 * PI * A.x * A.y)) * exp(E);
                
    intensity = Scale[0] * P.x * ONE_OVER_PI +
                Scale[1] * P.y * cosThetaI * brdf + 
                Scale[2] * dot(H, N) * P.y;
          
  // color = mix (color, intensity * SurfaceColor.rgb, 0.2);
    vec4 color = intensity * SurfaceColor;
  
    gl_FragColor = color;
}