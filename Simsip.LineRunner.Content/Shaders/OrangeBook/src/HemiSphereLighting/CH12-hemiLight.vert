//
// Vertex shader for hemispherical lighting
//
// Author: Randi Rost
//
// Copyright (C) 2005 3Dlabs, Inc.
//
// See 3Dlabs-License.txt for license information
//


uniform vec3 LightPosition;
uniform vec3 SkyColor;
uniform vec3 GroundColor;
uniform mat4 ModelViewMatrix;
uniform mat3 NormalMatrix;

attribute vec4 Vertex;
attribute vec3 Normal;
varying vec3  DiffuseColor;  // GLSLdemo requires vertex/fragment shader pair

void main(void)
{
    vec3 ecPosition = vec3(ModelViewMatrix * Vertex);
    vec3 tnorm      = normalize(NormalMatrix * Normal);
    vec3 lightVec   = normalize(LightPosition - ecPosition);
    float costheta  = dot(tnorm, lightVec);
    float a         = 0.5 + 0.5 * costheta;

    DiffuseColor    = mix(GroundColor, SkyColor, a);

    gl_Position     = ModelViewMatrix * Vertex;
}