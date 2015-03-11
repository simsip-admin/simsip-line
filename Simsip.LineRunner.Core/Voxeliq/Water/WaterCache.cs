using Cocos2D;
using Engine.Common.Logging;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.GameObjects.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner;


namespace Engine.Water
{
    public class WaterCache : DrawableGameComponent, IWaterCache
    {
        private GameTime _gameTime;

        GraphicsDevice device;

        VertexBuffer waterVertexBuffer;
        VertexDeclaration waterVertexDeclaration;

        Effect effect;
        Effect bbEffect;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        Matrix reflectionViewMatrix;

        Texture2D waterBumpMap;

        const float waterHeight = 5.0f;
        RenderTarget2D refractionRenderTarget;
        Texture2D refractionMap;
        RenderTarget2D reflectionRenderTarget;
        Texture2D reflectionMap;

        VertexPositionTexture[] fullScreenVertices;
        VertexDeclaration fullScreenVertexDeclaration;

        Vector3 windDirection = new Vector3(0, 0, 1);

        // Logging-facility
        private static readonly Logger Logger = LogManager.CreateLogger();

        public WaterCache(Game game)
            : base(game)
        {
            // Export service
            this.Game.Services.AddService(typeof(IWaterCache), this);

        }

        #region DrawableGameComponent Overrides

        public override void Initialize()
        {
            Logger.Trace("init()");


            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.device = TheGame.SharedGame.GraphicsDevice;

            // TODO: Commenting out to get to build, put back in
            // effect = Content.Load<Effect>("Series4Effects");

            PresentationParameters pp = device.PresentationParameters;
            refractionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth16);
            reflectionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth16);

            SetUpWaterVertices();
            // TODO: No more vertex declareations
            // waterVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);

            fullScreenVertices = SetUpFullscreenVertices();
            // TODO: No more vertex declarations
            // fullScreenVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);

            // TODO: Commenting out to get to build, put back in
            // waterBumpMap = Content.Load<Texture2D>("waterbump");
            
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            this._gameTime = gameTime;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            this.Draw();
        }

        #endregion

        #region IWaterCache Implementation

        public void Draw(Effect effect = null, EffectType type = EffectType.None)
        {
            /* TODO: Uncomment when ready to implement
            float time = (float)this._gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
            DrawRefractionMap();
            DrawReflectionMap();
            DrawWater(time);
            */
        }

        private Plane CreatePlane(float height, Vector3 planeNormalDirection, Matrix currentViewMatrix, bool clipSide)
        {
            planeNormalDirection.Normalize();
            Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);
            if (clipSide)
                planeCoeffs *= -1;

            Matrix worldViewProjection = currentViewMatrix * projectionMatrix;
            Matrix inverseWorldViewProjection = Matrix.Invert(worldViewProjection);
            inverseWorldViewProjection = Matrix.Transpose(inverseWorldViewProjection);

            planeCoeffs = Vector4.Transform(planeCoeffs, inverseWorldViewProjection);
            Plane finalPlane = new Plane(planeCoeffs);

            return finalPlane;
        }

        private void DrawRefractionMap()
        {
            // TODO: How to replace clip planes
            // http://xboxforums.create.msdn.com/forums/t/56786.aspx
            Plane refractionPlane = CreatePlane(waterHeight + 1.5f, new Vector3(0, -1, 0), viewMatrix, false);
            // device.ClipPlanes[0].Plane = refractionPlane;
            // device.ClipPlanes[0].IsEnabled = true;
            device.SetRenderTarget(refractionRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            // TODO: Integrate into drawing pipeline
            // DrawTerrain(viewMatrix);
            // device.ClipPlanes[0].IsEnabled = false;

            device.SetRenderTarget(null);
            
            // TODO: Just use rendertargets
            // refractionMap = refractionRenderTarget.GetTexture();
        }

        private void DrawReflectionMap()
        {
            Plane reflectionPlane = CreatePlane(waterHeight - 0.5f, new Vector3(0, -1, 0), reflectionViewMatrix, true);
            
            // TODO: See link below for how to implement clip planes
            // device.ClipPlanes[0].Plane = reflectionPlane;
            //  device.ClipPlanes[0].IsEnabled = true;
            device.SetRenderTarget(reflectionRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            /* TODO: Hook into drawing pipeline
            DrawSkyDome(reflectionViewMatrix);
            DrawTerrain(reflectionViewMatrix);
            DrawBillboards(reflectionViewMatrix);
             */
            // device.ClipPlanes[0].IsEnabled = false;

            device.SetRenderTarget(null);
            
            // TODO: Just use render target
            // reflectionMap = reflectionRenderTarget.GetTexture();
        }

        private void DrawWater(float time)
        {
            effect.CurrentTechnique = effect.Techniques["Water"];
            Matrix worldMatrix = Matrix.Identity;
            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xView"].SetValue(viewMatrix);
            effect.Parameters["xReflectionView"].SetValue(reflectionViewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);
            effect.Parameters["xReflectionMap"].SetValue(reflectionMap);
            effect.Parameters["xRefractionMap"].SetValue(refractionMap);
            effect.Parameters["xWaterBumpMap"].SetValue(waterBumpMap);
            effect.Parameters["xWaveLength"].SetValue(0.1f);
            effect.Parameters["xWaveHeight"].SetValue(0.3f);

            // TODO
            // effect.Parameters["xCamPos"].SetValue(cameraPosition);

            effect.Parameters["xTime"].SetValue(time);
            effect.Parameters["xWindForce"].SetValue(0.002f);
            effect.Parameters["xWindDirection"].SetValue(windDirection);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                /* TODO: Comment out to get to build, put back in
                device.Vertices[0].SetSource(waterVertexBuffer, 0, VertexPositionTexture.SizeInBytes);
                device.VertexDeclaration = waterVertexDeclaration;
                int noVertices = waterVertexBuffer.SizeInBytes / VertexPositionTexture.SizeInBytes;
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, noVertices / 3);
                */
            }
        }

        #endregion

        #region Helper methods
        private void SetUpWaterVertices()
        {
            VertexPositionTexture[] waterVertices = new VertexPositionTexture[6];

            /* TODO: Commenting out to get to build, put back in
            waterVertices[0] = new VertexPositionTexture(new Vector3(0, waterHeight, 0), new Vector2(0, 1));
            waterVertices[2] = new VertexPositionTexture(new Vector3(terrainWidth, waterHeight, -terrainLength), new Vector2(1, 0));
            waterVertices[1] = new VertexPositionTexture(new Vector3(0, waterHeight, -terrainLength), new Vector2(0, 0));

            waterVertices[3] = new VertexPositionTexture(new Vector3(0, waterHeight, 0), new Vector2(0, 1));
            waterVertices[5] = new VertexPositionTexture(new Vector3(terrainWidth, waterHeight, 0), new Vector2(1, 1));
            waterVertices[4] = new VertexPositionTexture(new Vector3(terrainWidth, waterHeight, -terrainLength), new Vector2(1, 0));

            waterVertexBuffer = new VertexBuffer(device, waterVertices.Length * VertexPositionTexture.SizeInBytes, BufferUsage.WriteOnly);
            waterVertexBuffer.SetData(waterVertices);
            */
        }

        private VertexPositionTexture[] SetUpFullscreenVertices()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 1));
            vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 0));

            return vertices;
        }


        #endregion
    }
}

/*
using System;
 using System.Collections.Generic;
 using Microsoft.Xna.Framework;
 using Microsoft.Xna.Framework.Audio;
 using Microsoft.Xna.Framework.Content;
 using Microsoft.Xna.Framework.GamerServices;
 using Microsoft.Xna.Framework.Graphics;
 using Microsoft.Xna.Framework.Input;
 using Microsoft.Xna.Framework.Net;
 using Microsoft.Xna.Framework.Storage;
 
 namespace XNAseries4
 {
     public struct VertexMultitextured
     {
         public Vector3 Position;
         public Vector3 Normal;
         public Vector4 TextureCoordinate;
         public Vector4 TexWeights;
 
         public static int SizeInBytes = (3 + 3 + 4 + 4) * sizeof(float);
         public static VertexElement[] VertexElements = new VertexElement[]
          {
              new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
              new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
              new VertexElement( 0, sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0 ),
              new VertexElement( 0, sizeof(float) * 10, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1 ),
          };
     }
 
     public class Game1 : Microsoft.Xna.Framework.Game
     {
         GraphicsDeviceManager graphics;
         GraphicsDevice device;
 
         int terrainWidth;
         int terrainLength;
         float[,] heightData;
 
         VertexBuffer terrainVertexBuffer;
         IndexBuffer terrainIndexBuffer;
         VertexDeclaration terrainVertexDeclaration;
 
         VertexBuffer waterVertexBuffer;
         VertexDeclaration waterVertexDeclaration;
 
         VertexBuffer treeVertexBuffer;
         VertexDeclaration treeVertexDeclaration;
 
         Effect effect;
         Effect bbEffect;
         Matrix viewMatrix;
         Matrix projectionMatrix;
         Matrix reflectionViewMatrix;
 
         Vector3 cameraPosition = new Vector3(130, 30, -50);
         float leftrightRot = MathHelper.PiOver2;
         float updownRot = -MathHelper.Pi / 10.0f;
         const float rotationSpeed = 0.3f;
         const float moveSpeed = 30.0f;
         MouseState originalMouseState;
 
         Texture2D grassTexture;
         Texture2D sandTexture;
         Texture2D rockTexture;
         Texture2D snowTexture;
         Texture2D cloudMap;
         Texture2D waterBumpMap;
         Texture2D treeTexture;
         Texture2D treeMap;
 
         Model skyDome;
 
         const float waterHeight = 5.0f;
         RenderTarget2D refractionRenderTarget;
         Texture2D refractionMap;
         RenderTarget2D reflectionRenderTarget;
         Texture2D reflectionMap;
         RenderTarget2D cloudsRenderTarget;
         Texture2D cloudStaticMap;
         VertexPositionTexture[] fullScreenVertices;
         VertexDeclaration fullScreenVertexDeclaration;
 
         Vector3 windDirection = new Vector3(0, 0, 1);
 
         public Game1()
         {
             graphics = new GraphicsDeviceManager(this);
             Content.RootDirectory = "Content";
         }
 
         protected override void Initialize()
         {
             graphics.PreferredBackBufferWidth = 500;
             graphics.PreferredBackBufferHeight = 500;
 
             graphics.ApplyChanges();
             Window.Title = "Riemer's XNA Tutorials -- Series 4";
 
             base.Initialize();
         }
 
         protected override void LoadContent()
         {
             device = GraphicsDevice;
 

            effect = Content.Load<Effect> ("Series4Effects");
            bbEffect = Content.Load<Effect> ("bbEffect");            UpdateViewMatrix();
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.3f, 1000.0f);

            Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();


            skyDome = Content.Load<Model> ("dome"); skyDome.Meshes[0].MeshParts[0].Effect = effect.Clone(device);
            PresentationParameters pp = device.PresentationParameters;
            refractionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, 1, device.DisplayMode.Format);
            reflectionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, 1, device.DisplayMode.Format);
            cloudsRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, 1, device.DisplayMode.Format);

            LoadVertices();
            LoadTextures();
        }

        private void LoadVertices()
        {

            Texture2D heightMap = Content.Load<Texture2D> ("heightmap"); LoadHeightData(heightMap);
            VertexMultitextured[] terrainVertices = SetUpTerrainVertices();
            int[] terrainIndices = SetUpTerrainIndices();
            terrainVertices = CalculateNormals(terrainVertices, terrainIndices);
            CopyToTerrainBuffers(terrainVertices, terrainIndices);
            terrainVertexDeclaration = new VertexDeclaration(device, VertexMultitextured.VertexElements);

            SetUpWaterVertices();
            waterVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);


            Texture2D treeMap = Content.Load<Texture2D> ("treeMap");
            List<Vector3> treeList = GenerateTreePositions(treeMap, terrainVertices);            CreateBillboardVerticesFromList(treeList);

            fullScreenVertices = SetUpFullscreenVertices();
            fullScreenVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
        }

        private void LoadTextures()
        {

            grassTexture = Content.Load<Texture2D> ("grass");
            sandTexture = Content.Load<Texture2D> ("sand");
            rockTexture = Content.Load<Texture2D> ("rock");
            snowTexture = Content.Load<Texture2D> ("snow");
            cloudMap = Content.Load<Texture2D> ("cloudMap");
            waterBumpMap = Content.Load<Texture2D> ("waterbump");
            treeTexture = Content.Load<Texture2D> ("tree");
            treeMap = Content.Load<Texture2D> ("treeMap");            cloudStaticMap = CreateStaticMap(32);
        }

        private void LoadHeightData(Texture2D heightMap)
        {
            float minimumHeight = float.MaxValue;
            float maximumHeight = float.MinValue;

            terrainWidth = heightMap.Width;
            terrainLength = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainLength];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainLength];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R;
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                    heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 30.0f;
        }

        private VertexMultitextured[] SetUpTerrainVertices()
        {
            VertexMultitextured[] terrainVertices = new VertexMultitextured[terrainWidth * terrainLength];

            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainLength; y++)
                {
                    terrainVertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);
                    terrainVertices[x + y * terrainWidth].TextureCoordinate.X = (float)x / 30.0f;
                    terrainVertices[x + y * terrainWidth].TextureCoordinate.Y = (float)y / 30.0f;

                    terrainVertices[x + y * terrainWidth].TexWeights.X = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 0) / 8.0f, 0, 1);
                    terrainVertices[x + y * terrainWidth].TexWeights.Y = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 12) / 6.0f, 0, 1);
                    terrainVertices[x + y * terrainWidth].TexWeights.Z = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 20) / 6.0f, 0, 1);
                    terrainVertices[x + y * terrainWidth].TexWeights.W = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 30) / 6.0f, 0, 1);

                    float total = terrainVertices[x + y * terrainWidth].TexWeights.X;
                    total += terrainVertices[x + y * terrainWidth].TexWeights.Y;
                    total += terrainVertices[x + y * terrainWidth].TexWeights.Z;
                    total += terrainVertices[x + y * terrainWidth].TexWeights.W;

                    terrainVertices[x + y * terrainWidth].TexWeights.X /= total;
                    terrainVertices[x + y * terrainWidth].TexWeights.Y /= total;
                    terrainVertices[x + y * terrainWidth].TexWeights.Z /= total;
                    terrainVertices[x + y * terrainWidth].TexWeights.W /= total;
                }
            }

            return terrainVertices;
        }

        private int[] SetUpTerrainIndices()
        {
            int[] indices = new int[(terrainWidth - 1) * (terrainLength - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainLength - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }

            return indices;
        }

        private VertexMultitextured[] CalculateNormals(VertexMultitextured[] vertices, int[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();

            return vertices;
        }

        private void CopyToTerrainBuffers(VertexMultitextured[] vertices, int[] indices)
        {
            terrainVertexBuffer = new VertexBuffer(device, vertices.Length * VertexMultitextured.SizeInBytes, BufferUsage.WriteOnly);
            terrainVertexBuffer.SetData(vertices);

            terrainIndexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            terrainIndexBuffer.SetData(indices);
        }

        private void SetUpWaterVertices()
        {
            VertexPositionTexture[] waterVertices = new VertexPositionTexture[6];

            waterVertices[0] = new VertexPositionTexture(new Vector3(0, waterHeight, 0), new Vector2(0, 1));
            waterVertices[2] = new VertexPositionTexture(new Vector3(terrainWidth, waterHeight, -terrainLength), new Vector2(1, 0));
            waterVertices[1] = new VertexPositionTexture(new Vector3(0, waterHeight, -terrainLength), new Vector2(0, 0));

            waterVertices[3] = new VertexPositionTexture(new Vector3(0, waterHeight, 0), new Vector2(0, 1));
            waterVertices[5] = new VertexPositionTexture(new Vector3(terrainWidth, waterHeight, 0), new Vector2(1, 1));
            waterVertices[4] = new VertexPositionTexture(new Vector3(terrainWidth, waterHeight, -terrainLength), new Vector2(1, 0));

            waterVertexBuffer = new VertexBuffer(device, waterVertices.Length * VertexPositionTexture.SizeInBytes, BufferUsage.WriteOnly);
            waterVertexBuffer.SetData(waterVertices);
        }


        private void CreateBillboardVerticesFromList(List<Vector3> treeList)        {
            VertexPositionTexture[] billboardVertices = new VertexPositionTexture[treeList.Count * 6];
            int i = 0;
            foreach (Vector3 currentV3 in treeList)
            {
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(0, 0));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(1, 0));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(1, 1));

                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(0, 0));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(1, 1));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(0, 1));
            }

            treeVertexBuffer = new VertexBuffer(device, billboardVertices.Length * VertexPositionTexture.SizeInBytes, BufferUsage.WriteOnly);
            treeVertexBuffer.SetData(billboardVertices);
            treeVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
        }


        private List<Vector3> GenerateTreePositions(Texture2D treeMap, VertexMultitextured[] terrainVertices)        {
            Color[] treeMapColors = new Color[treeMap.Width * treeMap.Height];
            treeMap.GetData(treeMapColors);

            int[,] noiseData = new int[treeMap.Width, treeMap.Height];
            for (int x = 0; x < treeMap.Width; x++)
                for (int y = 0; y < treeMap.Height; y++)
                    noiseData[x, y] = treeMapColors[y + x * treeMap.Height].R;


            List<Vector3> treeList = new List<Vector3> ();                        Random random = new Random();

            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainLength; y++)
                {
                    float terrainHeight = heightData[x, y];
                    if ((terrainHeight > 8) && (terrainHeight < 14))
                    {
                        float flatness = Vector3.Dot(terrainVertices[x + y * terrainWidth].Normal, new Vector3(0, 1, 0));
                        float minFlatness = (float)Math.Cos(MathHelper.ToRadians(15));
                        if (flatness > minFlatness)
                        {
                            float relx = (float)x / (float)terrainWidth;
                            float rely = (float)y / (float)terrainLength;

                            float noiseValueAtCurrentPosition = noiseData[(int)(relx * treeMap.Width), (int)(rely * treeMap.Height)];
                            float treeDensity;
                            if (noiseValueAtCurrentPosition > 200)
                                treeDensity = 5;
                            else if (noiseValueAtCurrentPosition > 150)
                                treeDensity = 4;
                            else if (noiseValueAtCurrentPosition > 100)
                                treeDensity = 3;
                            else
                                treeDensity = 0;

                            for (int currDetail = 0; currDetail < treeDensity; currDetail++)
                            {
                                float rand1 = (float)random.Next(1000) / 1000.0f;
                                float rand2 = (float)random.Next(1000) / 1000.0f;
                                Vector3 treePos = new Vector3((float)x - rand1, 0, -(float)y - rand2);
                                treePos.Y = heightData[x, y];
                                treeList.Add(treePos);
                            }
                        }
                    }
                }
            }

            return treeList;
        }

        private Texture2D CreateStaticMap(int resolution)
        {
            Random rand = new Random();
            Color[] noisyColors = new Color[resolution * resolution];
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                    noisyColors[x + y * resolution] = new Color(new Vector3((float)rand.Next(1000) / 1000.0f, 0, 0));

            Texture2D noiseImage = new Texture2D(device, resolution, resolution, 1, TextureUsage.None, SurfaceFormat.Color);
            noiseImage.SetData(noisyColors);
            return noiseImage;
        }

        private VertexPositionTexture[] SetUpFullscreenVertices()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 1));
            vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 0));

            return vertices;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            ProcessInput(timeDifference);

            base.Update(gameTime);
        }

        private void ProcessInput(float amount)
        {
            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState != originalMouseState)
            {
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftrightRot -= rotationSpeed * xDifference * amount;
                updownRot -= rotationSpeed * yDifference * amount;
                Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
                UpdateViewMatrix();
            }

            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.Q))
                moveVector += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.Z))
                moveVector += new Vector3(0, -1, 0);
            AddToCameraPosition(moveVector * amount);
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);

            Vector3 reflCameraPosition = cameraPosition;
            reflCameraPosition.Y = -cameraPosition.Y + waterHeight * 2;
            Vector3 reflTargetPos = cameraFinalTarget;
            reflTargetPos.Y = -cameraFinalTarget.Y + waterHeight * 2;

            Vector3 cameraRight = Vector3.Transform(new Vector3(1, 0, 0), cameraRotation);
            Vector3 invUpVector = Vector3.Cross(cameraRight, reflTargetPos - reflCameraPosition);

            reflectionViewMatrix = Matrix.CreateLookAt(reflCameraPosition, reflTargetPos, invUpVector);
        }

        protected override void Draw(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;

            DrawRefractionMap();
            DrawReflectionMap();
            GeneratePerlinNoise(time);

            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            DrawSkyDome(viewMatrix);
            DrawTerrain(viewMatrix);
            DrawWater(time);
            DrawBillboards(viewMatrix);

            base.Draw(gameTime);
        }

        private void DrawTerrain(Matrix currentViewMatrix)
        {
            effect.CurrentTechnique = effect.Techniques["MultiTextured"];
            effect.Parameters["xTexture0"].SetValue(sandTexture);
            effect.Parameters["xTexture1"].SetValue(grassTexture);
            effect.Parameters["xTexture2"].SetValue(rockTexture);
            effect.Parameters["xTexture3"].SetValue(snowTexture);

            Matrix worldMatrix = Matrix.Identity;
            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xView"].SetValue(currentViewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);

            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xAmbient"].SetValue(0.4f);
            effect.Parameters["xLightDirection"].SetValue(new Vector3(-0.5f, -1, -0.5f));

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.Vertices[0].SetSource(terrainVertexBuffer, 0, VertexMultitextured.SizeInBytes);
                device.Indices = terrainIndexBuffer;
                device.VertexDeclaration = terrainVertexDeclaration;

                int noVertices = terrainVertexBuffer.SizeInBytes / VertexMultitextured.SizeInBytes;
                int noTriangles = terrainIndexBuffer.SizeInBytes / sizeof(int) / 3;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noVertices, 0, noTriangles);

                pass.End();
            }
            effect.End();
        }

        private void DrawSkyDome(Matrix currentViewMatrix)
        {
            device.RenderState.DepthBufferWriteEnable = false;

            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(100) * Matrix.CreateTranslation(cameraPosition);
            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;

                     currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
                     currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                     currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                     currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                     currentEffect.Parameters["xTexture"].SetValue(cloudMap);
                 }
                 mesh.Draw();
             }
             device.RenderState.DepthBufferWriteEnable = true;
         }
 
         private Plane CreatePlane(float height, Vector3 planeNormalDirection, Matrix currentViewMatrix, bool clipSide)
         {
             planeNormalDirection.Normalize();
             Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);
             if (clipSide)
                 planeCoeffs *= -1;
 
             Matrix worldViewProjection = currentViewMatrix * projectionMatrix;
             Matrix inverseWorldViewProjection = Matrix.Invert(worldViewProjection);
             inverseWorldViewProjection = Matrix.Transpose(inverseWorldViewProjection);
 
             planeCoeffs = Vector4.Transform(planeCoeffs, inverseWorldViewProjection);
             Plane finalPlane = new Plane(planeCoeffs);
 
             return finalPlane;
         }
 
         private void DrawRefractionMap()
         {
             Plane refractionPlane = CreatePlane(waterHeight + 1.5f, new Vector3(0, -1, 0), viewMatrix, false);
             device.ClipPlanes[0].Plane = refractionPlane;
             device.ClipPlanes[0].IsEnabled = true;
             device.SetRenderTarget(0, refractionRenderTarget);
             device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
             DrawTerrain(viewMatrix);
             device.ClipPlanes[0].IsEnabled = false;
 
             device.SetRenderTarget(0, null);
             refractionMap = refractionRenderTarget.GetTexture();
         }
 
         private void DrawReflectionMap()
         {
             Plane reflectionPlane = CreatePlane(waterHeight - 0.5f, new Vector3(0, -1, 0), reflectionViewMatrix, true);
             device.ClipPlanes[0].Plane = reflectionPlane;
             device.ClipPlanes[0].IsEnabled = true;
             device.SetRenderTarget(0, reflectionRenderTarget);
             device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
             DrawSkyDome(reflectionViewMatrix);
             DrawTerrain(reflectionViewMatrix);
             DrawBillboards(reflectionViewMatrix);
             device.ClipPlanes[0].IsEnabled = false;
 
             device.SetRenderTarget(0, null);
             reflectionMap = reflectionRenderTarget.GetTexture();
         }
 
         private void DrawWater(float time)
         {
             effect.CurrentTechnique = effect.Techniques["Water"];
             Matrix worldMatrix = Matrix.Identity;
             effect.Parameters["xWorld"].SetValue(worldMatrix);
             effect.Parameters["xView"].SetValue(viewMatrix);
             effect.Parameters["xReflectionView"].SetValue(reflectionViewMatrix);
             effect.Parameters["xProjection"].SetValue(projectionMatrix);
             effect.Parameters["xReflectionMap"].SetValue(reflectionMap);
             effect.Parameters["xRefractionMap"].SetValue(refractionMap);
             effect.Parameters["xWaterBumpMap"].SetValue(waterBumpMap);
             effect.Parameters["xWaveLength"].SetValue(0.1f);
             effect.Parameters["xWaveHeight"].SetValue(0.3f);
             effect.Parameters["xCamPos"].SetValue(cameraPosition);
             effect.Parameters["xTime"].SetValue(time);
             effect.Parameters["xWindForce"].SetValue(0.002f);
             effect.Parameters["xWindDirection"].SetValue(windDirection);
 
             effect.Begin();
             foreach (EffectPass pass in effect.CurrentTechnique.Passes)
             {
                 pass.Begin();
 
                 device.Vertices[0].SetSource(waterVertexBuffer, 0, VertexPositionTexture.SizeInBytes);
                 device.VertexDeclaration = waterVertexDeclaration;
                 int noVertices = waterVertexBuffer.SizeInBytes / VertexPositionTexture.SizeInBytes;
                 device.DrawPrimitives(PrimitiveType.TriangleList, 0, noVertices / 3);
 
                 pass.End();
             }
             effect.End();
         }
 
         private void DrawBillboards(Matrix currentViewMatrix)
         {
             bbEffect.CurrentTechnique = bbEffect.Techniques["CylBillboard"];
             bbEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
             bbEffect.Parameters["xView"].SetValue(currentViewMatrix);
             bbEffect.Parameters["xProjection"].SetValue(projectionMatrix);
             bbEffect.Parameters["xCamPos"].SetValue(cameraPosition);
             bbEffect.Parameters["xAllowedRotDir"].SetValue(new Vector3(0, 1, 0));
             bbEffect.Parameters["xBillboardTexture"].SetValue(treeTexture);
 
             bbEffect.Begin();
 
             device.Vertices[0].SetSource(treeVertexBuffer, 0, VertexPositionTexture.SizeInBytes);
             device.VertexDeclaration = treeVertexDeclaration;
             int noVertices = treeVertexBuffer.SizeInBytes / VertexPositionTexture.SizeInBytes;
             int noTriangles = noVertices / 3;
             {
                 device.RenderState.AlphaTestEnable = true;
                 device.RenderState.AlphaFunction = CompareFunction.GreaterEqual;
                 device.RenderState.ReferenceAlpha = 200;
 
                 bbEffect.CurrentTechnique.Passes[0].Begin();
                 device.DrawPrimitives(PrimitiveType.TriangleList, 0, noTriangles);
                 bbEffect.CurrentTechnique.Passes[0].End();
             }
 
             {
                 device.RenderState.DepthBufferWriteEnable = false;
 
                 device.RenderState.AlphaBlendEnable = true;
                 device.RenderState.SourceBlend = Blend.SourceAlpha;
                 device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
 
                 device.RenderState.AlphaTestEnable = true;
                 device.RenderState.AlphaFunction = CompareFunction.Less;
                 device.RenderState.ReferenceAlpha = 200;
 
                 bbEffect.CurrentTechnique.Passes[0].Begin();
                 device.DrawPrimitives(PrimitiveType.TriangleList, 0, noTriangles);
                 bbEffect.CurrentTechnique.Passes[0].End();
             }
             
             device.RenderState.AlphaBlendEnable = false;
             device.RenderState.DepthBufferWriteEnable = true;
             device.RenderState.AlphaTestEnable = false;            
 
             bbEffect.End();
         }
 
         private void GeneratePerlinNoise(float time)
         {
             device.SetRenderTarget(0, cloudsRenderTarget);
             device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
                         
             effect.CurrentTechnique = effect.Techniques["PerlinNoise"];
             effect.Parameters["xTexture"].SetValue(cloudStaticMap);
             effect.Parameters["xOvercast"].SetValue(1.1f);
             effect.Parameters["xTime"].SetValue(time/1000.0f);
             effect.Begin();
             foreach (EffectPass pass in effect.CurrentTechnique.Passes)
             {
                 pass.Begin();
 
                 device.VertexDeclaration = fullScreenVertexDeclaration;
                 device.DrawUserPrimitives(PrimitiveType.TriangleStrip, fullScreenVertices, 0, 2);
 
                 pass.End();
             }
             effect.End();
 
             device.SetRenderTarget(0, null);
             cloudMap = cloudsRenderTarget.GetTexture();
         }
     }
 }
*/