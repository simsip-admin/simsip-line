// This fragment shader paints all objects red.

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* The Fragment shader is run many times for a single polygon,
 * This means for a single triangle, the value of a 'varying'
 * is interpolated from the three values generated by the 
 * runs of the vertex shader on each of the vertices.
 */

/* main function - the entry point for the fragment shader */
void main(void)
{
	/* We write a vec4 into gl_FragColor to indicate the colour 
	 * that OpenGL should use for this fragment.
	 */
    gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}
