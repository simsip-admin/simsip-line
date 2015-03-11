// This is a skeleton fragment shader. It applies a color to
// each fragment based on its position in 3D space

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* The varying position co-ordinates from the vertex shader */
varying vec3 v;

/* Fragment shader entry point */
void main()
{
	/* Assign the 3D position to the RGB color of the fragment */
	gl_FragColor = vec4((v.x+1.0)/2.0, (v.y+1.0)/2.0, (v.z+1.0)/2.0, 1.0);
}
