//
// Vertex shader for glyph bombing
//
// Author: Joshua Doss, Randi Rost
//
// Copyright (c) 2003-2006: 3Dlabs, Inc.
//
// See 3Dlabs-License.txt for license information
//

uniform float  SpecularContribution;
uniform vec3   LightPosition;
uniform float  ScaleFactor;
uniform mat4 camera;
uniform mat3 NormalMatrix;

varying float  LightIntensity;
varying vec2 TexCoord;

attribute vec2 TextureCoord;
attribute vec4 Vertex;
attribute vec3 Normal;


void main()
{
    vec3  ecPosition = vec3(camera * Vertex);
    vec3  tnorm      = normalize(NormalMatrix * Normal);
    vec3  lightVec   = normalize(LightPosition - ecPosition);
    vec3  reflectVec = reflect(-lightVec, tnorm);
    vec3  viewVec    = normalize(-ecPosition);
    float diffuse    = max(dot(lightVec, tnorm), 0.0);
    float spec       = 0.0;

    if(diffuse > 0.0)
       {
          spec = max(dot(reflectVec, viewVec), 0.0);
          spec = pow(spec, 16.0);
       }
    
    float diffusecontribution  = 1.0 - SpecularContribution;
    LightIntensity = diffusecontribution * diffuse * 2.0 +
                         SpecularContribution * spec;
    
    TexCoord = TextureCoord.xy * ScaleFactor;
    
    gl_Position = camera * Vertex;
}