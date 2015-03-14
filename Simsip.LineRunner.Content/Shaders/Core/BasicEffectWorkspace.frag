// Use of the precision keyword is a GLES requirement
precision mediump float;

uniform sampler2D BlockTextureAtlasSampler;
varying vec2 BlockTextureCoordOut;
 
void main(void) 
{ 
    gl_FragColor = texture2D(BlockTextureAtlasSampler, BlockTextureCoordOut);
}