// This fragment shader calculates the reflection from the
// environment for each fragment, and combines it with lighting 
// and base object color. Type of environment mapping implemented 
// here uses equirectangular texture map.

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* Constants for computing the altitude and azimuth angles */
const vec3 XUnitVec = vec3(1.0, 0.0, 0.0);
const vec3 YUnitVec = vec3(0.0, -1.0, 0.0);

/* Environment map texture */
uniform sampler2D envMap;

/* Base color of the object */
uniform vec3 surfaceColour;

/* Mix ratio between reflective and base color components */
uniform float mixRatio;

/* Ambient light level */
uniform float ambient;

/* Normal to the surface */
varying vec3 normal;

/* Intensity of diffuse lighting */
varying float lightIntensity;

/* Direction of eye */
varying vec3 eyeDir;

/* Fragment shader entry point */
void main(void)
{
	/* Calculate the reflection vector */
	vec3 reflectDir = reflect(eyeDir, normal);	
	vec2 index;
	
	/* Calculate the altitude and azimuth angles */
	index.t = dot(normalize(reflectDir), YUnitVec);
	reflectDir.y = 0.0;
	index.s = dot(normalize(reflectDir), XUnitVec) * 0.5;

	/* Test whether reflection is towards the front or back */	
	if (reflectDir.z >= 0.0)
	{
		/* Towards the front - index the texture from 0.25 to 0.75 */
		index = (index + 1.0) * 0.5;
		index.t = 1.0 - index.t;
	}
	else
	{
		/* Towards the back - index the texture from 0.75 to 1.25
		 * using the GL_REPEAT texturing mode to map 1.0 - 1.25 to
		 * 0 - 0.25.
		 */
		index.t = 1.0 - (index.t + 1.0) * 0.5;
		index.s = (-index.s) * 0.5 + 1.0;
	}
	
	/* Look up a color from the environment map based on the
	 * index vector.
	 */
	vec3 envColour = vec3(texture2D(envMap, index));
	 
	/* Light the resulting surface */ 
	vec3 baseCoat = lightIntensity * surfaceColour + ambient * surfaceColour;

	/* Mix the colouring from the environment map with the base
	 * surface color.
	 */	
	envColour = mix(envColour, baseCoat, mixRatio);
		
	/* Assign the computed color to the fragment */
    gl_FragColor = vec4(envColour, 0.5);
}
