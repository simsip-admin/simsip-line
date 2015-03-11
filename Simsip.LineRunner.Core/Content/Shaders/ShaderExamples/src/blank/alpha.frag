// This is the simplest fragment shader. It paints every
// fragment white.

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* Fragment shader entry point */
void main()
{
	/* Assign white to the fragment */
	gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0);
}
