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

varying vec3 eyePosition;
varying vec3 T;
varying vec3  N;

const float GratingSpacing = 900.0;
const float SurfaceRoughness = 0.15;
const vec3 HighlightColor = vec3 (1.0, 0.88, 0.78);

uniform vec3 lightPosition;
uniform mat4 ViewMatrix;
uniform mat4 ModelViewMatrix;
uniform mat3 NormalMatrix;


// map a visible wavelength [nm] to OpenGL's RGB representation

vec3 lambda2rgb(float lambda)
{
    const float ultraviolet = 400.0;
    const float infrared    = 700.0;

    // map visible wavelength range to 0.0 -> 1.0
    float a = (lambda-ultraviolet) / (infrared-ultraviolet);

    // bump function for a quick/simple rainbow map
    const float C = 7.0;        // controls width of bump
    vec3 b = vec3(a) - vec3(0.75, 0.5, 0.25);
    return max((1.0 - C * b * b), 0.0);
}

void main()
{

    // H = halfway vector between light and viewer from vertex
    vec3 P = eyePosition;
    vec3 L = normalize(lightPosition - P);
    vec3 V = normalize(eyePosition - P);
    vec3 H = L + V;

    // accumulate contributions from constructive interference
    // over several spectral orders.
    float u = abs(dot(T, H));
    vec3 diffColor = vec3(0.0);
    const int numSpectralOrders = 3; /*Corresponding to RGB spectral orders of light*/
    for (int m = 1; m <= numSpectralOrders; ++m)
    {
        float lambda = GratingSpacing * u / float(m);
        diffColor += lambda2rgb(lambda);
    }

    // compute anisotropic highlight for zero-order (m = 0) reflection.
    float w = dot(N, H);
    float e = SurfaceRoughness * u / w;
    vec3 hilight = exp(-e * e) * HighlightColor;

    // write the values required for fixed function fragment processing
    const float diffAtten = 0.8; // attenuation of the diffraction color
    gl_FragColor = vec4(diffAtten * diffColor + hilight, 1.0);

}
