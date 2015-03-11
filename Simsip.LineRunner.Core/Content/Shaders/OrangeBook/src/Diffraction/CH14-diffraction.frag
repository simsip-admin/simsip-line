//
// Vertex shader for diffraction effect
//
// Author: Mike Weiblen
//         based on a shader by Jos Stam
//
// Copyright (c) 2003-2006: 3Dlabs, Inc.
//
// See 3Dlabs-License.txt for license information
//

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* The varying FrontColor from the vertex shader */
varying vec4 FrontColor;

/* Fragment shader entry point */
void main()
{
	/* Assign the FrontColor to FragColor */
	gl_FragColor = FrontColor;
}