using Cocos2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenTK.Graphics.ES20;
using System;
using System.Linq;

namespace Simsip.LineRunner.Shaders
{
    public class BasicEffectWorkspace : DrawableGameComponent
    {
        // Setup strings to reference attributes and uniforms we need
        private readonly string PositionAttributeKey = "Position";
        private readonly string BlockTextureCoordInAttributeKey = "BlockTextureCoordIn";
        private readonly string ModelViewProjectionUniformKey = "ModelViewProjection";
        private readonly string BlockTextureAtlasSamplerKey = "BlockTextureAtlasSampler";

        // Texture support
        private readonly string TextureKey = "BasicTexture";
        private readonly int TextureUnit = 0;

        // Vertex buffer support
        private readonly string VerticesVertexBufferKey = "Vertices";
        private readonly string TextureCoordsVertexBufferKey = "TextureCoords";
        private readonly string IndicesVertexBufferKey = "Indices";

        // Matrix support
        private Matrix ModelViewProjection;

        // Optional, in case you want to render to an offscreen target
        // TODO: Not working yet
        private RenderTarget2D _renderTarget;
        
        private ShaderProgram _shaderProgram;
        private TexturedCube _texturedCube;

        public BasicEffectWorkspace(Game game, RenderTarget2D renderTarget=null)
            : base(game)
        {
            this._renderTarget = renderTarget;
        }

        public override void Initialize()
        {

 	        base.Initialize();
        }

        /// <summary>
        /// Needed to get access to the protected LoadContent()
        /// </summary>
        public void LoadContentProxy()
        {
            LoadContent();
        }

        protected override void LoadContent()
        {
            try
            {
                // Create, compile and link our program
                _shaderProgram = new ShaderProgram("BasicEffectWorkspace.vert", "BasicEffectWorkspace.frag");

                // TODO: Do we need to do this?
                _shaderProgram.UseProgram(true);

                // Initialize attribute and uniform locations we are interested in
                _shaderProgram.InitAttribLocation(PositionAttributeKey);
                _shaderProgram.InitAttribLocation(BlockTextureCoordInAttributeKey);
                _shaderProgram.InitUniformLocation(ModelViewProjectionUniformKey);
                _shaderProgram.InitUniformLocation(BlockTextureAtlasSamplerKey);

                // Load/Bind our uniforms
                var matrixTest = CCDrawManager.WorldMatrix;
                ModelViewProjection = /*CCDrawManager.WorldMatrix * */ CCDrawManager.ViewMatrix * CCDrawManager.ProjectionMatrix;
                _shaderProgram.UniformMatrix(ModelViewProjectionUniformKey, ModelViewProjection);

                // Load/Bind our texture
                var texture = Game.Content.Load<Texture2D>(@"Textures/terrain");
                _shaderProgram.LoadTexture(TextureKey,  Game.Content.Load<Texture2D>(@"Textures/terrain"));
                _shaderProgram.BindTextureToUniform(TextureUnit, 
                                                   TextureKey, 
                                                   BlockTextureAtlasSamplerKey);

                // Load our shape
                _texturedCube = new TexturedCube();

                // Load vertex buffers
                _shaderProgram.LoadVertexBuffer(VerticesVertexBufferKey,
                                                _texturedCube.Vertices.Count(),
                                                _texturedCube.Vertices);
                _shaderProgram.LoadVertexBuffer(TextureCoordsVertexBufferKey,
                                                _texturedCube.TextureCoords.Count(),
                                                _texturedCube.TextureCoords);
                _shaderProgram.LoadIndexBuffer(IndicesVertexBufferKey,
                                                _texturedCube.Indices.Count(),
                                                _texturedCube.Indices);

                // Bind previously loaded vertex buffers to attribute arrays and indices
                _shaderProgram.BindVertexBufferToAttribArray(VerticesVertexBufferKey, 
                                                             PositionAttributeKey);
                _shaderProgram.BindVertexBufferToAttribArray(TextureCoordsVertexBufferKey, 
                                                             BlockTextureCoordInAttributeKey);
                _shaderProgram.BindVertexBufferToIndices(IndicesVertexBufferKey);

                // Point attribute arrays into vertex buffers
                _shaderProgram.VertexAttribPointer(PositionAttributeKey, 3, All.Float, 3 * sizeof(float), 0);
                _shaderProgram.VertexAttribPointer(BlockTextureCoordInAttributeKey, 2, All.Float, 2 * sizeof(float), 0);

#if DEBUG
                _shaderProgram.ValidateProgram();
#endif
                // TODO: Is this needed (see above as well)
                _shaderProgram.UseProgram(false);
            }
            catch(Exception ex)
            {
                string temp = ex.ToString();
            }
        }

        public override void Update(GameTime gameTime)
        {
            // TODO: Add per frame update logic here
        }

        public override void Draw(GameTime gameTime)
        {
            try
            {
                // TODO: Haven't figured this out yet
                if (_renderTarget != null)
                {
                    // Draw into our offscreen buffer
                    Game.GraphicsDevice.SetRenderTarget(_renderTarget);
                }
#if DEBUG
                // Don't let incoming opengl errors confuse us
                ShaderProgram.ClearErrors();

                // Did we screw anything up in last cycle?
                _shaderProgram.ValidateProgram();
#endif
                // Enable the program we want for subsequent call to draw elements
                _shaderProgram.UseProgram(true);

                //
                // TODO: Do we have to rebind (see LoadContent)
                //

                // Bind our uniforms
                _shaderProgram.UniformMatrix(ModelViewProjectionUniformKey, ModelViewProjection);

                // Bind our texture
                _shaderProgram.BindTextureToUniform(TextureUnit,
                                                    TextureKey,
                                                    BlockTextureAtlasSamplerKey);

                // Bind previously loaded vertex buffers to attribute arrays and indices
                _shaderProgram.BindVertexBufferToAttribArray(VerticesVertexBufferKey,
                                                             PositionAttributeKey);
                _shaderProgram.BindVertexBufferToAttribArray(TextureCoordsVertexBufferKey,
                                                             BlockTextureCoordInAttributeKey);
                _shaderProgram.BindVertexBufferToIndices(IndicesVertexBufferKey);

                // Point attribute arrays into vertex buffers
                _shaderProgram.VertexAttribPointer(PositionAttributeKey, 3, All.Float, 3 * sizeof(float), 0);
                _shaderProgram.VertexAttribPointer(BlockTextureCoordInAttributeKey, 2, All.Float, 2 * sizeof(float), 0);

#if DEBUG
                _shaderProgram.ValidateProgram();
#endif
                // Render
                _shaderProgram.DrawElements(All.Triangles, _texturedCube.Indices.Count(), 0);

                // Clean-up
                _shaderProgram.UseProgram(false);

                // TODO: Have not figured this out yet
                if (_renderTarget != null)
                {
                    // Revert to back buffer
                    Game.GraphicsDevice.SetRenderTarget(null);
                }
            }
            catch(Exception ex)
            {
                string temp = ex.ToString();
            }
        }
    }
}