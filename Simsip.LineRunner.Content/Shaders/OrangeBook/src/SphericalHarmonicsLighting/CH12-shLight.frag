//
// Fragment shader for spherical harmonics lighting
//
// Author: Randi Rost
//
// Copyright (C) 2005 3Dlabs, Inc.
//
// See 3Dlabs-License.txt for license information
//

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

varying vec3  DiffuseColor;

void main(void)
{
    gl_FragColor = vec4(DiffuseColor, 1.0);
}