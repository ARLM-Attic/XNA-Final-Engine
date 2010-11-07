
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
using System.IO;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    /// <summary>
    /// Generates a texture with the Z-Buffer and scene-normal map.
    /// It can generate also a single Z-Buffer texture with 32 bits single channel precision.
    /// </summary>
    public class PreDepthNormal : ScreenShader
    {

        #region Variables

        /// <summary>
        /// Z-Buffer and scene-normal map.
        /// </summary>
        private RenderToTexture normalDepthMapTexture;

        /// <summary>
        /// Single Z-Buffer texture with 32 bits single channel precision.
        /// </summary>
        private RenderToTexture highPrecisionDepthMapTexture;

		#endregion

        #region Properties

        /// <summary>
        /// Z-Buffer and scene-normal map.
        /// First three components are normals and the last is the z-buffer.
        /// If high precision is enable the last component doesn't have the z-buffer.
        /// </summary>
        public RenderToTexture NormalDepthMapTexture { get { return normalDepthMapTexture; } }

        /// <summary>
        /// Single Z-Buffer texture with 32 bits single channel precision.
        /// </summary>
        public RenderToTexture HighPrecisionDepthMapTexture { get { return highPrecisionDepthMapTexture; } }

        /// <summary>
        /// Does this texture use the 32 bits single channel high percision format?
        /// </summary>
        public bool UsesHighPrecisionFormat { get; private set; }

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private EffectParameter epFarPlane,
                                epNearPlane,
                                epWorldViewProj,
                                epWorldView,
                                epWorldViewIT;

        #region Matrices

        /// <summary>
        /// Last used transpose inverse world view matrix
        /// </summary>
        private static Matrix? lastUsedTransposeInverseWorldViewMatrix = null;
        /// <summary>
        /// Set transpose inverse world view matrix
        /// </summary>
        private Matrix TransposeInverseWorldViewMatrix
        {
            set
            {
                if (lastUsedTransposeInverseWorldViewMatrix != value)
                {
                    lastUsedTransposeInverseWorldViewMatrix = value;
                    epWorldViewIT.SetValue(value);
                } // if
            } // set
        } // TransposeInverseWorldViewMatrix

        /// <summary>
        /// Last used world view matrix
        /// </summary>
        private static Matrix? lastUsedWorldViewMatrix = null;
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
        private static Matrix? lastUsedWorldViewProjMatrix = null;
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
        private static float? lastUsedFarPlane = null;
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
        private static float? lastUsedNearPlane = null;
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
		/// Create the depth and normal shader.
        /// It generates a texture with the Z-Buffer and scene-normal map.
        /// It also can generate also a single Z-Buffer texture with 32 bits single channel precision.
		/// </summary>
        public PreDepthNormal(bool _usesHighPrecisionFormat = false, RenderToTexture.SizeType _rendeTargetSize = RenderToTexture.SizeType.FullScreen)			
		{
            UsesHighPrecisionFormat = _usesHighPrecisionFormat;
            
            Effect = LoadShader("PreDepthNormal");
                        
            // The final depth texture
            normalDepthMapTexture = new RenderToTexture(_rendeTargetSize, false);
            if (UsesHighPrecisionFormat)
            {
                highPrecisionDepthMapTexture = new RenderToTexture(_rendeTargetSize, true);
            }
           
            GetParametersHandles();
            FarPlane = ApplicationLogic.Camera.FarPlane;
            NearPlane = ApplicationLogic.Camera.NearPlane;
            LoadUITestElements();
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
                epWorldViewIT = Effect.Parameters["worldViewIT"];
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
            TransposeInverseWorldViewMatrix = Matrix.Transpose(Matrix.Invert(world * ApplicationLogic.Camera.ViewMatrix));
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
                SetAtributes(((GraphicObject)renderObjects).WorldMatrix);
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
        /// Base render algorithm.
        /// </summary>
        private void BaseRender(Object renderObjects, RenderToTexture resultTexture)
        {
            // Start rendering onto the depth map
            resultTexture.EnableRenderTarget();

            // Clear render target
            resultTexture.Clear(Color.White);
            
            // Start effect (current technique should be set)
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
            resultTexture.DisableRenderTarget();
        } // BaseRender

        /// <summary>
        /// It generates the depth map and normal map of the object given.
        /// If high precision format is used then the depth is store in a 32bits single channel texture.
		/// </summary>
        public void GenerateDepthNormalMap(Object renderObjects)
        {
            if (UsesHighPrecisionFormat)
            {
                Effect.CurrentTechnique = Effect.Techniques["OnlyNormals"];
            }
            else
            {
                Effect.CurrentTechnique = Effect.Techniques["DepthAndNormals"];
            }
            BaseRender(renderObjects, normalDepthMapTexture);
            
            if (UsesHighPrecisionFormat)
            {
                Effect.CurrentTechnique = Effect.Techniques["HighPresicionDepth"];
                BaseRender(renderObjects, highPrecisionDepthMapTexture);
            }
        } // GenerateDepthMap

		#endregion

        #region Test

        #region Variables

        private UISliderNumeric uiFarPlane;

        #endregion

        /// <summary>
        /// Load the different elements of the user interface needed to modify the shader.
        /// </summary>
        public void LoadUITestElements()
        {

            uiFarPlane = new UISliderNumeric("Far Plane", new Vector2(EngineManager.Width - 390, 110), 1, 300, 10, FarPlane);
        } // LoadUITestElements

        /// <summary>
        /// It allows modifying the shader/material in real time with the help of the engine UI for testing purposes.
        /// </summary>
        public void Test()
        {

            #region Reset Parameters

            // Si los parametros se han modificado es mejor tener los nuevos valores
            uiFarPlane.CurrentValue = FarPlane;

            #endregion

            Primitives.Draw2DSolidPlane(new Rectangle(EngineManager.Width - 400, 0, 400, EngineManager.Height), new Color(0.0f, 0.0f, 0.0f, 1f), new Color(0.0f, 0.0f, 0.0f, 0.0f));
            FontArial14.Render("Depth and Normal parameters", new Vector2(EngineManager.Width - 380, 40), Color.White);
            uiFarPlane.UpdateAndRender();

            FarPlane = uiFarPlane.CurrentValue;

        } // Test

        #endregion

    } // PreDepthNormal
} // XNAFinalEngine.GraphicElements
