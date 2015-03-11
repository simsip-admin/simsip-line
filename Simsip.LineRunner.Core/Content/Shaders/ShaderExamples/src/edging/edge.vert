// This vertex shader applies the camera matrix, and provides
// a vector of edge definitions to the fragment shader

/* Set the default floating point precision */
precision mediump float;

/* The vertex co-ordinates */
attribute vec4 coord;

/* A vector that indicates the next vertex in the edge.
 * This is a pre-calculated list, imported from the
 * Shader Attribute view.
 */
attribute vec3 edges;

/* The modelview projection matrix */
uniform mat4 camera;

/* The edge for this vertex, passed to the fragment shader */
varying vec3 edge;

/* Vertex shader entry point */
void main()
{
	/* Pass the edge to the fragment shader */
	edge = edges;
	
	/* Apply the MVP matrix */
	gl_Position = camera * coord;
}
