//  This is a vertex shader program. It and applies the camera matrix to
//  the supplied vertex coordinates.

/* The co-ordinates vector - this can be set from the Shader
 * Attributes panel.
 */
attribute vec4 coord;

/* The modelview projection matrix - this can be set from the Shader
 * Uniforms panel.
 */
uniform mat4 camera;

/* Vertex co-ordinates passed to the fragment shader */
varying vec4 eyePos;

/* Vertex shader entry point */
void main()
{
	eyePos = camera * coord;
	/* Apply the camera matrix */
	gl_Position = eyePos;
}
