// This vertex shader computes the normal and eye direction
// for a vertex, which is passed to the fragment shader
// for indexing into the environment map. Diffuse lighting
// is also calculated.

/* Set the default floating point precision */
precision mediump float;

/* The vertex co-ordinates */
attribute vec4 coord;

/* The normal for the vertex */
attribute vec4 normals;

/* The projection matrix */
uniform mat4 proj;

/* The modelview matrix */
uniform mat4 modelview;

/* Matrix for transforming vectors into camera space */
uniform mat3 normalMatrix;

/* Position of the light source */
uniform vec3 lightPosition;

/* Direction of the eye */
varying vec3 eyeDir;

/* Intensity of diffuse lighting */
varying float lightIntensity;

/* Normal after camera transformation */
varying vec3 normal;

/* Vertex shader entry point */
void main() 
{
	/* Apply the camera matrix */
	gl_Position = proj * modelview * coord;

	/* Transform the normals into camera space */
	normal = normalize(normalMatrix * normals.xyz);
	
	/* Calculate the eye direction */
	vec4 pos = modelview * coord;
	eyeDir = pos.xyz;
	
	/* Compute the diffuse lighting at the vertex */
	lightIntensity = max(dot(normalize(lightPosition - eyeDir), normal), 0.0);	

	
}

