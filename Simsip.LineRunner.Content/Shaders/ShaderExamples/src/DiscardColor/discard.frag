/* Fragment shader to demonstrate discarding fragments based on the 
   color or position of the fragment. */

/* Set the default floating point precision */
precision mediump float;

/* Uniform variables - These can be set from the Shader
 * Uniforms panel.
 */
/* Color of brick and mortar */
uniform vec3  BrickColor, MortarColor;
/* Size of the brick: width and height */
uniform vec2  BrickSize;
/* Proportion of brick's width and height with Brick+Mortar pattern*/
uniform vec2  BrickPct;

/* Specified color to discard the fragments */
uniform vec3 discard_color;

/* Light Intensity: Color due to lighting computations */
varying float LightIntensity;

/* Variable to store 2D vertex positions */
varying vec2  MCposition;

void main()
{
    vec3  color;
    vec2  position, useBrick;
    
    position = MCposition / BrickSize;

    if (fract(position.y * 0.5) > 0.5)
    position.x += 0.5;

    position = fract(position);

    useBrick = step(position, BrickPct);

    color  = mix(MortarColor, BrickColor, useBrick.x * useBrick.y);
    
    /* Discard all the fragments which matches the supplied color */
    if ( color.xyz == discard_color.xyz )
    discard;
    
    /* Discard the fragments based on position. Comment out above
    two lines code and uncomment below two lines to see the effect 
    of discarding fragments based on position*/
    //if (position.y > 0.5)
    //discard;
    
    color *= LightIntensity;
    gl_FragColor = vec4(color, 1.0);
}