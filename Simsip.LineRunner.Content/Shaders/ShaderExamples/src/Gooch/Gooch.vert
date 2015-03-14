// Vertex shader for Gooch matte shading.

/* Set the default floating point precision */
precision mediump float;

/* Light position */
uniform vec3 LightPosition;

/* Modelview matrix */
uniform mat4 modelview;

/* Projection matrix */
uniform mat4 projection;

/* Normal matrix */
uniform mat3 NormalMatrix;

/* The vertex co-ordinates */
attribute vec4 coord;

/* The normal vector for each vertex */
attribute vec4 normals;

/* The light reflection angle */
varying float NdotL;

/* The light vector when reflected from the surface */
varying vec3 ReflectVec;

/* Vector from a vertex to the camera */
varying vec3 ViewVec;

/* Vertex shader entry point */
void main()
{
	/* Apply the modelview matrix to the vertices */
	vec3 ecPos = vec3(modelview * coord);
	
	/* Calculate the normal of the transformed vertices */
	vec3 tnorm = normalize(NormalMatrix * normals.xyz);
	
	/* Vector from the transformed vertex to the light */	
	vec3 lightVec = normalize(LightPosition - ecPos);
	
	/* Vector of the reflection of the light from the surface */
	ReflectVec = normalize(reflect(-lightVec, tnorm));

	/* Vector from the transformed vertex to the camera */	
	ViewVec = normalize(-ecPos);
	
	/* Calculate the angle of the light reflected from the surface,
	 * scaled to the range [0, 1].
	 */
	NdotL = (dot(lightVec, tnorm) + 1.0 ) * 0.5;
	
	/* Apply the MVP matrix */
	gl_Position = projection * modelview * coord;
}