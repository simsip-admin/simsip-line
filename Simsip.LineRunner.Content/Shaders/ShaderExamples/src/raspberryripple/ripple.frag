// This fragment shader creates a three dimensional red and
// white ripple effect.

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* Variables holding the rate at which ripples oscillate in
 * the three dimensions. These can be assigned to through
 * the Shader Uniforms view, and modulated for animation.
 */
uniform float xRippleRate;
uniform float yRippleRate;
uniform float zRippleRate;

/* Interpolated values of x, y and z across the fragment */
varying vec3 v;

/* Fragment shader entry point */
void main(void)
{
    /* Use sin built-in function to modulate the green channel according
     * to position and ripple rate
     */
	float green = abs(sin(v.x*xRippleRate+sin(v.y*yRippleRate+sin(v.z*zRippleRate))));
	
	/* Do the same for the blue channel */
	float blue = green;
	
	/* Assign the computed color to the fragment */
	gl_FragColor = vec4(1.0, green, blue, 1.0);
}
