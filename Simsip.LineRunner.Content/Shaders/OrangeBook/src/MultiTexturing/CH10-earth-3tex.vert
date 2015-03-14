//
// Vertex shader for drawing the earth with three textures
//
// Author: Randi Rost
//
// Copyright (c) 2002-2006 3Dlabs Inc. Ltd. 
//
// See 3Dlabs-License.txt for license information
//

varying float Diffuse;
varying vec3  Specular;
varying vec3 normal;

uniform vec3 LightPosition;

uniform mat4 ModelViewMatrix;
uniform mat3 NormalMatrix;

attribute vec4 Vertex;
attribute vec3 Normal;

const vec3 Xunitvec = vec3(1.0, 0.0, 0.0);
const vec3 Yunitvec = vec3(0.0, 1.0, 0.0);

void main()
{
	normal = Normal;
    vec3 ecPosition = vec3(ModelViewMatrix * Vertex);
    vec3 tnorm      = normalize(NormalMatrix * Normal);
    vec3 lightVec   = normalize(LightPosition - ecPosition);
    vec3 reflectVec = reflect(-lightVec, tnorm);
    vec3 viewVec    = normalize(-ecPosition);

    float spec      = clamp(dot(reflectVec, viewVec), 0.0, 1.0);
    spec            = pow(spec, 8.0);
    Specular        = vec3(spec) * vec3(1.0, 0.941, 0.898) * 0.3;

    Diffuse         = max(dot(lightVec, tnorm), 0.0);
    gl_Position     = ModelViewMatrix * Vertex;
}
