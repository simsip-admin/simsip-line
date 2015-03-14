// Use this as a starting point to build out shaders.
// 1. Copy contents to BasicEffect2.frag
// 2. Use BasicEffect2.cs (DrawableComponent) to stage build out.
// 3. Build out BasicEffect2.vert/frag as your sandbox.
// 4. When stabilized and close to finished product
//    a. copy BasicEffect2.vert/frag to <FinalName>.vert/frag
//    b. copy BasicEffect2.cs to <FinalName>.cs

// Use of the precision keyword is a GLES requirement
precision mediump float;

varying vec4 DestinationColor; 
 
void main(void) 
{ 
    gl_FragColor = DestinationColor;
}