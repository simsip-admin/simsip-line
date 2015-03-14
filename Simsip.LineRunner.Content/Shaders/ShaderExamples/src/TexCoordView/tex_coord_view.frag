// This fragment shader combines the object color and texture
// value to color the fragment.

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* The texture co-ordinates from the vertex shader */
varying vec2 TextureCoords;

/* Fragment shader entry point */
void main(void)
{ 
    /* Assign the the value of texture coordinates (x, y) to fragment color */
    gl_FragColor = vec4 ( TextureCoords, 0 ,1);
}
