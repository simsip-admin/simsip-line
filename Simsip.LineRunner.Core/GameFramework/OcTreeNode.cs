using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Effects.Deferred;
using Simsip.LineRunner.GameObjects;
using Simsip.LineRunner.Utils;
using System.Collections.Generic;


namespace Simsip.LineRunner.GameFramework
{
    public class OcTreeNode
    {
        // Once we reach this count of models in one of the 8 child nodes we
        // are parent for, we will subdivide that child node into 8 grandchildren 
        // and distribute the current child's models into the grandchildren -
        // subdividing those also as needed.
        private const int _maxObjectsInNode = 5;

        // We will only break down the size of a node to this value.
        private const float _minSize = 5f;

        // The center of this node
        private Vector3 _center;

        // The size of this node. The width, height and depth is 1/2 this amount.
        private float _size;
        
        // The models contained in this node.
        List<GameModel> _modelList;

        // The bounding box for this node which will be centered around _center
        // and will be _size in width, height and depth
        private BoundingBox _nodeBoundingBox;

        // The 8 child nodes we are direct parent of
        private OcTreeNode _nodeUFL; // Upper Front Left
        private OcTreeNode _nodeUFR; // Upper Front Right
        private OcTreeNode _nodeUBL; // Upper Back Left
        private OcTreeNode _nodeUBR; // Upper Back Right
        private OcTreeNode _nodeDFL; // Down Front Left
        private OcTreeNode _nodeDFR; // Down Front Right
        private OcTreeNode _nodeDBL; // Down Back Left
        private OcTreeNode _nodeDBR; // Down Back Right

        // The 8 child nodes we are direct parent of
        List<OcTreeNode> _childList;

        // For debugging, the count of models stored across all nodes
        private static int _modelsStoredInQuadTree;


        public OcTreeNode(Vector3 center, float size)
        {
            // Where are we located?
            this._center = center;

            // How big is each side of our bounding box?
            this._size = size;

            // Initalize empty model and child node collections
            this._modelList = new List<GameModel>();
            this._childList = new List<OcTreeNode>(8);

            // Construct our bounding box centered around _center and
            // _size in width, height and depth
            var diagonalVector = new Vector3(size / 2f, size / 2f, size / 2f);
            this._nodeBoundingBox = new BoundingBox(center - diagonalVector, center + diagonalVector);
        }

        #region Properties

        /// <summary>
        /// TODO: We use a default of twice our typical world size of 150.
        /// </summary>
        public static float DefaultSize = 1000f;

        // For debugging, the count of models drawn across all nodes
        public static int _modelsDrawn;

        public int ModelsDrawn 
        { 
            get 
            { 
                return OcTreeNode._modelsDrawn; 
            } 
            set 
            { 
                OcTreeNode._modelsDrawn = value; 
            } 
        }

        #endregion

        #region Api

        /// <summary>
        /// Add a model to the collection maintained by this OcTreeNode.
        /// </summary>
        /// <param name="model">The model to add.</param>
        public void AddModel(GameModel model)
        {
            // Have we subdivided into children yet?
            // (still just this one node containing all models it is responsible for)
            if (this._childList.Count == 0)
            {
                // Ok, we have not subdivided proceed to simply add the model
                this._modelList.Add(model);

                // Now, let's determine if we need/can subdivide
                bool maxObjectsReached = (this._modelList.Count > OcTreeNode._maxObjectsInNode);
                bool minSizeNotReached = (this._size > OcTreeNode._minSize);
                if (maxObjectsReached && 
                    minSizeNotReached)
                {
                    // Ok, we need and can subvidive, proceed to create our 
                    // 8 child nodes
                    CreateChildNodes();
                    
                    // Loop over our current set of models
                    foreach (var modelToDistribute in this._modelList)
                    {
                        // And distribute each model to an appropriate child node
                        this.Distribute(modelToDistribute);
                    }

                    // We no longer have a set of models to manage directly, 
                    // they have been distributed to our child nodes, so
                    // clear out our collection.
                    this._modelList.Clear();
                }
            }
            else
            {
                // Ok, we have already subdived so we no longer directly manage a set
                // of models - this is handled by our child nodes.
                this.Distribute(model);
            }
        }

        /// <summary>
        /// Remove a model from the collection maintained by this OcTreeNode.
        /// </summary>
        /// <param name="modelID">The id of the model to remove.</param>
        /// <returns>The model removed or null if not found.</returns>
        public GameModel RemoveModel(int modelID)
        {
            GameModel modelToRemove = null;

            // First loop over any models we are directly responsible for
            for (int i = 0; i < this._modelList.Count; i++)
            {
                // Did we get a hit?
                if (this._modelList[i].ModelID == modelID)
                {
                    // Proceed to remove
                    modelToRemove = this._modelList[i];
                    this._modelList.Remove(modelToRemove);
                }
            }

            // Now, recursivly loop over our children
            int child = 0;
            while ((modelToRemove == null) && 
                   (child < this._childList.Count))
            {
                // If we get a hit, our loop logic will short circuit
                modelToRemove = this._childList[child++].RemoveModel(modelID);
            }

            // Return model we removed or null if not found
            return modelToRemove;
        }

        /// <summary>
        /// Update the world matrix for a model in this OcTreeNode's collection.
        /// </summary>
        /// <param name="modelID">The model id corresponding to the model to update for.</param>
        /// <param name="newWorldMatrix">The new world matrix for this model.</param>
        public void UpdateModelWorldMatrix(int modelID, Matrix newWorldMatrix)
        {
            // First remove the model from our hierarchy of nodes
            var removedModel = RemoveModel(modelID);

            // Now update the model's world matrix
            // TODO: Not needed as already done via our Box2DModel implementations
            // removedModel.WorldMatrix = newWorldMatrix;
            
            // Finally, add the model back in. The Add()/Distribute() logic will take of
            // placing the model into the correct node.
            AddModel(removedModel);
        }

        /// <summary>
        /// Perform a filtered draw of the contents of this OcTreeNode
        /// </summary>
        /// <param name="viewMatrix"></param>
        /// <param name="projectionMatrix"></param>
        /// <param name="cameraFrustrum"></param>
        /// <param name="effect"></param>
        /// <param name="type"></param>
        public void Draw(Matrix viewMatrix, 
                         Matrix projectionMatrix, 
                         BoundingFrustum cameraFrustrum,
                         Effect effect=null,
                         EffectType type=EffectType.None)
        {
            // To what extent does the camera frustrum intersect our bounding box?
            var cameraNodeContainment = cameraFrustrum.Contains(this._nodeBoundingBox);

            // Do we completely contain or partially overlap?
            if (cameraNodeContainment != ContainmentType.Disjoint)
            {
                // Ok, we passed our filter, let's draw our directly managed models (if any)
                foreach (GameModel model in this._modelList)
                {
                    model.Draw(viewMatrix, projectionMatrix, effect, type);
                    OcTreeNode._modelsDrawn++;
                }
                
                // Now recursively loop over our child nodes (if any) and perform
                // the same filtered draw on them
                foreach (OcTreeNode childNode in this._childList)
                {
                    childNode.Draw(viewMatrix, 
                                   projectionMatrix, 
                                   cameraFrustrum, 
                                   effect,
                                   type);
                }
            }
        }

        public void DrawBoxLines(Matrix viewMatrix, Matrix projectionMatrix, GraphicsDevice device, BasicEffect basicEffect)
        {
            foreach (OcTreeNode childNode in _childList)
                childNode.DrawBoxLines(viewMatrix, projectionMatrix, device, basicEffect);

            if (_childList.Count == 0)
                XNAUtils.DrawBoundingBox(_nodeBoundingBox, device, basicEffect, Matrix.Identity, viewMatrix, projectionMatrix);
        }

        #endregion

        #region Helper methods

        // Our criteria for creating child nodes has been reached - proceed
        // to create 8 child nodes.
        private void CreateChildNodes()
        {
            // This will represent the child nodes size. Hence, if we are dividing this cube
            // into 8 cubes, the child's size will be 1/2 of our size.
            float sizeOver2 = _size / 2.0f;

            // This will represent the offset used to place the center of the child.
            float sizeOver4 = _size / 4.0f;

            // Create the 8 child nodes.
            this._nodeUFR = new OcTreeNode(_center + new Vector3(sizeOver4, sizeOver4, -sizeOver4), sizeOver2);
            this._nodeUFL = new OcTreeNode(_center + new Vector3(-sizeOver4, sizeOver4, -sizeOver4), sizeOver2);
            this._nodeUBR = new OcTreeNode(_center + new Vector3(sizeOver4, sizeOver4, sizeOver4), sizeOver2);
            this._nodeUBL = new OcTreeNode(_center + new Vector3(-sizeOver4, sizeOver4, sizeOver4), sizeOver2);
            this._nodeDFR = new OcTreeNode(_center + new Vector3(sizeOver4, -sizeOver4, -sizeOver4), sizeOver2);
            this._nodeDFL = new OcTreeNode(_center + new Vector3(-sizeOver4, -sizeOver4, -sizeOver4), sizeOver2);
            this._nodeDBR = new OcTreeNode(_center + new Vector3(sizeOver4, -sizeOver4, sizeOver4), sizeOver2);
            this._nodeDBL = new OcTreeNode(_center + new Vector3(-sizeOver4, -sizeOver4, sizeOver4), sizeOver2);

            // Add the 8 child nodes to our children collection
            this._childList.Add(_nodeUFR);
            this._childList.Add(_nodeUFL);
            this._childList.Add(_nodeUBR);
            this._childList.Add(_nodeUBL);
            this._childList.Add(_nodeDFR);
            this._childList.Add(_nodeDFL);
            this._childList.Add(_nodeDBR);
            this._childList.Add(_nodeDBL);
        }

        // Given a Box2DModel with a proper WorldOrigin representing its
        // position, place the model into an appropriate child node.
        private void Distribute(GameModel model)
        {
            // What is the position for the model we want to place
            // into an appropriate child node?
            Vector3 position = model.WorldOrigin;

            //
            // Now that we know the model's position, and we know 
            // the position of our _center, determine which child
            // node the model should be added to.
            //

            if (position.Y > this._center.Y)            // Up
            {
                if (position.Z < this._center.Z)        // Forward
                {
                    if (position.X < this._center.X)    // Left
                    {
                        this._nodeUFL.AddModel(model);
                    }
                    else                                // Right
                    {
                        this._nodeUFR.AddModel(model);
                    }
                }
                else                                    // Back
                {
                    if (position.X < this._center.X)    // Left
                    {
                        this._nodeUBL.AddModel(model);
                    }
                    else                                // Right
                    {
                        this._nodeUBR.AddModel(model);
                    }
                }
            }
            else                                        // Down
            {
                if (position.Z < this._center.Z)        // Forward
                {
                    if (position.X < this._center.X)    // Left
                    {
                        this._nodeDFL.AddModel(model);
                    }
                    else                                // Right
                    {
                        this._nodeDFR.AddModel(model);
                    }
                }
                else                                    // Back
                {
                    if (position.X < this._center.X)    // Left
                    {
                        this._nodeDBL.AddModel(model);
                    }
                    else                                // Right
                    {
                        this._nodeDBR.AddModel(model);
                    }
                }
            }
        }

        #endregion
    }

}