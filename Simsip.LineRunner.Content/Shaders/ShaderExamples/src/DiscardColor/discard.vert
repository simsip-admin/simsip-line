// Vertex shader to reneder the brick pattern.

uniform vec3 LightPosition;
/* The Modelview matrix */
uniform mat4 ModelViewMatrix;

/* The Normal Matrix */
uniform mat3 NormalMatrix;

/* The vertex co-ordinates */
attribute vec4 coord;

/* The normal vector for each vertex */
attribute vec3 normal;

const float SpecularContribution = 0.3;
const float DiffuseContribution  = 1.0 - SpecularContribution;

/* Light Intensity: Color due to lighting computations */
varying float LightIntensity;

/* Variable to store 2D vertex positions */
varying vec2  MCposition;

void main()
{
    vec3 ecPosition = vec3(ModelViewMatrix * coord);
    vec3 tnorm      = normalize(NormalMatrix * normal);
    vec3 lightVec   = normalize(LightPosition - ecPosition);
    vec3 reflectVec = reflect(-lightVec, tnorm);
    vec3 viewVec    = normalize(-ecPosition);
    float diffuse   = max(dot(lightVec, tnorm), 0.0);
    float spec      = 0.0;

    if (diffuse > 0.0)
    {
        spec = max(dot(reflectVec, viewVec), 0.0);
        spec = pow(spec, 16.0);
    }

	/* Lighting computations */
    LightIntensity  = DiffuseContribution * diffuse +
                      SpecularContribution * spec;
	/* Store the x,y position of vertex in a varying variable */
    MCposition      = (coord).xy;
    
    /* Apply the modelview matrix to the vertices */
    gl_Position     = 	ModelViewMatrix*coord;
}

