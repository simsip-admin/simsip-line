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
const float GratingSpacing = 900.0;
const float SurfaceRoughness = 0.15;
const vec3 HighlightColor = vec3 (1.0, 0.82, 0.53);

uniform vec3 lightPosition;
uniform mat4 ViewMatrix;
uniform mat4 ModelViewMatrix;
uniform mat3 NormalMatrix;

attribute vec4 Vertex;
attribute vec3 Tangent;     // parallel to grating lines at each vertex
attribute vec3 Normal;

varying vec4 FrontColor;

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
    // extract positions from input uniforms
    //vec3 eyePosition   = -osg_ViewMatrix[3].xyz / osg_ViewMatrix[3].w;
    vec3 eyePosition   = (ModelViewMatrix * Vertex).xyz;

    // H = halfway vector between light and viewer from vertex
    vec3 P = eyePosition;
    vec3 L = normalize(lightPosition - P);
    vec3 V = normalize(eyePosition - P);
    vec3 H = L + V;

    // accumulate contributions from constructive interference
    // over several spectral orders.
    vec3  T = NormalMatrix * Tangent;
    float u = abs(dot(T, H));
    vec3 diffColor = vec3(0.0);
    const int numSpectralOrders = 3; /*Corresponding to RGB spectral orders of light*/
    for (int m = 1; m <= numSpectralOrders; ++m)
    {
        float lambda = GratingSpacing * u / float(m);
        diffColor += lambda2rgb(lambda);
    }

    // compute anisotropic highlight for zero-order (m = 0) reflection.
    vec3  N = NormalMatrix * Normal;
    float w = dot(N, H);
    float e = SurfaceRoughness * u / w;
    vec3 hilight = exp(-e * e) * HighlightColor;

    // write the values required for fixed function fragment processing
    const float diffAtten = 0.8; // attenuation of the diffraction color
    FrontColor = vec4(diffAtten * diffColor + hilight, 1.0);
    
    gl_Position = ModelViewMatrix * Vertex;
}
