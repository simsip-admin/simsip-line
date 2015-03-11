// This fragment shader interpolates the normals across each fragment 
// and colors them according to their direction - red for the x-axis,
// green for the y-axis, blue for the z-axis.

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* The vertex coordiantes, varying across the fragment */
varying vec4 vertex_pos;

/* Fragment shader entry point */
void main(void)
{
	/* Assign the x, y and z components of the interpolated
	 * vertex coordinates to the red, green and blue components of the fragment 
	 * color
	 */
	gl_FragColor = vec4(abs(vertex_pos.x), abs(vertex_pos.y), abs(vertex_pos.z), 1.0);
}
