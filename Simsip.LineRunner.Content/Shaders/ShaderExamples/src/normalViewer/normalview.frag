// This fragment shader interpolates the normals across each fragment 
// and colors them according to their direction - red for the x-axis,
// green for the y-axis, blue for the z-axis.

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* The normal, varying across the fragment */
varying vec4 normal;

/* Fragment shader entry point */
void main(void)
{
	/* Assign the x, y and z components of the interpolated
	 * normal to the red, green and blue components of the fragment 
	 * color
	 */
	gl_FragColor = vec4(abs(normal.x), abs(normal.y), abs(normal.z), abs(normal.w));
}
