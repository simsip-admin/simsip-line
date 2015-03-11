// This fragment shader combines the object color and texture
// value to color the fragment.

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* The texture image */
uniform sampler2D texture;

/* The texture co-ordinates from the vertex shader */
varying float x;
varying float y;

/* The color of the object from the vertex shader */
varying vec4 color;

/* Fragment shader entry point */
void main(void)
{
	/* Combine the object color and texture value for the fragment */
    //vec4 composite = color + texture2D(texture,vec2(x,y));
    vec4 composite = texture2D(texture, vec2(x, y));
    
    /* Assign the resulting color to the fragment */
    gl_FragColor = composite;
}
