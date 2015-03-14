// This fragment shader applies lighting and texture
// mapping effects.

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* Per-light data structure */
struct Light {
	vec3 halfVector;	// The sum of the eye and light vectors 
	vec3 position;		// The light direction vector
	vec4 ambient;		// The color of the ambient component of the light
	vec4 diffuse;		// The color of the diffuse component of the light
	vec4 specular;		// The color of the specular component of the light
};

/* Camera matrix */
uniform mat4 camera;

/* Shininess, controls the size of the specular highlight */
uniform float shininess;

/* The two-dimensioanl texture map */
uniform sampler2D texture;

/* Initial values for lighting components, as these are accumulated
 * for each light.
 */
vec4 ambient = vec4(0,0,0,0);
vec4 diffuse = vec4(0,0,0,0);
vec4 specular = vec4(0,0,0,0);

/* The texture co-ordinates from the vertex shader */
varying vec3 v;

/* The normal, computed in the vertex shader and varying across the 
 * fragment.
 */
varying vec4 normal;

/* Declare two lights */
Light light1;
Light light2;

/* Fragment shader entry point */
void main(void)
{
	float nDotVP;	// normal . light direction
	float nDotNV;	// normal . light half vector
	float pf;		// power factor

	/* Set up the first light */
	light1.halfVector = vec3(0.3, 0.2, 0.1);
	light1.position = vec3(2.0, 2.0, -5.0);
	light1.ambient = vec4(0.0, 0.0, 0.0, 1.0);
	light1.diffuse = vec4(0.1, 0.7, 0.4, 1.0);
	light1.specular = vec4(0.0, 0.5, 0.1, 1.0);

	/* Set up the second light */
	light2.halfVector = vec3(-0.3, -0.2, -0.1);
	light2.position = vec3(-2.0, -2.0, -8.0);
	light2.ambient = vec4(0.0, 0.2, 0.2, 1.0);
	light2.diffuse = vec4(0.7, 0.1, 0.4, 1.0);
	light2.specular = vec4(0.7, 0.1, 0.4, 1.0);

	/* Set the position of the first light */
	light1.position = (vec4(light1.position,1.0) * camera).xyz;

	/* Compute the intensity of light hitting the fragment */	
	nDotVP = max(0.0, dot(normal.xyz, normalize(vec3(light1.position))));
	
	/* Compute the specular component of the light hitting the fragment */ 
	nDotNV = max(0.0, dot(normal.xyz, vec3(light1.halfVector)));

	/* Compute the intensity of the specular highlight for the fragment */	
	if (nDotVP ==0.0)
		pf = 0.0;
	else
		pf = pow(nDotNV, shininess);

	/* Add in the ambient, diffuse and specular lighting components */		
	ambient += light1.ambient;
	diffuse += light1.diffuse * nDotVP;
	specular += light1.specular * pf;
	
	/* Repeat for the second light */
	light2.position = (vec4(light2.position,1.0) * camera).xyz;

	nDotVP = max(0.0, dot(normal.xyz, normalize(vec3(light2.position))));
	nDotNV = max(0.0, dot(normal.xyz, vec3(light2.halfVector)));
	
	if (nDotVP ==0.0)
		pf = 0.0;
	else
		pf = pow(nDotNV, shininess);
		
	ambient += light2.ambient;
	diffuse += light2.diffuse * nDotVP;
	specular += light2.specular * pf;
	
	/* Obtain the value from the texture at the current co-ordinate */
    vec4 composite = texture2D(texture,vec2(v.x, v.y));
 
 	/* Assign the texture and lighting result to the fragment color */
    gl_FragColor = mix(ambient + diffuse + specular, composite, 0.5);
}

