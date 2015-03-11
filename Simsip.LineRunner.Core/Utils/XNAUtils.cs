using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Cocos2D;
using Engine.Graphics;
using Engine.Universe;
using Simsip.LineRunner.GameFramework;
using System.Diagnostics;
using BEPUphysicsDemos;
using Engine.Input;


namespace Simsip.LineRunner.Utils
{
    public static class XNAUtils
    {
        public enum CameraType
        {
            Cocos2D,
            Player,
            Stationary,
            Tracking,
        }

        public enum ScaleModelBy
        {
            Width,
            Height
        }

        private static IInputManager _inputManager;
        private static Camera _playerCamera;
        private static Camera _trackingCamera;
        private static Camera _stationaryCamera;

        static XNAUtils()
        {

            XNAUtils._inputManager = (IInputManager)TheGame.SharedGame.Services.GetService(typeof(IInputManager));

            XNAUtils._playerCamera = XNAUtils._inputManager.PlayerCamera;
            XNAUtils._stationaryCamera = XNAUtils._inputManager.TheStationaryCamera;
            XNAUtils._trackingCamera = XNAUtils._inputManager.LineRunnerCamera;
        }

        public static IList<BasicEffect> GetOriginalEffects(Model model)
        {
            List<BasicEffect> effectList = new List<BasicEffect>();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart modmeshpart in mesh.MeshParts)
                {
                    try
                    {
                        BasicEffect oldEffect = (BasicEffect)modmeshpart.Effect;
                        effectList.Add(oldEffect);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception in GetOriginalEffects: " + ex);
                    }
                }
            }

            return effectList;
        }

        public static IList<BasicEffect> ChangeEffect(Model model, Effect newEffect)
        {
            List<BasicEffect> effectList = new List<BasicEffect>();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart modmeshpart in mesh.MeshParts)
                {
                    BasicEffect oldEffect = (BasicEffect)modmeshpart.Effect;
                    effectList.Add(oldEffect);
                    // TODO
                    // modmeshpart.Effect = newEffect.Clone(device);
                    modmeshpart.Effect = newEffect.Clone();
                }
            }

            return effectList;
        }

        /// <summary>
        /// Centralized implementation to set all states to known default values for
        /// default 3d rendering for this game.
        /// </summary>
        public static void DefaultDrawState()
        {
            TheGame.SharedGame.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            TheGame.SharedGame.GraphicsDevice.BlendState = BlendState.Opaque;
            TheGame.SharedGame.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            TheGame.SharedGame.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        /// <summary>
        /// Centralized implementation to set all states to known default values for
        /// Cocos2d rendering for this game.
        /// </summary>
        public static void Cocos2dDrawState()
        {
            TheGame.SharedGame.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            TheGame.SharedGame.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            TheGame.SharedGame.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            TheGame.SharedGame.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        public static CCPoint WorldToLogical(Vector3 worldCoordinates, CameraType cameraType)
        {
            // Pull view/projection matrix from designated camera
            Matrix viewMatrix;
            Matrix projectionMatrix;
            switch (cameraType)
            {
                case CameraType.Cocos2D:
                {
                    viewMatrix = CCDrawManager.ViewMatrix;
                    projectionMatrix = CCDrawManager.ProjectionMatrix;
                    break;
                }
                case CameraType.Player:
                {
                    viewMatrix = XNAUtils._playerCamera.ViewMatrix;
                    projectionMatrix = XNAUtils._playerCamera.ProjectionMatrix;
                    break;
                }
                case CameraType.Stationary:
                {
                    viewMatrix = XNAUtils._stationaryCamera.ViewMatrix;
                    projectionMatrix = XNAUtils._stationaryCamera.ProjectionMatrix;
                    break;
                }
                case CameraType.Tracking:
                {
                    viewMatrix = XNAUtils._trackingCamera.ViewMatrix;
                    projectionMatrix = XNAUtils._trackingCamera.ProjectionMatrix;
                    break;
                }
                default:
                {
                    // We need a default here to make sure code doesn't complain about unassigned locals
                    // but we'll still throw an exception
                    viewMatrix = XNAUtils._trackingCamera.ViewMatrix;
                    projectionMatrix = XNAUtils._trackingCamera.ProjectionMatrix;
                    throw new NotSupportedException("Default camera not supported in WorldToLogical");
                }
            }

            // Now project the world coordinates to screen coordinates
            var screenCoordinates = TheGame.SharedGame.GraphicsDevice.Viewport.Project(
                worldCoordinates,
                projectionMatrix,
                viewMatrix,
                Matrix.Identity);

            // And finally convert screen coordinates to logical coordinates
            var logicalCoordinates = new CCPoint(
                screenCoordinates.X * GameConstants.PixelToLogicalScaleWidth,
                (CCDrawManager.FrameSize.Height - screenCoordinates.Y) * GameConstants.PixelToLogicalScaleHeight);

            return logicalCoordinates;
        }

        public static CCPoint WorldToScreen(Vector3 worldCoordinates, CameraType cameraType)
        {
            // Pull view/projection matrix from designated camera
            Matrix viewMatrix;
            Matrix projectionMatrix;
            switch (cameraType)
            {
                case CameraType.Cocos2D:
                    {
                        viewMatrix = CCDrawManager.ViewMatrix;
                        projectionMatrix = CCDrawManager.ProjectionMatrix;
                        break;
                    }
                case CameraType.Player:
                    {
                        viewMatrix = XNAUtils._playerCamera.ViewMatrix;
                        projectionMatrix = XNAUtils._playerCamera.ProjectionMatrix;
                        break;
                    }
                case CameraType.Stationary:
                    {
                        viewMatrix = XNAUtils._stationaryCamera.ViewMatrix;
                        projectionMatrix = XNAUtils._stationaryCamera.ProjectionMatrix;
                        break;
                    }
                case CameraType.Tracking:
                    {
                        viewMatrix = XNAUtils._trackingCamera.ViewMatrix;
                        projectionMatrix = XNAUtils._trackingCamera.ProjectionMatrix;
                        break;
                    }
                default:
                    {
                        // We need a default here to make sure code doesn't complain about unassigned locals
                        // but we'll still throw an exception
                        viewMatrix = XNAUtils._trackingCamera.ViewMatrix;
                        projectionMatrix = XNAUtils._trackingCamera.ProjectionMatrix;
                        throw new NotSupportedException("Default camera not supported in WorldToLogical");
                    }
            }

            // Now project the world coordinates to screen coordinates
            var screenCoordinatesProjected = TheGame.SharedGame.GraphicsDevice.Viewport.Project(
                worldCoordinates,
                projectionMatrix,
                viewMatrix,
                Matrix.Identity);

            // And finally convert screen coordinates to logical coordinates
            var screenCoordinates = new Vector2(screenCoordinatesProjected.X, 
                                                screenCoordinatesProjected.Y);

            return screenCoordinates;
        }

        /// <summary>
        /// Given a a point in logical coordinates on the screen and a desired depth to unproject to,
        /// return the 3d world coordinate for this point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 LogicalToWorld(CCPoint logicalPoint, float desiredDepth, CameraType cameraType)
        {
            // Determine ratios to convert logical points to screen points
            var ccWinSize = CCDirector.SharedDirector.WinSize;
            var ratioWidth = CCDrawManager.FrameSize.Width / ccWinSize.Width;
            var ratioHeight = CCDrawManager.FrameSize.Height / ccWinSize.Height;
            
            // Convert cocos2d logical points to ui screen points
            var ccPoint = new CCPoint(logicalPoint.X * ratioWidth, logicalPoint.Y * ratioHeight);
            var uiPoint = new CCPoint(ccPoint.X, CCDrawManager.FrameSize.Height - ccPoint.Y);
            
            // Convert opengl screen points to opengl 3d points
            var nearScreenPoint = new Vector3(uiPoint.X, uiPoint.Y, 0);
            var farScreenPoint = new Vector3(uiPoint.X, uiPoint.Y, 1);

            // Pull view/projection matrix from designated camera
            Matrix viewMatrix;
            Matrix projectionMatrix;
            switch (cameraType)
            {
                case CameraType.Cocos2D:
                    {
                        viewMatrix = CCDrawManager.ViewMatrix;
                        projectionMatrix = CCDrawManager.ProjectionMatrix;
                        break;
                    }
                case CameraType.Player:
                    {
                        viewMatrix = XNAUtils._playerCamera.ViewMatrix;
                        projectionMatrix = XNAUtils._playerCamera.ProjectionMatrix;
                        break;
                    }
                case CameraType.Stationary:
                    {
                        viewMatrix = XNAUtils._stationaryCamera.ViewMatrix;
                        projectionMatrix = XNAUtils._stationaryCamera.ProjectionMatrix;
                        break;
                    }
                case CameraType.Tracking:
                    {
                        viewMatrix = XNAUtils._trackingCamera.ViewMatrix;
                        projectionMatrix = XNAUtils._trackingCamera.ProjectionMatrix;
                        break;
                    }
                default:
                    {
                        // We need a default here to make sure code doesn't complain about unassigned locals
                        // but we'll still throw an exception
                        viewMatrix = XNAUtils._trackingCamera.ViewMatrix;
                        projectionMatrix = XNAUtils._trackingCamera.ProjectionMatrix;
                        throw new NotSupportedException("Default camera not supported in WorldToLogical");
                    }
            }
            var nearWorldPoint = TheGame.SharedGame.GraphicsDevice.Viewport.Unproject(
                nearScreenPoint, projectionMatrix, viewMatrix, Matrix.Identity);
            var farWorldPoint = TheGame.SharedGame.GraphicsDevice.Viewport.Unproject(
                farScreenPoint, projectionMatrix, viewMatrix, Matrix.Identity);

            var pointerRayDirection = farWorldPoint - nearWorldPoint;
            pointerRayDirection.Normalize();
            var pointerRay = new Ray(nearWorldPoint, pointerRayDirection);

            var plane = new Plane(Vector3.Backward, desiredDepth);

            float distanceOnRay = RayPlaneIntersection(pointerRay, plane);

            Vector3 intersectionPoint = pointerRay.Position + (distanceOnRay * pointerRay.Direction);

            return intersectionPoint;
        }

        /// <summary>
        /// Create a matrix that can be used to scale a model to a desired size.
        /// </summary>
        /// <param name="screenPointStart">If by height, the bottom point on the screen the model will line up with, else
        ///                                the left point on the screen the model will line up with.
        /// </param>
        /// <param name="screenPointEnd">If by height, the top point on the screen the model will line up with, else
        ///                               the right poin on the screen the model will line up with.</param>
        /// <param name="desiredDepth">The depth from the origin this model will be positioned at.</param>
        /// <param name="modelLength">The original height or width of the model (unscaled)</param>
        /// <returns>A matrix ready to be applied as part of the world matrix for a model.
        /// Example: 
        /// // Parameters determined in code preceding this call
        /// var scaleMatrix = XNAUtils.ScaleModel(bottom, top, depth, _heroModel.Height);
        ///
        /// // Determine translation
        /// var translate = _camera.Position + new Vector3(0, 0, -8);
        /// var translateMatrix = Matrix.CreateTranslation(translate);
        ///
        /// // Determine world matrix 
        /// _heroModel.WorldMatrix = scaleMatrix * translateMatrix;
        /// </returns>
        public static Matrix ScaleModel(CCPoint screenPointStart,
                                        CCPoint screenPointEnd,
                                        float desiredDepth,
                                        float modelLength,
                                        ScaleModelBy scaleModelBy,
                                        CameraType cameraType)
        {
            Vector3 intersectionPointStart = LogicalToWorld(screenPointStart, desiredDepth, cameraType);
            Vector3 intersectionPointEnd = LogicalToWorld(screenPointEnd, desiredDepth, cameraType);

            // Determine scaling
            float desiredLength = 0.0f;
            if (scaleModelBy == ScaleModelBy.Height)
            {
                desiredLength= intersectionPointEnd.Y - intersectionPointStart.Y;
            }
            else
            {
                desiredLength = intersectionPointEnd.X - intersectionPointStart.X;
            }
            float scale = desiredLength / modelLength;
            var scaleMatrix = Matrix.CreateScale(scale);

            return scaleMatrix;
        }

        private static float RayPlaneIntersection(Ray ray, Plane plane)
        {
            float rayPointDis = -plane.DotNormal(ray.Position);
            float rayPointToPlaneDist = rayPointDis - plane.D;
            float directionProjectedLength = Vector3.Dot(plane.Normal, ray.Direction);
            float factor = rayPointToPlaneDist / directionProjectedLength;
            return factor;
        }

        private static VertexDeclaration posColVertexDeclaration;
        public static BoundingBox TransformBoundingBox(BoundingBox origBox, Matrix matrix)
        {
            Vector3 origCorner1 = origBox.Min;
            Vector3 origCorner2 = origBox.Max;

            Vector3 transCorner1 = Vector3.Transform(origCorner1, matrix);
            Vector3 transCorner2 = Vector3.Transform(origCorner2, matrix);

            return new BoundingBox(transCorner1, transCorner2);
        }

        public static BoundingSphere TransformBoundingSphere(BoundingSphere originalBoundingSphere, Matrix transformationMatrix)
        {
            Vector3 trans;
            Vector3 scaling;
            Quaternion rot;
            transformationMatrix.Decompose(out scaling, out rot, out trans);

            float maxScale = scaling.X;
            if (maxScale < scaling.Y)
                maxScale = scaling.Y;
            if (maxScale < scaling.Z)
                maxScale = scaling.Z;

            float transformedSphereRadius = originalBoundingSphere.Radius * maxScale;
            Vector3 transformedSphereCenter = Vector3.Transform(originalBoundingSphere.Center, transformationMatrix);

            BoundingSphere transformedBoundingSphere = new BoundingSphere(transformedSphereCenter, transformedSphereRadius);

            return transformedBoundingSphere;
        }

        public static Model LoadModelWithBoundingSphere(ref Matrix[] modelTransforms, string asset, ContentManager content)
        {
            Model newModel = content.Load<Model>(asset);

            modelTransforms = new Matrix[newModel.Bones.Count];
            newModel.CopyAbsoluteBoneTransformsTo(modelTransforms);

            BoundingSphere completeBoundingSphere = new BoundingSphere();
            foreach (ModelMesh mesh in newModel.Meshes)
            {
                BoundingSphere origMeshSphere = mesh.BoundingSphere;
                BoundingSphere transMeshSphere = XNAUtils.TransformBoundingSphere(origMeshSphere, modelTransforms[mesh.ParentBone.Index]);
                completeBoundingSphere = BoundingSphere.CreateMerged(completeBoundingSphere, transMeshSphere);
            }
            newModel.Tag = completeBoundingSphere;

            return newModel;
        }

        public static void DrawBoundingBox(BoundingBox bBox, GraphicsDevice device, BasicEffect basicEffect, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            // TODO: New VertexDeclaration syntax?
            if (posColVertexDeclaration == null)
            {
                posColVertexDeclaration = new VertexDeclaration(/* device, */
                    new VertexElement[] 
                    {
                        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                        new VertexElement(sizeof (float)*3,VertexElementFormat.Color, VertexElementUsage.Color,0)
                    }
                    /*VertexPositionColor.VertexElements*/);
            }

            Vector3 v1 = bBox.Min;
            Vector3 v2 = bBox.Max;

            VertexPositionColor[] cubeLineVertices = new VertexPositionColor[8];
            cubeLineVertices[0] = new VertexPositionColor(v1, Color.White);
            cubeLineVertices[1] = new VertexPositionColor(new Vector3(v2.X, v1.Y, v1.Z), Color.Red);
            cubeLineVertices[2] = new VertexPositionColor(new Vector3(v2.X, v1.Y, v2.Z), Color.Green);
            cubeLineVertices[3] = new VertexPositionColor(new Vector3(v1.X, v1.Y, v2.Z), Color.Blue);

            cubeLineVertices[4] = new VertexPositionColor(new Vector3(v1.X, v2.Y, v1.Z), Color.White);
            cubeLineVertices[5] = new VertexPositionColor(new Vector3(v2.X, v2.Y, v1.Z), Color.Red);
            cubeLineVertices[6] = new VertexPositionColor(v2, Color.Green);
            cubeLineVertices[7] = new VertexPositionColor(new Vector3(v1.X, v2.Y, v2.Z), Color.Blue);

            short[] cubeLineIndices = { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7 };

            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.VertexColorEnabled = true;
            
            // TODO: RenderState goes to Rasterizer state?
            // device.RenderState.FillMode = FillMode.Solid;
            device.RasterizerState.FillMode = FillMode.Solid;
            // basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                // pass.Begin();
                // device.VertexDeclaration = posColVertexDeclaration;
                device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList, cubeLineVertices, 0, 8, cubeLineIndices, 0, 12);
                // pass.End();
            }
            // basicEffect.End();
        }

        public static void DrawSphereSpikes(BoundingSphere sphere, GraphicsDevice device, BasicEffect basicEffect, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            // TODO: New VertexDeclaration syntax?
            if (posColVertexDeclaration == null)
            {
                posColVertexDeclaration = new VertexDeclaration(/* device, */
                    new VertexElement[] 
                    {
                        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                        new VertexElement(sizeof (float)*3,VertexElementFormat.Color, VertexElementUsage.Color,0)
                    }
                    /*VertexPositionColor.VertexElements*/);
            }

            Vector3 up = sphere.Center + sphere.Radius * Vector3.Up;
            Vector3 down = sphere.Center + sphere.Radius * Vector3.Down;
            Vector3 right = sphere.Center + sphere.Radius * Vector3.Right;
            Vector3 left = sphere.Center + sphere.Radius * Vector3.Left;
            Vector3 forward = sphere.Center + sphere.Radius * Vector3.Forward;
            Vector3 back = sphere.Center + sphere.Radius * Vector3.Backward;

            VertexPositionColor[] sphereLineVertices = new VertexPositionColor[6];
            sphereLineVertices[0] = new VertexPositionColor(up, Color.White);
            sphereLineVertices[1] = new VertexPositionColor(down, Color.White);
            sphereLineVertices[2] = new VertexPositionColor(left, Color.White);
            sphereLineVertices[3] = new VertexPositionColor(right, Color.White);
            sphereLineVertices[4] = new VertexPositionColor(forward, Color.White);
            sphereLineVertices[5] = new VertexPositionColor(back, Color.White);

            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.VertexColorEnabled = true;
            // basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                // pass.Begin();
                // device.VertexDeclaration = posColVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, sphereLineVertices, 0, 3);
                // pass.End();
            }
            // basicEffect.End();

        }

        public static VertexPositionColor[] VerticesFromVector3List(List<Vector3> pointList, Color color)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[pointList.Count];

            int i = 0;
            foreach (Vector3 p in pointList)
                vertices[i++] = new VertexPositionColor(p, color);

            return vertices;
        }

        public static BoundingBox CreateBoxFromSphere(BoundingSphere sphere)
        {
            float radius = sphere.Radius;
            Vector3 outerPoint = new Vector3(radius, radius, radius);

            Vector3 p1 = sphere.Center + outerPoint;
            Vector3 p2 = sphere.Center - outerPoint;

            return new BoundingBox(p1, p2);
        }
    }
}