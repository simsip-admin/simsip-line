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

uniform mat4 ModelViewMatrix;
uniform mat3 NormalMatrix;

attribute vec4 Vertex;
attribute vec3 Tangent;     // parallel to grating lines at each vertex
attribute vec3 Normal;

varying vec3 eyePosition;
varying vec3 T;
varying vec3  N;


void main()
{
    gl_Position = ModelViewMatrix * Vertex;
    
    eyePosition   = (gl_Position).xyz;

    // accumulate contributions from constructive interference
    // over several spectral orders.
    T = NormalMatrix * Tangent;

    // compute anisotropic highlight for zero-order (m = 0) reflection.
    N = NormalMatrix * Normal;
    

}
