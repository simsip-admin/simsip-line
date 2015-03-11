// This is a fragment shader to render the fog effect on the supplied geometry.
// It calculates the fog factor for each fragment using linear or exponential computations. 
// Fog effect blends a fog_color with each rasterized fragment's post-texturing color using 
// a blending factor fogfactor. 


/* Use of the precision keyword is a GLES requirement */
precision mediump float;

varying vec4 eyePos;

/* Uniform variables can be set from the Shader
 * Uniforms panel.
 * bool variables to indicate the use of linear and exponential computations
 * while calculating fog factor
 */
uniform bool UseLinear;
uniform bool UseEXP;
uniform bool UseEXP2;

/* Fog density, a value between 0.0 to 1.0 */
uniform float density;

/* Color of the fog */
uniform vec3 fog_color;

/* Global variables
*/
float fogfactor;
float fog_end = 1.0;
float fog_start = 0.0;

/* Fragment shader entry point */
void main()
{
float FogFragCoord;
/* Assign the Green color to all the fragments */
gl_FragColor = vec4(0.0, 1.0, 0.0, 1.0);
vec3 color = vec3 (gl_FragColor);
FogFragCoord = abs (eyePos.z);
	
// fogfactor is computed in one of three ways, depending on the fog mode.
// Only one of the three modes should be enabled at a time to see the desired fog effect.
if (UseLinear) 
	fogfactor = (fog_end - FogFragCoord)/(fog_end -fog_start);
if (UseEXP)
	fogfactor = exp (-density*FogFragCoord);
if (UseEXP2)
	fogfactor = exp (-density*density*FogFragCoord*FogFragCoord);

fogfactor = clamp (fogfactor, 0.0, 1.0);

/* Blending of fragment color with the fog color */
color = vec3 ((fogfactor * fog_color) + ((1.0 - fogfactor) * color));

/* Final fragment color */
gl_FragColor = vec4 ( color.xyz, 1.0);
}
