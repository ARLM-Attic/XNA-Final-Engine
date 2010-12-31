
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
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.UI;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// This shader generates a depth and normal map.
    /// It has two variants:
    /// 
    /// * A simple version that stores in one texture (normalMapTexture) the normals (rgb) and the depth (alpha).
    /// 
    /// * And a improve version that stores in two textures the normals (normalMapTexture) and the depth (depthMapTexture).
    ///   The depth texture has a texture with a 32 bits single channel precision, and the normal has a RGBA1010102 format,
    ///   and these normals are store in the range [0,1] to avoid negative values.
    /// 
    /// This last option is the recommended.
    /// </summary>
    public class PreDepthNormal : ScreenShader
    {

        #region Variables

        /// <summary>
        /// Normal Map. It can store the Z-Buffer if high precision mode is off.
        /// </summary>
        public readonly RenderToTexture normalMapTexture;

        /// <summary>
        /// Single Z-Buffer texture with 32 bits single channel precision.
        /// </summary>
        private readonly RenderToTexture depthMapTexture;

		#endregion

        #region Properties

        /// <summary>
        /// Normal Map. It can store the Z-Buffer if high precision mode is off.
        /// The normals are store in the [0, 1] range.
        /// </summary>
        public RenderToTexture NormalMapTexture { get { return normalMapTexture; } }

        /// <summary> 
        /// Single Z-Buffer texture with 32 bits single channel precision.
        /// </summary>
        public RenderToTexture DepthMapTexture { get { return depthMapTexture; } }

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private EffectParameter epFarPlane,
                                epNearPlane,
                                epWorldViewProj,
                                epWorldView,
                                epWorldIT;

        #region Matrices

        /// <summary>
        /// Last used transpose inverse world view matrix
        /// </summary>
        private static Matrix? lastUsedTransposeInverseWorldViewMatrix;
        /// <summary>
        /// Set transpose inverse world view matrix
        /// </summary>
        private Matrix TransposeInverseWorldMatrix
        {
            set
            {
                if (lastUsedTransposeInverseWorldViewMatrix != value)
                {
                    lastUsedTransposeInverseWorldViewMatrix = value;
                    epWorldIT.SetValue(value);
                } // if
            } // set
        } // TransposeInverseWorldViewMatrix

        /// <summary>
        /// Last used world view matrix
        /// </summary>
        private static Matrix? lastUsedWorldViewMatrix;
        /// <summary>
        /// Set world view matrix
        /// </summary>
        private Matrix WorldViewMatrix
        {
            set
            {
                if (lastUsedWorldViewMatrix != value)
                {
                    lastUsedWorldViewMatrix = value;
                    epWorldView.SetValue(value);
                } // if
            } // set
        } // WorldViewMatrix

        /// <summary>
        /// Last used world view projection matrix
        /// </summary>
        private static Matrix? lastUsedWorldViewProjMatrix;
        /// <summary>
        /// Set world view projection matrix
        /// </summary>
        private Matrix WorldViewProjMatrix
        {
            set
            {
                if (lastUsedWorldViewProjMatrix != value)
                {
                    lastUsedWorldViewProjMatrix = value;
                    epWorldViewProj.SetValue(value);
                } // if
            } // set
        } // WorldViewProjMatrix

        #endregion

        #region Far Plane

        /// <summary>
        /// Far Plane
        /// </summary>
        private float farPlane = 100.0f;

        /// <summary>
        /// Far Plane
        /// </summary>
        public float FarPlane
        {
            get { return farPlane; }
            set { farPlane = value; }
        } // FarPlane

        /// <summary>
        /// Last used far plane
        /// </summary>
        private static float? lastUsedFarPlane;
        /// <summary>
        /// Set Far Plane (greater or equal to 1)
        /// </summary>
        private void SetFarPlane(float _farPlane)
        {
            if (lastUsedFarPlane != _farPlane && _farPlane >= 1.0f)
            {
                lastUsedFarPlane = _farPlane;
                epFarPlane.SetValue(_farPlane);
            }
        } // SetFarPlane

        #endregion

        #region Near Plane

        /// <summary>
        /// Near Plane
        /// </summary>
        private float nearPlane = 1.0f;

        /// <summary>
        /// Near Plane
        /// </summary>
        public float NearPlane
        {
            get { return nearPlane; }
            set { nearPlane = value; }
        } // NearPlane

        /// <summary>
        /// Last used near plane
        /// </summary>
        private static float? lastUsedNearPlane;
        /// <summary>
        /// Set Near Plane (greater or equal to 1)
        /// </summary>
        private void SetNearPlane(float _nearPlane)
        {
            if (lastUsedNearPlane != _nearPlane && _nearPlane >= 1.0f)
            {
                lastUsedNearPlane = _nearPlane;
                epNearPlane.SetValue(_nearPlane);
            }
        } // SetNearPlane

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// This shader generates a depth and normal map.
        /// It has two variants:
        /// 
        /// * A simple version that stores in one texture (normalMapTexture) the normals (rgb) and the depth (alpha).
        /// 
        /// * And a improve version that stores in two textures the normals (normalMapTexture) and the depth (depthMapTexture).
        ///   The depth texture has a texture with a 32 bits single channel precision, and the normal has a RGBA1010102 format,
        ///   and these normals are store in the range [0,1] to avoid negative values.
        /// 
        /// This last option is the recommended.
		/// </summary>
        public PreDepthNormal(RenderToTexture.SizeType _rendeTargetSize = RenderToTexture.SizeType.FullScreen)			
		{

            
            Effect = LoadShader("PreDepthNormal");
                                  
            depthMapTexture = new RenderToTexture(_rendeTargetSize, true);
            normalMapTexture = new RenderToTexture(_rendeTargetSize, SurfaceFormat.HalfVector2);
           
            GetParametersHandles();

            FarPlane = ApplicationLogic.Camera.FarPlane;
            NearPlane = ApplicationLogic.Camera.NearPlane;
		} // PreDepthShader

		#endregion

		#region Get parameters handles

		/// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected void GetParametersHandles()
		{	
            try
			{
                // Matrices //
                epWorldViewProj = Effect.Parameters["worldViewProj"];
                epWorldView = Effect.Parameters["worldView"];
                epWorldIT = Effect.Parameters["worldIT"];
                // Others //
                epFarPlane = Effect.Parameters["FarPlane"];
                epNearPlane = Effect.Parameters["NearPlane"];
            }
            catch
            {
                throw new Exception("Get the handles from the depth and normal shader failed.");
            }
		} // GetParameters

		#endregion

        #region SetAtributes

        /// <summary>
        /// Setea los atributos en el shader
        /// </summary>
        public virtual void SetAtributes(Matrix world)
        {
            // Initialization of the Matrices
            TransposeInverseWorldMatrix = Matrix.Transpose(Matrix.Invert(world));
            WorldViewMatrix = world * ApplicationLogic.Camera.ViewMatrix;
            WorldViewProjMatrix = world * ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix;
            // Inicialization of the far clip
            SetFarPlane(FarPlane);
            SetNearPlane(NearPlane);
            Effect.CurrentTechnique.Passes[0].Apply(); // Commit Changes
        } // SetAtributes

        #endregion

        #region Generate Depth Map

        /// <summary>
        /// Render the object without taking care of the illumination information.
        /// </summary>
        private void RenderObjects(Object renderObjects)
        {
            if (renderObjects is GraphicObject)
            {
                // Set parameters
                SetAtributes(renderObjects.WorldMatrix);
                // Render
                ((GraphicObject)renderObjects).Model.Render();
            }
            else // if is a container object
            {
                foreach (GraphicObject graphicObj in ((ContainerObject)renderObjects).GraphicObjectsChildren)
                {
                    RenderObjects(graphicObj);
                }
                foreach (ContainerObject containerObject in ((ContainerObject)renderObjects).ContainerObjectsChildren)
                {
                    RenderObjects(containerObject);
                }
            }
        } // RenderObjects

        /// <summary>
        /// It generates the depth map and normal map of the object given.
        /// If high precision format is used then the depth is store in a 32bits single channel texture.
		/// </summary>
        public void GenerateDepthNormalMap(Object renderObjects)
        {
            Effect.CurrentTechnique = Effect.Techniques["DepthAndNormals"];
            // Start rendering onto the depth map
            RenderToTexture.EnableRenderTargets(depthMapTexture, normalMapTexture);
                
            // Clear render target
            EngineManager.ClearTargetAndDepthBuffer(Color.White);
            
            // Start effect (current technique should be set));
            try
            {
                Effect.CurrentTechnique.Passes[0].Apply();
                    
                RenderObjects(renderObjects);
                    
            } // try
            catch (Exception e)
            {
                throw new Exception("Unable to render the depth normal effect " + e.Message);
            }
            // Resolve the render target to get the texture
            RenderToTexture.DisableMultipleRenderTargets();
        } // GenerateDepthMap

		#endregion

    } // PreDepthNormal
} // XNAFinalEngine.Graphics
