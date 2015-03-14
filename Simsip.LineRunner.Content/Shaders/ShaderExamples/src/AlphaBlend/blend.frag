// This fragment shader uses an array of textures and different
// mixing modes to form a final texturing effect.

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* An array of 2D texture samplers */
uniform sampler2D imageArray[3];

/* Texture mixing control */
uniform float mixratio;

/* The blend mode applied to the textures 
 * Modes are:
 *  0 - mix
 *  1 - average
 *  2 - subtract
 *  3 - reverse subtract
 */
uniform int mode;

/* The varying position co-ordinates from the vertex shader */
varying vec3 v;

/* The varying texture co-ordinates from the vertex shader */
varying vec2 texcoord;

/* Fragment shader entry point */
void main()
{
	/* Obtain a color from the texture using the interpolated
	 * texture coordinates supplied by the vertex shader.
	 */
	vec4 base = texture2D(imageArray[0], texcoord);
	vec4 blend = texture2D(imageArray[1], texcoord);
	vec4 blend2 = texture2D(imageArray[2], texcoord);

	/* Switch based on the blend mode */
	if (mode == 0) {
		/* Mix the textures using the mix ratio uniform */
		blend = mix(base, blend, mixratio);
		blend = mix(blend, blend2, mixratio);
	} else if (mode == 1) {
	    /* Average the textures */
		blend = (blend + base) * 0.5;
	} else if (mode == 2) {
		/* Subtract the base from the blend texture */
		blend = (base - blend);
	} else if (mode == 3) {
		/* Subtract the blend from the base texture */
		blend = (blend - base);
	}
	
	/* Assign the blended pixel color to the RGB color of the fragment. */
	gl_FragColor = blend;
}
