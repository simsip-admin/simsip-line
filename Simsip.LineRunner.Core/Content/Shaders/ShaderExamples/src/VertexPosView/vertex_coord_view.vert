// This vertex shader declares a vertex_pos vector, and applies 
// the camera matrix. 

/* Set the default floating point precision */
precision mediump float;

/* The vertex co-ordinates */
attribute vec4 coord;

/* A varying vertex_pos declared for use by the fragment shader */
varying vec4 vertex_pos;

/* The modelview projection matrix */
uniform mat4 camera;

/* Vertex shader entry point */
void main() 
{
	/* Apply the MVP matrix */
	gl_Position = camera * coord;
	
	/* Assign  gl_Position to the varying vertex_pos */
	vertex_pos = gl_Position ;

}

