// This vertex shader calculates light and eye vectors for 
// a bump mapping effect.

/* Set the default floating point precision */
precision mediump float;

/* The vertex co-ordinates */
attribute vec4 coord;

/* The texture co-ordinate for the vertex */
attribute vec2 texcoord;

/* The tangent vector to the vertex */
attribute vec4 tangent;

/* The normal for the vertex */
attribute vec4 normal;

/* The modelview projection matrix */
uniform mat4 modelViewProjection;

/* The camera matrix */
uniform mat4 projection;

/* The camera matrix */
uniform mat4 modelView;

/* Matrix for transforming vectors into camera space */
uniform mat3 normalMatrix;

/* Position of the light source */
uniform vec3 lightPosition;

/* Texture co-ordinates */
varying float x;
varying float y;

/* Light vector in tangent space */
varying vec3 lightDir;

/* Eye vector in tangent space */
varying vec3 eyeDir;

/* Vertex shader entry point */
void main() 
{
	/* Assign the texture co-ordinates to our new vertices */
	x = texcoord.x;
	y = texcoord.y;

	/* Calculate the eye direction for the vertex by applying
	 * the camera matrix.
	 */	
	eyeDir = vec3(modelView * coord);

	/* Calculate basis vectors for tangent space */
	vec3 n = normalize(normalMatrix * normal.xyz);
	vec3 t = normalize(normalMatrix * tangent.xyz);
	vec3 b = cross(n,t);

	/* Project light vector into tangent space */	
	vec3 v;
	v.x = dot(lightPosition,t);
	v.y = dot(lightPosition,b);
	v.z = dot(lightPosition,n);
	lightDir = normalize(v);

	/* Project eye vector into tangent space */
	v.x = dot(eyeDir,t);
	v.y = dot(eyeDir,b);
	v.z = dot(eyeDir,n);
	eyeDir = normalize(v);

	/* Apply the MVP matrix */
	gl_Position = modelViewProjection * coord;
}

