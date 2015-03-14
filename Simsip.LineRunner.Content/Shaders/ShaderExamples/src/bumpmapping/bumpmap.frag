// This fragment shader applies lighting effects, modulated
// by a bump mapping effect

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* Base color of the fragment */
uniform vec3 surfaceColour;

/* Spacing of the bumps */
uniform float bumpDensity;

/* Size of bumps */
uniform float bumpSize;

/* Intensity of the specular highlight */
uniform float specularFactor;

/* Texture co-ordinates */
varying float x;
varying float y;

/* Light vector in tangent space */
varying vec3 lightDir;

/* Eye vector in tangent space */
varying vec3 eyeDir;

/* Fragment shader entry point */
void main(void)
{
	vec3 litColour;
	
	/* Scale the bump texture according to density */
	vec2 c = bumpDensity * vec2(x, y);
	
	/* Calculate the perturbation vector as the distance
	 * center of the bump.
	 */
	vec2 p = fract(c) - vec2(0.5);
	
	float d, f;
	
	d = p.x * p.x + p.y * p.y;
	
	/* Calculate the normalization factor for the 
	 * perturbation normal.
	 */
	f = 1.0 / sqrt(d + 1.0);
	
	/* Test if the fragment is in a bump, and if not, flatten
	 * the surface.
	 */
	if (d >= bumpSize) {
		p = vec2(0.0);
		f = 1.0;
	}

	/* Normalize the perturbation vector */
	vec3 normalDelta = vec3(p.x, p.y, 1.0) * f;
	
	/* Compute the specular and diffuse lighting components using the
	 * perturbed "bumpy" normals.
	 */
	litColour = surfaceColour * max(dot(normalDelta, lightDir), 0.0);
	vec3 reflectDir = reflect(lightDir, normalDelta);
	float spec = max(dot(eyeDir, reflectDir), 0.0);
	spec = pow(spec, 6.0);
	spec *= specularFactor;
	litColour = min(litColour + spec, vec3(1.0));
	
	/* Assign the lit surface color to the fragment */
    gl_FragColor = vec4(litColour, 1.0);
}
