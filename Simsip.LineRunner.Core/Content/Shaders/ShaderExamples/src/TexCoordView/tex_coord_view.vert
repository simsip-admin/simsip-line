// This vertex shader supplies texture co-ordinate and
// color data to the fragment shader, and applies the
// camera matrix.

/* Set the default floating point precision */
precision mediump float;

/* The vertex co-ordinates */
attribute vec4 coord;

/* The texture co-ordinates for this vertex */
attribute vec2 texCoords;

/* The modelview projection matrix */
uniform mat4 camera;

/* The texture co-ordinates passed to the fragment shader */   
varying vec2 TextureCoords;

/* Vertex shader entry point */
void main() 
{
	/* Assign the texture co-ordinates to varying for use
	 * by the fragment shader.
	 */
	TextureCoords = texCoords;
	
	/* Apply the MVP matrix */
	gl_Position = camera*coord;
}

