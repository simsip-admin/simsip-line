// This is the simplest vertex shader. It declares some
// co-ordinates, and camera, and applies the camera
// transformation.

/* Set the default floating point precision */
precision mediump float;

/* The co-ordinates vector - this can be set from the
 * Shader Attributes panel.
 */
attribute vec4 coord;

/* The modelview projection matrix - this can be set from the 
 * Shader Uniforms panel.
 */
uniform mat4 camera;

/* Vertex shader entry point */
void main()
{
	/* Apply the MVP matrix, transforming the geometries
	 * vertices according to the camera's view.
	 */
	gl_Position = camera * coord;
}
