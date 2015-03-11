// This vertex shader supplies texture co-ordinate and
// color data to the fragment shader, and applies the
// camera matrix.

/* Set the default floating point precision */
precision mediump float;

/* The vertex co-ordinates */
attribute vec4 coord;

/* The texture co-ordinates for this vertex */
attribute vec2 texCoords;

/* The base color of the object */
attribute vec4 colors;

/* The modelview projection matrix */
uniform mat4 camera;

/* The texture co-ordinates passed to the fragment shader */   
varying float x;
varying float y;


varying vec4 color;

/* Vertex shader entry point */
void main() 
{
	/* Assign the texture co-ordinates to varyings for use
	 * by the fragment shader.
	 */
	x = texCoords.x;
	y = texCoords.y;
	
	/* Assign the color to a varying for the fragment shader */
	color = colors;
	
	/* Apply the MVP matrix */
	gl_Position = camera*coord;
}

