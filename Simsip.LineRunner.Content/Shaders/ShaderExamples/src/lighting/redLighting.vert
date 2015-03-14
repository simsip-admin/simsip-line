// This vertex shader declares a normal vector, and applies 
// the camera matrix. 

/* Set the default floating point precision */
precision mediump float;

/* The vertex co-ordinates */
attribute vec4 coord;

/* The normal for each vertex */
attribute vec4 normals;

/* The modelview projection matrix */
uniform mat4 camera;

/* A varying normal declared for use by the fragment shader */
varying vec4 normal;

/* Vertex shader entry point */
void main() 
{	
	/* Assign the vertex's normal to the varying */
	normal = normals;
	
	/* Apply the MVP matrix */
	gl_Position = camera * coord;
}

