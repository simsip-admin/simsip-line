//
// Fragment shader for drawing procedural stripes
//
// Author: OGLSL implementation by Ian Nurse
//
// Copyright (C) 2002-2006  LightWork Design Ltd.
//          www.lightworkdesign.com
//
// See LightworkDesign-License.txt for license information
//

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

uniform vec3  StripeColor;
uniform vec3  BackColor;
uniform float Width;
uniform float Fuzz;
uniform float Scale;

varying vec3  DiffuseColor;
varying vec3  SpecularColor;
varying vec2 TexCoord;

void main()
{
    float scaled_t = fract(TexCoord.y * Scale);

    float frac1 = clamp(scaled_t / Fuzz, 0.0, 1.0);
    float frac2 = clamp((scaled_t - Width) / Fuzz, 0.0, 1.0);

    frac1 = frac1 * (1.0 - frac2);
    frac1 = frac1 * frac1 * (3.0 - (2.0 * frac1));
  
    vec3 finalColor = mix(BackColor, StripeColor, frac2);
    finalColor = finalColor * DiffuseColor + SpecularColor;

    gl_FragColor = vec4(finalColor, 1.0);
}