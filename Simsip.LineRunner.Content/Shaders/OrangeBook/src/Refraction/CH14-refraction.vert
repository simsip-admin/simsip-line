//
// Vertex shader for refraction
//
// Author: Randi Rost
//
// Copyright (c) 2003-2006: 3Dlabs, Inc.
//
// See 3Dlabs-License.txt for license information
//

const float Eta = 0.66;         // Ratio of indices of refraction 
const float FresnelPower = 1.4; 

const float F  = ((1.0-Eta) * (1.0-Eta)) / ((1.0+Eta) * (1.0+Eta));

uniform mat4 ModelViewMatrix;
uniform mat3 NormalMatrix;
uniform mat4 TextureMatrix;

varying vec3  Reflect;
varying vec3  Refract;
varying float Ratio;

attribute vec4 Vertex;
attribute vec3 Normal;

void main()
{
    vec4 ecPosition  = ModelViewMatrix * Vertex;
    vec3 ecPosition3 = ecPosition.xyz / ecPosition.w;

    vec3 i = normalize(ecPosition3);
    vec3 n = normalize(NormalMatrix * Normal);

    Ratio   = F + (1.0 - F) * pow((1.0 - dot(-i, n)), FresnelPower);

    Refract = refract(i, n, Eta);
    Refract = vec3(TextureMatrix * vec4(Refract.x, -Refract.y, Refract.z, 1.0));

    Reflect = reflect(i, n);
    Reflect = vec3(TextureMatrix * vec4(Reflect.x, -Reflect.y, Reflect.z, 1.0));
    
    //Refract = Reflect;
    //Reflect = Refract;
    gl_Position = ecPosition;
}