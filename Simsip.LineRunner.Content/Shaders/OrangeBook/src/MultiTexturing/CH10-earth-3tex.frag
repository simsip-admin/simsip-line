//
// Fragment shader for drawing the earth with three textures
//
// Author: Randi Rost
//
// Copyright (c) 2002-2006 3Dlabs Inc. Ltd. 
//
// See 3Dlabs-License.txt for license information
//


/* Use of the precision keyword is a GLES requirement */
precision mediump float;

#define PI  3.141592

uniform sampler2D EarthDay;
uniform sampler2D EarthNight;
uniform sampler2D EarthCloudGloss;

varying float Diffuse;
varying vec3  Specular;
varying vec3  normal;

/* atan2() function is not an inbuilt function in GLSL ES. 
This is an approximate implementation of atan2() function */

float aTan2(float y, float x) {
	float coeff_1 = PI / 4.0;
	float coeff_2 = 3.0 * coeff_1;
	float abs_y = abs(y);
	float angle;
	if (x >= 0.0) {
		float r = (x - abs_y) / (x + abs_y);
		angle = coeff_1 - coeff_1 * r;
	} else {
		float r = (x + abs_y) / (abs_y - x);
		angle = coeff_2 - coeff_1 * r;
	}
	return y < 0.0 ? -angle : angle;
}

void main()
{
	vec2  TexCoord;
	
	/* Calculate the texture coordinates for each fragment on the sphere surface.
	  We use the surface normal as a way to find where we are on surface of the sphere.
	  Considering normal as a vector from centre of the sphere, we compute
	  the latitude and longitude on the surface of the sphere */
	
	// A vector made up of x and z components tells the longitude on the globe.
	// Use atan2() to convert it to an angle in radians
	// This angle is between -PI and PI. Divide by 2PI, so it's now between -.5 and .5.
	// Add 0.5 to make it in the range of 0 to 1. This is the x value in the texture map.   
    TexCoord.x		= aTan2(normal.x, normal.z)/(2.0 * PI) + 0.5;
    
    // Consider a right triangle with a hypotenuse of length 1 (normalized normal vector)
    // and a rise of normal.y over x-z plane, we can compute the latitude using asin.
    // This is the angle between the normal vector and the projection of normal vector on x-z plane.
    // This angle varies from -PI / 2 to PI / 2. Divide by PI, so it's now between -0.5 and 0.5.
    // Add 0.5 to make it in the range of 0 to 1. This is the y value in the texture map. 
    TexCoord.y      = asin(normal.y)/PI + 0.5;
    
    // Monochrome cloud cover value will be in clouds.r
    // Gloss value will be in clouds.g
    // clouds.b will be unused

    vec2 clouds    = texture2D(EarthCloudGloss, TexCoord).rg;
    vec3 daytime   = (texture2D(EarthDay, TexCoord).rgb * Diffuse + 
                          Specular * clouds.g) * (1.0 - clouds.r) +
                          clouds.r * Diffuse;
    vec3 nighttime = texture2D(EarthNight, TexCoord).rgb * 
                         (1.0 - clouds.r) * 2.0;

    vec3 color = daytime;

    if (Diffuse < 0.1)
        color = mix(nighttime, daytime, (Diffuse + 0.1) * 5.0);

    gl_FragColor = vec4(color, 1.0);
}

