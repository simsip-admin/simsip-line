//
// Vertex shader for Ward BRDF reflection
//
// Author: Randi Rost
//
// Copyright (c) 2003-2006: 3Dlabs, Inc.
//
// See 3Dlabs-License.txt for license information
//

attribute vec4 Vertex;
attribute vec3 Normal;
attribute vec3 Tangent;

uniform vec3 LightDir;  // Light direction in eye coordinates
uniform vec4 ViewPosition;
uniform mat4 ModelViewMatrix;
uniform mat3 NormalMatrix;

varying vec3 N, L, H, R, T, B;

void main() 
{
    vec3 Binormal = cross(Tangent, Normal);
    vec3 V, eyeDir;
    vec4 pos;
       
    pos    = ModelViewMatrix * Vertex;
    eyeDir = pos.xyz;
    
    N = normalize(NormalMatrix * Normal);
    L = normalize(LightDir);
    V = normalize((ModelViewMatrix * ViewPosition).xyz - pos.xyz);
    H = normalize(L + V);
    R = normalize(reflect(eyeDir, N));
    T = normalize(NormalMatrix * Tangent);
    B = normalize(NormalMatrix * Binormal);
    
    gl_Position = pos;
}