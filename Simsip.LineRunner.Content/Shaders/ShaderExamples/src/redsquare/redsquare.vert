// This vertex shader applies the MVP matrix.

/* The Vertex Shader iterates through the list of 'coord' values
 * running the program on every vertex of a model
 */

/* Set the default floating point precision */
precision mediump float;
 
/* The vertex co-ordinates */
attribute vec4 coord;

/* The modelview projection matrix */
uniform mat4 camera;

/* main function - the entry point for the vertex shader */
void main() 
{	
	/* This shader doesn't alter the vertex positions other than
	 * to apply the MVP matrix - the camera will
	 * scale, rotate and translate the model so that it can
	 * be seen in the preview window.
	 */
	gl_Position = camera * coord;
}

