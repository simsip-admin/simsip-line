attribute vec3 Position; 
attribute vec2 BlockTextureCoordIn; 

uniform mat4 ModelViewProjection;

uniform sampler2D BlockTextureAtlasSampler;

varying vec2 BlockTextureCoordOut; 
 
void main(void) 
{ 
	BlockTextureCoordOut = BlockTextureCoordIn;
    gl_Position = ModelViewProjection * vec4(Position, 1.0);
}

