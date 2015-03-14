// This is a skeleton vertex shader. It creates a variable for the
// vertices passed to the fragment shader, and applies the camera matrix.

/* The co-ordinates vector - this can be set from the Shader
 * Attributes panel.
 */
attribute vec4 coord;

/* The modelview projection matrix - this can be set from the Shader
 * Uniforms panel.
 */
uniform mat4 camera;

/* Vertex co-ordinates passed to the fragment shader */
varying vec3 v;

/* Vertex shader entry point */
void main()
{
	/* Assign the vertex's co-ordinates to the varyings */
	v.x = coord.x;
	v.y = coord.y;
	v.z = coord.z;

	/* Apply the MVP matrix */
	gl_Position = camera * coord;
}
