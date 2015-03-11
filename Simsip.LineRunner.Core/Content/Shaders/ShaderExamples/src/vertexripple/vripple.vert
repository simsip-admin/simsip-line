// This vertex shader creates a wavy effect. It's most effective
// for the Lattice Plane object.

/* Set the default floating point precision */
precision mediump float;

/* The vertex co-ordinates */
attribute vec4 coord;

/* The texture co-ordinate for the vertex */
attribute vec2 texcoords;

/* The modelview projection matrix */
uniform mat4 camera;

/* Matrix for transforming normals into camera space */
uniform mat3 normalMatrix;

/* Wave parameters */
uniform float t1;
uniform float t2;

/* The vertices of the wavy plane */
varying vec3 v;

/* The normals for the wavy plane */
varying vec4 normal;

void main() 
{
	/* Assign the texture co-ordinates and z position to our 
	 * new vertices
	 */
	v.x = texcoords.x;
	v.y = texcoords.y;
	v.z = coord.z;
	
	/* Create a copy of the vertex, so that we can apply our
	 * effect to it.
	 */
	vec4 altcoord = coord;

	/* Calculate the wave applied in the z-axis depending on x
	 * and y position in the plane.
	 */	
	altcoord.z = (sin(v.y * 20.0 + t1) + sin(v.x * 20.0 + t2)) + v.z;
	
	/* Calculate the normals for the now wavy plane */
	normal.x = (cos(v.y * 20.0 + t1) + cos(v.x * 20.0 + t2));
	normal.y = (cos(v.y * 20.0 + t1) + cos(v.x * 20.0 + t2));
	normal.z = 1.0;
	normal.w = 1.0;
	normal = normalize(normal);
	
	/* Scale the wave along the x-axis of the plane */
	altcoord.z *= v.x * 0.08;

	/* Apply the MVP matrix to the shifted co-ordinates */
	gl_Position = camera * altcoord;
}

