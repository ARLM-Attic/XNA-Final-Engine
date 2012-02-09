
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Assets;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// This shader generates a depth map, a normal map, a specular power map and a motion vectors map.
    /// It stores the result in several render targets (depth texture, normal texture, and motion vector and specular power texture).
    /// The depth texture has a surface format of 32 bits single channel precision, and the normal has a half vector 2 format (r16f g16f). 
    /// The normals are store with spherical coordinates and the depth is store using the equation: -DepthVS / FarPlane.
    /// </summary>
    internal class GBufferShader : Shader
    {

        #region Variables

        private Matrix viewMatrix, projectionMatrix;

        // Singleton reference.
        private static GBufferShader instance;
        
        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a G-Buffer shader.
        /// </summary>
        public static GBufferShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new GBufferShader();
                return instance;
            }
        } // Instance

        #endregion
        
        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter epFarPlane,
                                       epWorldViewProj,
                                       epWorldView,
                                       epWorldViewIT,
                                       epObjectNormalTexture,
                                       epSpecularPower,
                                       epSpecularTextured,
                                       epObjectSpecularTexture,
                                       // Parallax
                                       epObjectNormalTextureSize,
                                       epLODThreshold,
                                       epMinimumNumberSamples,
                                       epMaximumNumberSamples,
                                       epHeightMapScale,
                                       // Skinning
                                       epBones,
                                       // Terrain
                                       epUvRectangleMin,
                                       epUvRectangleSide,
                                       epFarTerrainBeginDistance,
                                       epFlatRange,
                                       epDisplacementTexture,
                                       epFarTerrain;

        
        #region Transpose Inverse World View Matrix

        private static Matrix? lastUsedTransposeInverseWorldViewMatrix;
        private static void SetTransposeInverseWorldViewMatrix(Matrix transposeInverseWorldViewMatrix)
        {
            if (lastUsedTransposeInverseWorldViewMatrix != transposeInverseWorldViewMatrix)
            {
                lastUsedTransposeInverseWorldViewMatrix = transposeInverseWorldViewMatrix;
                epWorldViewIT.SetValue(transposeInverseWorldViewMatrix);
            }
        } // SetTransposeInverseWorldViewMatrix

        #endregion

        #region World View Matrix

        private static Matrix? lastUsedWorldViewMatrix;
        private static void SetWorldViewMatrix(Matrix worldViewMatrix)
        {
            if (lastUsedWorldViewMatrix != worldViewMatrix)
            {
                lastUsedWorldViewMatrix = worldViewMatrix;
                epWorldView.SetValue(worldViewMatrix);
            }
        } // SetWorldViewMatrix

        #endregion

        #region World View Projection Matrix

        private static Matrix? lastUsedWorldViewProjMatrix;
        private static void SetWorldViewProjMatrix(Matrix worldViewProjMatrix)
        {
            if (lastUsedWorldViewProjMatrix != worldViewProjMatrix)
            {
                lastUsedWorldViewProjMatrix = worldViewProjMatrix;
                epWorldViewProj.SetValue(worldViewProjMatrix);
            }
        } // SetWorldViewProjMatrix

        #endregion

        #region Far Plane

        private static float lastUsedFarPlane;
        private static void SetFarPlane(float _farPlane)
        {
            if (lastUsedFarPlane != _farPlane)
            {
                lastUsedFarPlane = _farPlane;
                epFarPlane.SetValue(_farPlane);
            }
        } // SetFarPlane

        #endregion

        #region Object Normal Texture (and size)

        private static Texture2D lastUsedObjectNormalTextureTexture;
        private static void SetObjectNormalTexture(Texture objectNormalTexture)
        {
            EngineManager.Device.SamplerStates[0] = SamplerState.AnisotropicWrap;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedObjectNormalTextureTexture != objectNormalTexture.Resource)
            {
                lastUsedObjectNormalTextureTexture = objectNormalTexture.Resource;
                epObjectNormalTextureSize.SetValue(new Vector2(objectNormalTexture.Width, objectNormalTexture.Height));
                epObjectNormalTexture.SetValue(objectNormalTexture.Resource);
            }
        } // SetObjectNormalTexture

        #endregion

        #region LOD Threshold

        private static int lastUsedLODThreshold;
        private static void SetLODThreshold(int lodThreshold)
        {
            if (lastUsedLODThreshold != lodThreshold)
            {
                lastUsedLODThreshold = lodThreshold;
                epLODThreshold.SetValue(lodThreshold);
            }
        } // SetLODThreshold

        #endregion

        #region Minimum Number Samples

        private static int lastUsedMinimumNumberSamples;
        private static void SetMinimumNumberSamples(int minimumNumberSamples)
        {
            if (lastUsedMinimumNumberSamples != minimumNumberSamples)
            {
                lastUsedMinimumNumberSamples = minimumNumberSamples;
                epMinimumNumberSamples.SetValue(minimumNumberSamples);
            }
        } // SetMinimumNumberSamples

        #endregion

        #region Maximum Number Samples

        private static int lastUsedMaximumNumberSamples;
        private static void SetMaximumNumberSamples(int maximumNumberSamples)
        {
            if (lastUsedMaximumNumberSamples != maximumNumberSamples)
            {
                lastUsedMaximumNumberSamples = maximumNumberSamples;
                epMaximumNumberSamples.SetValue(maximumNumberSamples);
            }
        } // SetMaximumNumberSamples

        #endregion

        #region Height Map Scale

        private static float lastUsedHeightMapScale;
        private static void SetHeightMapScale(float heightMapScale)
        {
            if (lastUsedHeightMapScale != heightMapScale)
            {
                lastUsedHeightMapScale = heightMapScale;
                epHeightMapScale.SetValue(heightMapScale);
            }
        } // SetHeightMapScale

        #endregion

        #region Specular Power

        private static float? lastUsedSpecularPower;
        private static void SetSpecularPower(float specularPower)
        {
            if (lastUsedSpecularPower != specularPower)
            {
                lastUsedSpecularPower = specularPower;
                epSpecularPower.SetValue(specularPower);
            }
        } // SetSpecularPower

        #endregion

        #region Specular Textured

        private static bool lastUsedSpecularTextured;
        private static void SetSpecularTextured(bool specularTextured)
        {
            if (lastUsedSpecularTextured != specularTextured)
            {
                lastUsedSpecularTextured = specularTextured;
                epSpecularTextured.SetValue(specularTextured);
            }
        } // SetSpecularTextured

        #endregion

        #region Specular Texture

        private static Texture2D lastUsedSpecularTexture;
        private static void SetSpecularTexture(Texture specularTexture)
        {
            EngineManager.Device.SamplerStates[1] = SamplerState.LinearWrap;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedSpecularTexture != specularTexture.Resource)
            {
                lastUsedSpecularTexture = specularTexture.Resource;
                epObjectSpecularTexture.SetValue(specularTexture.Resource);
            }
        } // SetSpecularTexture

        #endregion

        #region UvRectangle

        private static RectangleF? lastUsedUvRectangle;
        private static void SetUvRectangle(RectangleF uvRectangle)
        {
            if (lastUsedUvRectangle != uvRectangle)
            {
                lastUsedUvRectangle = uvRectangle;
                epUvRectangleMin.SetValue(new Vector2(uvRectangle.X, uvRectangle.Y));
                epUvRectangleSide.SetValue(new Vector2(uvRectangle.Width, uvRectangle.Height));
            }
        } // SetUvRectangle

        #endregion

        #region Far Terrain Begin Distance

        private static float lastUsedFarTerrainBeginDistance;
        private static void SetFarTerrainBeginDistance(float farTerrainBeginDistance)
        {
            if (lastUsedFarTerrainBeginDistance != farTerrainBeginDistance)
            {
                lastUsedFarTerrainBeginDistance = farTerrainBeginDistance;
                epFarTerrainBeginDistance.SetValue(farTerrainBeginDistance);
            }
        } // SetFarTerrainBeginDistance

        #endregion

        #region Flat Range

        private static float lastUsedFlatRange;
        private static void SetFlatRange(float flatRange)
        {
            if (lastUsedFlatRange != flatRange)
            {
                lastUsedFlatRange = flatRange;
                epFlatRange.SetValue(flatRange);
            }
        } // SetFlatRange

        #endregion

        #region Displacement Texture

        private static Texture2D lastUsedDisplacementTexture;
        private static void SetDisplacementTexture(Texture displacementTexture)
        {
            EngineManager.Device.SamplerStates[2] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedDisplacementTexture != displacementTexture.Resource)
            {
                lastUsedDisplacementTexture = displacementTexture.Resource;
                epDisplacementTexture.SetValue(displacementTexture.Resource);
            }
        } // SetDisplacementTexture

        #endregion

        #region Far Terrain

        private static bool lastUsedFarTerrain;
        private static void SetFarTerrain(bool farTerrain)
        {
            if (lastUsedFarTerrain != farTerrain)
            {
                lastUsedFarTerrain = farTerrain;
                epFarTerrain.SetValue(farTerrain);
            }
        } // SetFarTerrain

        #endregion

        #region Bones

        //private static Matrix[] lastUsedBones;
        private static void SetBones(Matrix[] bones)
        {
            // The values are probably different and the operation is costly and garbage prone (but this can be avoided).
            /*if (!ArrayHelper.Equals(lastUsedBones, bones))
            {
                lastUsedBones = (Matrix[])(bones.Clone());
                epBones.SetValue(bones);
            }*/
            epBones.SetValue(bones);
        } // SetBones

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// This shader generates a depth and normal map.
        /// It also generates a special buffer with motion vectors for motion blur and the specular power of the material.
        /// It stores the result in two textures, the normals (normalMapTexture) and the depth (depthMapTexture).
        /// The depth texture has a texture with a 32 bits single channel precision, and the normal has a half vector 2 format (r16f g16f). 
        /// The normals are store with spherical coordinates and the depth is store using the equation: -DepthVS / FarPlane.
        /// </summary>
        internal GBufferShader() : base("GBuffer\\GBuffer") { }

        #endregion

        #region Get Parameters Handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        /// <remarks>
        /// Creating and assigning a EffectParameter instance for each technique in your Effect is significantly faster than using the Parameters indexed property on Effect.
        /// </remarks>
        protected override void GetParametersHandles()
        {
            try
            {
                // Matrices //
                epWorldViewProj           = Resource.Parameters["worldViewProj"];
                epWorldView               = Resource.Parameters["worldView"];
                epWorldViewIT             = Resource.Parameters["worldViewIT"];
                // Others //
                epFarPlane                = Resource.Parameters["farPlane"];
                epObjectNormalTexture     = Resource.Parameters["objectNormalTexture"];
                epSpecularPower           = Resource.Parameters["specularPower"];
                epObjectSpecularTexture   = Resource.Parameters["objectSpecularTexture"];
                epSpecularTextured        = Resource.Parameters["specularTextured"];
                // Parallax //
                epObjectNormalTextureSize = Resource.Parameters["objectNormalTextureSize"];
                epLODThreshold            = Resource.Parameters["LODThreshold"];
                epMinimumNumberSamples    = Resource.Parameters["minimumNumberSamples"];
                epMaximumNumberSamples    = Resource.Parameters["maximumNumberSamples"];
                epHeightMapScale          = Resource.Parameters["heightMapScale"];
                // Skinning //
                epBones                   = Resource.Parameters["Bones"];
                // Terrain //
                epUvRectangleMin          = Resource.Parameters["uvRectangleMin"];
                epUvRectangleSide         = Resource.Parameters["uvRectangleSide"];
                epFarTerrainBeginDistance = Resource.Parameters["farTerrainBeginDistance"];
                epFlatRange               = Resource.Parameters["flatRange"];
                epDisplacementTexture     = Resource.Parameters["displacementTexture"];
                epFarTerrain              = Resource.Parameters["farTerrain"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Begin

        /// <summary>
        /// Begins the G Buffer rendering.
        /// </summary>
        internal void Begin(Matrix _viewMatrix, Matrix _projectionMatrix, float farPlane)
        {
            try
            {
                viewMatrix = _viewMatrix;
                projectionMatrix = _projectionMatrix;
                SetFarPlane(farPlane);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Directional Light:: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render Model

        /// <summary>
        /// Render a model into the GBuffer.
        /// </summary>
        internal void RenderModel(Matrix worldMatrix, Assets.Model model, Matrix[] boneTransform, Material material)
        {
            try
            {
                // Set parameters
                SetTransposeInverseWorldViewMatrix(Matrix.Transpose(Matrix.Invert(worldMatrix * viewMatrix)));
                SetWorldViewMatrix(worldMatrix * viewMatrix);
                SetWorldViewProjMatrix(worldMatrix * viewMatrix * projectionMatrix);

                if (model is FileModel && ((FileModel)model).IsSkinned) // If it is a skinned model.
                {
                    // I only consider skinning model with UV information for now. The extension is pretty simple thought.
                    // I have to do the extensions for normal and specular textures. TODO!!!
                    Resource.CurrentTechnique = Resource.Techniques["GBufferSkinnedWithTexture"];
                    SetBones(((FileModel) model).SkinTransforms);
                }
                else
                {
                    if (material is Constant)
                    {
                        Resource.CurrentTechnique = Resource.Techniques["GBufferWithoutTexture"];
                    }
                    else if (material is BlinnPhong)
                    {
                        BlinnPhong blinnPhongMaterial = ((BlinnPhong)material);
                        bool textured = false;
                        // Specular texture
                        if (blinnPhongMaterial.SpecularTexture != null && blinnPhongMaterial.SpecularPowerFromTexture)
                        {
                            SetSpecularTexture(blinnPhongMaterial.SpecularTexture);
                            SetSpecularTextured(true);
                            textured = true;
                            Resource.CurrentTechnique = Resource.Techniques["GBufferWithSpecularTexture"];
                        }
                        else
                        {
                            SetSpecularPower(blinnPhongMaterial.SpecularPower);
                            SetSpecularTextured(false);
                        }
                        // Normal texture
                        if (blinnPhongMaterial.NormalTexture != null)
                        {
                            textured = true;
                            SetObjectNormalTexture(blinnPhongMaterial.NormalTexture);
                            if (blinnPhongMaterial.ParallaxEnabled)
                            {
                                Resource.CurrentTechnique = Resource.Techniques["GBufferWithParallax"];
                                SetLODThreshold(blinnPhongMaterial.ParallaxLodThreshold);
                                SetMinimumNumberSamples(blinnPhongMaterial.ParallaxMinimumNumberSamples);
                                SetMaximumNumberSamples(blinnPhongMaterial.ParallaxMaximumNumberSamples);
                                SetHeightMapScale(blinnPhongMaterial.ParallaxHeightMapScale);
                            }
                            else
                            {
                                Resource.CurrentTechnique = Resource.Techniques["GBufferWithNormalMap"];
                            }
                        }
                        if (!textured)
                            Resource.CurrentTechnique = Resource.Techniques["GBufferWithoutTexture"];
                    }
                    else if (material is CarPaint)
                    {
                        CarPaint blinnPhongMaterial = ((CarPaint)material);
                        bool textured = false;
                        // Specular texture
                        if (blinnPhongMaterial.SpecularTexture != null && blinnPhongMaterial.SpecularTexturePowerEnabled)
                        {
                            SetSpecularTexture(blinnPhongMaterial.SpecularTexture);
                            SetSpecularTextured(true);
                            textured = true;
                            Resource.CurrentTechnique = Resource.Techniques["GBufferWithSpecularTexture"];
                        }
                        else
                        {
                            SetSpecularPower(blinnPhongMaterial.SpecularPower);
                            SetSpecularTextured(false);
                        }
                        // Normal texture
                        if (blinnPhongMaterial.NormalTexture != null)
                        {
                            textured = true;
                            SetObjectNormalTexture(blinnPhongMaterial.NormalTexture);
                            Resource.CurrentTechnique = Resource.Techniques["GBufferWithNormalMap"];
                        }
                        if (!textured)
                            Resource.CurrentTechnique = Resource.Techniques["GBufferWithoutTexture"];
                    }
                    /*else if (material is Terrain)
                    {
                        SetDisplacementTexture(TerrainMaterial.DisplacementTexture);
                        SetUvRectangle(((TerrainMaterial)((GraphicObject)renderObjects).Material).UvRectangle);
                        SetFarTerrainBeginDistance(TerrainMaterial.FarTerrainBeginDistance);
                        SetFlatRange(TerrainMaterial.FlatRange);
                        SetFarTerrain(((TerrainMaterial)((GraphicObject)renderObjects).Material).FarTerrain);
                        SetObjectNormalTexture(TerrainMaterial.NormalTexture);
                        SetSpecularPower(500);
                        Effect.CurrentTechnique = Effect.Techniques["GBufferTerrain"];
                    }*/
                    else
                    {
                        throw new InvalidOperationException("GBuffer: This material is not supported by the GBuffer renderer.");
                    }
                }
                Resource.CurrentTechnique.Passes[0].Apply();
                model.Render();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("GBuffer: Unable to render model.", e);
            }
        } // RenderModel

        #endregion

    } // GBufferShader
} // XNAFinalEngine.Graphics
