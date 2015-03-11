// This vertex shader applies the camera matrix, and creates
// a varying set of co-ordinates for the fragment shader

/* Set the default floating point precision */
precision mediump float;

/* The vertex co-ordinates */
attribute vec4 coord;

/* The modelview projection matrix */
uniform mat4 camera;

/* Varying co-ordinates for the fragment shader */
varying vec3 v;

/* Vertex shader entry point */
void main() 
{
	/* Assign the vertex co-ordinates to the varying */
	v.x = coord.x;
	v.y = coord.y;
	v.z = coord.z;
	
	/* Apply the MVP matrix */
	gl_Position = camera * coord;
}

