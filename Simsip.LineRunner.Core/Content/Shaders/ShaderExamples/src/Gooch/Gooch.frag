// Fragment shader for Gooch matte shading

/* Use of the precision keyword is a GLES requirement */
precision mediump float;

/* The base surface colour of the object */
uniform vec3 	SurfaceColor;

/* Colours applied to 'warm' and 'cool' areas of the object */
uniform vec3 	WarmColor;
uniform vec3 	CoolColor;

/* Mix between surface colour, and cool/warm colours */
uniform float 	DiffuseWarm;
uniform float 	DiffuseCool;

/* Angle of light from normal */
varying float NdotL;

/* Interpolated reflection vector from the vertex shader */
varying vec3 ReflectVec;

/* Interpolated view vector from the vertex shader */
varying vec3 ViewVec;

/* Fragment shader entry point */
void main()
{
	/* Mix the warm and cool colours with the object's suface colour */
	vec3 kcool = min(CoolColor + DiffuseCool * SurfaceColor, 1.0);
	vec3 kwarm = min(WarmColor + DiffuseWarm * SurfaceColor, 1.0);

	/* Mix between cool and warm colours based on reflection angle */	
	vec3 kfinal = mix(kcool, kwarm, NdotL);

	/* Calculate the specular highlight */	
	vec3 nreflect = normalize(ReflectVec);
	vec3 nview = normalize(ViewVec);	
	float spec = max(dot(nreflect,nview), 0.0);
	spec = pow(spec, 22.0);

	/* Add the specular highlight into the cool/warm shading and assign
	 * to the fragment colour.
	 */
	gl_FragColor = vec4(min(kfinal + spec, 1.0), 1.0);
}
