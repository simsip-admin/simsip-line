// This vertex shader applies the modelview projection matrix
// and passes interpolated texture coordinates to the fragment
// shader.

/* Set the default floating point precision */
precision mediump float;

/* The modelview projection matrix */
uniform mat4 camera;

/* The vertex co-ordinates */
attribute vec4 coord;

/* The texture co-ordinates for the vertex */
attribute vec2 texCoords;

/* Interpolated texture co-ordinates passed to the fragment shader */
varying vec2 Position;

/* Vertex shader entry point */
void main()
{
	/* Assign the texture co-ordinates to the position, adjusting
	 * the values to be suitable for drawing a fractal.
	 */
	Position = vec2(texCoords - 0.5) * 5.0;
	
	/* Apply the modelview projection matrix */
	gl_Position = camera * coord;
}


