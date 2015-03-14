
/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* Parameters for the fractal. Edit these in the shader uniforms
 * view.
 */
uniform float MaxIterations;
uniform float Zoom;
uniform float Xcenter;
uniform float Ycenter;
uniform vec3 InnerColor;
uniform vec3 OuterColor1;
uniform vec3 OuterColor2;

/* Real/imaginary interpolated fractal co-ordinates, based on
 * texture co-ordinates.
 */
varying vec2 Position;

/* Fragment shader entry point */
void main()
{
	/* Set up the initial values of Z and C */
	float Zreal = Position.x * Zoom + Xcenter;
	float Zimag = Position.y * Zoom + Ycenter;
	float Creal = Zreal;
	float Cimag = Zimag;
	
	float r2 = 0.0;
	float iter;
	vec3 color;

	/* Iterate on the Mandelbrot fractal, using the formula 
	 *	Z = Z * Z + C, where Z, C are complex.
	 */ 
	for (iter = 0.0; iter < MaxIterations && r2 < 4.0; ++iter)
	{
		float tempZreal = Zreal;
		
		Zreal = (tempZreal * tempZreal) - (Zimag * Zimag) + Creal;
		Zimag = 2.0 * tempZreal * Zimag + Cimag;
		r2 = (Zreal * Zreal) + (Zimag * Zimag);
	}
	
	if (r2 < 4.0)
		/* Assign the inner colour to the fragment if the number of iterations
		 * is exceeded.
		 */
		color = InnerColor;
	else
		/* Otherwise, assign a value derived from the number of iterations */
		color = mix(OuterColor1, OuterColor2, fract(iter * 0.055));

	/* Assign the computed colour to the fragment */
	gl_FragColor = vec4(color, 1.0);
}
