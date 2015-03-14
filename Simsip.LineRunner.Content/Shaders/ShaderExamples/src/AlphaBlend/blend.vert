// This vertex shader passes transformed vertex co-ordinates and
// texture co-ordinates to the fragment shader.

/* Set the default floating point precision */
precision mediump float;

/* The co-ordinates vector - this can be set from the Shader
 * Attributes panel.
 */
attribute vec4 coord;

/* The texture co-ordinates for the vertex */
attribute vec2 texcoords;

/* The modelview projection matrix - this can be set from the Shader
 * Uniforms panel.
 */
uniform mat4 camera;

/* The interpolated texture coordinates for each fragment */
varying vec2 texcoord;

/* Vertex co-ordinates passed to the fragment shader */
varying vec3 v;

/* Vertex shader entry point */
void main()
{
	/* Assign the vertex's co-ordinates to the varyings */
	v.x = coord.x;
	v.y = coord.y;
	v.z = coord.z;
	
	/* Assign the texture co-ordinates to a varying */
	texcoord = texcoords;

	/* Apply the MVP matrix */
	gl_Position = camera * coord;
}
