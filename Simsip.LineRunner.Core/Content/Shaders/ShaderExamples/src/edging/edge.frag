// This fragment shader simulates a wireframe display
// by only painting fragments that are close to the edges
// of each polygon

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* The interpolated edge indicator from the vertex shader.
 * After interpolation, one component will be equal to 1.0
 * at the edge of the polygon between the current and next 
 * vertex.
 */
varying vec3 edge;

/* Fragment shader entry point */
void main()
{
	/* If one of the interpolated edge values is close to 1.0,
	 * the fragment must be close to the edge of the polygon.
	 */
	if ((edge.x>0.97) || (edge.y>0.97) || (edge.z>0.97)) {
		/* Set the color to the interpolated edge's position 
		 * This causes the first edge to be red, the second
		 * green and the third blue.
		 */
		gl_FragColor = vec4(edge.x, edge.y, edge.z, 0.0);
	} else {
		/* Make the fragment transparent */
		discard;
	} 
}
