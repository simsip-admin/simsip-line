//
// Vertex shader for cube map environment mapping
//
// Author: Randi Rost
//
// Copyright (c) 2003-2006: 3Dlabs, Inc.
//
// See 3Dlabs-License.txt for license information
//

// This vertex shader declares a normal vector, and applies 
// the camera matrix. 

/* Set the default floating point precision */
precision mediump float;

/* The vertex co-ordinates */
attribute vec4 coord;
attribute vec3 Normal;

/* The modelview projection matrix */
uniform mat4 camera;

varying vec3  ReflectDir;
varying float LightIntensity;

uniform vec3  LightPos;
uniform mat3 NormalMatrix;

/* Vertex shader entry point */
void main() 
{	
	/* Apply the MVP matrix */
	gl_Position    = camera * coord;
    vec3 normal    = normalize(NormalMatrix * Normal);
    vec4 pos       = camera * coord;
    vec3 eyeDir    = pos.xyz;
    ReflectDir     = reflect(eyeDir, normal);
    ReflectDir	   = vec3(ReflectDir.x,-ReflectDir.y , ReflectDir.z);
    LightIntensity = max(dot(normalize(LightPos - eyeDir), normal),0.0);
}