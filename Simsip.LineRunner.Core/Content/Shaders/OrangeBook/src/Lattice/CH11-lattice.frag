//
// Fragment shader for testing the discard command
//
// Author: Randi Rost
//
// Copyright (c) 2002-2006 3Dlabs Inc. Ltd. 
//
// See 3Dlabs-License.txt for license information
//

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

varying vec3  DiffuseColor;
varying vec3  SpecularColor;
varying vec2 TexCoord;

uniform vec2  Scale;
uniform vec2  Threshold;
uniform vec3  SurfaceColor;

void main()
{
    float ss = fract(TexCoord.x * Scale.x);
    float tt = fract(TexCoord.y * Scale.y);

    if ((ss > Threshold.s) && (tt > Threshold.t)) discard;

    vec3 finalColor = SurfaceColor * DiffuseColor + SpecularColor;
    gl_FragColor = vec4(finalColor, 1.0);
}