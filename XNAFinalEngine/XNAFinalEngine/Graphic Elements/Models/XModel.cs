
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
                         Departamento de Ciencias e Ingeniería de la Computación - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using Microsoft.Xna.Framework.Content;
using System;
using System.IO;
using System.Drawing;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using System.Collections.Generic;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    /// <summary>
    /// .X Format (the DirectX format for mesh)
    /// There was skinning animation up and running. However I temporally removed this feature because it needs more work.
    /// </summary>
    public class XModel : Model
    {

        #region Variables

        /// <summary>
        /// Transform matrices for each mesh part, we only have to get them once if the model is not animated.
        /// </summary>
        public Matrix[] transforms = null; // TODO

        /// <summary>
        /// Calcula las matrices en el tiempo actual.
        /// </summary>
        //AnimationPlayer animationPlayer = null;

        //private List<XSIAnimationContent> Animations;
        //XSIAnimationData l_Animations;
        //private bool PlaybackStatus = false;
        //private int AnimationIndex = 0;
        //private int OldAnimationIndex = 0;
        //private float Blend = 1.0f;
        
        #endregion

        #region Properties

        /// <summary>
        /// Underlying xna model object. Loaded with the content system.
        /// </summary>
        public XnaModel XnaModel { get; private set; }

        #endregion

        #region Constructor
        /// <summary>
        /// Load the .x file
        /// </summary>
        public XModel(String _modelFilename)
        {
            ModelFilename = _modelFilename;
            string fullFilename = Directories.ModelsDirectory + "\\" + ModelFilename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load model: File " + ModelFilename + " does not exists!");
            } // if (File.Exists)

            if (EngineManager.UsesSystemContent)
                XnaModel = EngineManager.SystemContent.Load<XnaModel>(fullFilename);
            else
                XnaModel = EngineManager.CurrentContent.Load<XnaModel>(fullFilename);
            
            
            // Get matrices for each mesh part
            transforms = new Matrix[XnaModel.Bones.Count];
            XnaModel.CopyAbsoluteBoneTransformsTo(transforms);
            /*
            Animations = new List<XSIAnimationContent>();
            // post process animation
            l_Animations = xnaModel.Tag as XSIAnimationData;
            if (l_Animations != null)
            {
                foreach (KeyValuePair<String, XSIAnimationContent> AnimationClip in l_Animations.RuntimeAnimationContentDictionary)
                {
                    AnimationClip.Value.BindModelBones(xnaModel);
                    Animations.Add(AnimationClip.Value);
                }
                l_Animations.ResolveBones(xnaModel);
                Animations[0].Reset();
            }
            */
            /*
            // Look up our custom skinning information.
            SkinningData skinningData = xnaModel.Tag as SkinningData;

            if (skinningData != null)
            {
                // Create an animation player, and start decoding an animation clip.
                animationPlayer = new AnimationPlayer(skinningData);

                AnimationClip clip = skinningData.AnimationClips["Take 001"];//Biped

                animationPlayer.StartClip(clip);
            }
            */
        }

        #endregion

        #region Update

        /// <summary>
        /// Update skinning animations.
        /// </summary>
        public void Update()
        {
            /*
            if (PlaybackStatus)
            {
                if (Animations.Count > 0)
                {
                    if (Blend < 1.0f)
                    {
                        Animations[OldAnimationIndex].PlayBack(TimeSpan.Parse("0"), 1.0f);
                        Blend += 0.1f;
                    }
                    else
                    {
                        OldAnimationIndex = AnimationIndex;
                    }
                    Animations[AnimationIndex].PlayBack(DeviceManager.GameTime.ElapsedGameTime, Blend);
                }
            }
            // post process animation
            l_Animations.ComputeBoneTransforms(transforms);
            transforms = l_Animations.BoneTransforms;
            */
            /*
            if (Animations != null && Animations.Count > 0)
            {
                Animations[0].PlayBack(DeviceManager.GameTime.ElapsedGameTime, 0.1f);
            }
            l_Animations.ComputeBoneTransforms(transforms);
            transforms = l_Animations.BoneTransforms;
            */
            /*
            if (animationPlayer != null)
            {   
                animationPlayer.Update(DeviceManager.GameTime.ElapsedGameTime, true, Matrix.Identity);
                transforms = animationPlayer.GetSkinTransforms();
            }*/
        } // Update

        #endregion

        #region Render

        /// <summary>
        /// Render the model.
        /// </summary>
        public override void Render()
        {   
            // Go through all meshes in the model
            foreach (ModelMesh mesh in XnaModel.Meshes)
			{
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {   
                    // Tenemos dos posibilidades. Pasarle el effecto (o shader) al modelo y que lo renderice.
                    // O realizar nosotros la renderizacion manualmente. Esta ultima respeta el esquema clasico y permite tener mas poder.

                    ModelMeshPart part = mesh.MeshParts[i];
                                    
                    // Set vertex buffer and index buffer
                    EngineManager.Device.SetVertexBuffer(part.VertexBuffer);
                    EngineManager.Device.Indices = part.IndexBuffer;

                    // And render all primitives
                    EngineManager.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                }
            }
        } // Render

        #endregion

        #region Vertices

        /// <summary>
        /// Get the vertices' positions of the model.
        /// </summary>
        /// <param name="worldMatrix">Assosiated world matrix</param>
        /// <returns>The list of vertices' positons</returns>
        protected override List<Vector3> Vertices(Matrix worldMatrix)
        {
            List<Vector3> vertexs = new List<Vector3>();

            foreach (ModelMesh mesh in XnaModel.Meshes)
            {
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    ModelMeshPart part = mesh.MeshParts[i];
                    VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[part.VertexBuffer.VertexCount];

                    part.VertexBuffer.GetData<VertexPositionNormalTexture>(vertices);

                    for (int index = 0; index < vertices.Length; index++)
                    {
                        if (worldMatrix != Matrix.Identity) // Optimization
                        {
                            vertices[index].Position = Vector3.Transform(vertices[index].Position, worldMatrix);
                        }
                        vertexs.Add(vertices[index].Position);
                    }
                }
            }
            return vertexs;
        } // Vertices

        #endregion

    } // XModel
} // XNAFinalEngine.GraphicElements