
uniform vec4 LightPosition;
uniform mat4 ModelViewMatrix;
uniform mat3 NormalMatrix;

attribute vec4 Vertex;
attribute vec3 Normal;

varying float intensity;

void main()
{	
	vec3 lightDir = normalize(vec3(LightPosition));
	vec3 normal = normalize(NormalMatrix * Normal);
	intensity = max(dot(lightDir,normal),0.0); 
		
	gl_Position = ModelViewMatrix*Vertex;
}

