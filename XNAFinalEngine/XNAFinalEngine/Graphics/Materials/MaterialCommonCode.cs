
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

#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Auxilliary class. 
    /// Some code are used in more than one material, but these code aren’t used in every material.
    /// In those cases the C# hierarchy doesn’t help too much. What’s the mayor problem? The static variables.
    /// It’s not elegant, but the alternatives don’t seem to work well.
    /// </summary>
    internal abstract class MaterialCommonCode
    {

        #region Variables

        /// <summary>
        /// Needed it because the compiler will give undesirable errors.
        /// </summary>
        private Effect Effect = null;

        #endregion

        #region Common Atributes
           
        #region Shader Parameters

        /// <summary>
        /// Effect handles for all material shaders.
        /// </summary>
        protected static EffectParameter
                               // Matrices // 
                               epWorldIT,
                               epWorldViewProj,
                               epWorld,
                               epViewI,
                               epViewProj,
                               // Surface //
                               epSurfaceColor,
                               epAlphaBlending;

        #region Matrices

        /// <summary>
        /// Last used transpose inverse world matrix.
        /// </summary>
        private static Matrix ?lastUsedTransposeInverseWorldMatrix;
        /// <summary>
        /// Set transpose inverse world matrix.
        /// </summary>
        private static Matrix TransposeInverseWorldMatrix
        {
            set
            {
                if (lastUsedTransposeInverseWorldMatrix != value)
                {
                    lastUsedTransposeInverseWorldMatrix = value;
                    epWorldIT.SetValue(value);
                } // if
            } // set
        } // TransposeInvertWorldMatrix

        /// <summary>
        /// Last used world matrix.
        /// </summary>
        private static Matrix ?lastUsedWorldMatrix;
        /// <summary>
        /// Set world matrix.
        /// </summary>
        private static Matrix WorldMatrix
        {
            set
            {
                if (lastUsedWorldMatrix != value)
                {
                    lastUsedWorldMatrix = value;
                    epWorld.SetValue(value);
                } // if
            } // set
        } // WorldMatrix

        /// <summary>
        /// Last used inverse view matrix.
        /// </summary>
        private static Matrix ?lastUsedInverseViewMatrix;
        /// <summary>
        /// Set view inverse matrix.
        /// </summary>
        private static Matrix InverseViewMatrix
        {
            set
            {
                if (lastUsedInverseViewMatrix != value)
                {
                    lastUsedInverseViewMatrix = value;
                    epViewI.SetValue(value);
                } // if
            } // set
        } // InverseViewMatrix

        /// <summary>
        /// Last used world view projection matrix.
        /// </summary>
        private static Matrix ?lastUsedWorldViewProjMatrix;
        /// <summary>
        /// Set world view projection matrix.
        /// </summary>
        private static Matrix WorldViewProjMatrix
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

        /// <summary>
        /// Last used view projection matrix.
        /// </summary>
        private static Matrix ?lastUsedViewProjMatrix;
        /// <summary>
        /// Set view projection matrix.
        /// </summary>
        private static Matrix ViewProjMatrix
        {
            set
            {
                if (lastUsedViewProjMatrix != value)
                {
                    lastUsedViewProjMatrix = value;
                    epViewProj.SetValue(value);
                } // if
            } // set
        } // ViewProjMatrix

        #endregion

        #region Surface

        /// <summary>
        /// Surface color.
        /// </summary>
        private Color surfaceColor = Color.White;

        /// <summary>
        /// Surface color.
        /// </summary>
        public Color SurfaceColor
        {
            get { return surfaceColor; }
            set { surfaceColor = value; }
        } // SurfaceColor

        /// <summary>
        /// Last used surface color.
        /// </summary>
        private static Color ?lastUsedSurfaceColor;
        /// <summary>
        /// Set surface color.
        /// </summary>
        private static void SetSurfaceColor(Color _surfaceColor)
        {
            if (lastUsedSurfaceColor != _surfaceColor)
            {
                lastUsedSurfaceColor = _surfaceColor;
                epSurfaceColor.SetValue(new Vector3(_surfaceColor.R / 255f, _surfaceColor.G / 255f, _surfaceColor.B / 255f));
            }
        } // SetSurfaceColor

        /// <summary>
        /// Alpha blending.
        /// </summary>
        private float alphaBlending = 1.0f;

        /// <summary>
        /// Alpha blending.
        /// </summary>
        public float AlphaBlending
        {
            get { return alphaBlending; }
            set { alphaBlending = value; }
        } // AlphaBlending

        /// <summary>
        /// Last used alpha blending.
        /// </summary>
        private static float ?lastUsedAlphaBlending;
        /// <summary>
        /// Set surface's alpha blending (value between 0 and 1)
        /// </summary>
        private static void SetAlphaBlending(float _alphaBlending)
        {
            if (lastUsedAlphaBlending != _alphaBlending && _alphaBlending >= 0.0f && _alphaBlending <= 1.0f)
            {
                lastUsedAlphaBlending = _alphaBlending;
                epAlphaBlending.SetValue(_alphaBlending);
            }
        } // SetAlphaBlending

        #endregion

        #endregion

        /// <summary>
        /// Get the common handlers from the shader.
        /// </summary>
        protected void GetCommonParametersHandles()
        {
            // Matrices //
            epWorldIT = Effect.Parameters["WorldIT"];
            epWorldViewProj = Effect.Parameters["WorldViewProj"];
            epWorld = Effect.Parameters["World"];
            epViewI = Effect.Parameters["ViewI"];
            epViewProj = Effect.Parameters["ViewProj"];
            // Alpha Blending //
            epSurfaceColor = Effect.Parameters["SurfaceColor"];
            epAlphaBlending = Effect.Parameters["AlphaBlending"];
        } // GetCommonParametersHandles

        /// <summary>
        /// Set to the shader the common atributes of this material.
        /// </summary>
        protected virtual void SetCommomParameters(Matrix worldMatrix)
        {
            // Initialization of the Matrices
            TransposeInverseWorldMatrix = Matrix.Transpose(Matrix.Invert(worldMatrix));
            WorldMatrix = worldMatrix;
            WorldViewProjMatrix = worldMatrix * ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix;
            InverseViewMatrix = Matrix.Invert(ApplicationLogic.Camera.ViewMatrix);
            ViewProjMatrix = ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix;
            // Inicialization of the alpha blending
            SetSurfaceColor(surfaceColor);
            SetAlphaBlending(alphaBlending);
        } // SetCommomParameters

        #endregion

        #region Ambient Light Attributes
        
        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epAmbientLightColor;

        /// <summary>
        /// Last used ambient light color.
        /// </summary>
        private static Color ?lastUsedAmbientLightColor;
        /// <summary>
        /// Set ambient light color.
        /// </summary>
        protected void SetAmbientLightColor(Color _ambientLightColor)
        {
            if (lastUsedAmbientLightColor != _ambientLightColor)
            {
                lastUsedAmbientLightColor = _ambientLightColor;
                epAmbientLightColor.SetValue(new Vector3(_ambientLightColor.R / 255f, _ambientLightColor.G / 255f, _ambientLightColor.B / 255f));
            }
        } // SetAmbientLightColor

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetAmbientLightParametersHandles()
        {
            epAmbientLightColor = Effect.Parameters["AmbientLightColor"];
        } // GetAmbientLightParametersHandles

        /// <summary>
        /// Set the ambient light parameters to the shader.
        /// </summary>
        public virtual void SetAmbientLightParameters()
        {
            SetAmbientLightColor(AmbientLight.LightColor);
        } // SetAmbientLightParameters

        #endregion

        #region Point Light 1 Attributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles.
        /// </summary>
        protected static EffectParameter epPointLight1Pos,
                                         epPointLight1Color;

        #region Color

        /// <summary>
        /// Last used point light 1 color.
        /// </summary>
        private static Color ?lastUsedPointLight1Color;
        /// <summary>
        /// Set point light 1 color.
        /// </summary>
        protected void SetPointLight1Color(Color _pointLight1Color)
        {
            if (lastUsedPointLight1Color != _pointLight1Color)
            {
                lastUsedPointLight1Color = _pointLight1Color;
                epPointLight1Color.SetValue(new Vector3(_pointLight1Color.R / 255f, _pointLight1Color.G / 255f, _pointLight1Color.B / 255f));
            }
        } // SetPointLight1Color

        #endregion

        #region Position

        /// <summary>
        /// Last used point light 1 position.
        /// </summary>
        private static Vector3 ?lastUsedPointLight1Pos;
        /// <summary>
        /// Set point light 1 position.
        /// </summary>
        protected void SetPointLight1Pos(Vector3 _pointLight1Pos)
        {
            if (lastUsedPointLight1Pos != _pointLight1Pos)
            {
                lastUsedPointLight1Pos = _pointLight1Pos;
                epPointLight1Pos.SetValue(new Vector3(_pointLight1Pos.X, _pointLight1Pos.Y, _pointLight1Pos.Z));
            }
        } // SetPointLight1Pos

        #endregion

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetPointLight1ParametersHandles()
        {
            epPointLight1Color = Effect.Parameters["PointLightColor"];
            epPointLight1Pos   = Effect.Parameters["PointLightPos"];
        } // GetPointLight1ParametersHandles

        /// <summary>
        /// Set the point light parameters to the shader.
        /// </summary>
        public virtual void SetPointLight1Parameters(PointLight pointLight)
        {
            if (pointLight != null)
            {
                SetPointLight1Color(pointLight.Color);
                SetPointLight1Pos(pointLight.Position);
            }
            else
            {
                SetPointLight1Color(Color.Black);
            }
        } // SetPointLight1Parameters

        #endregion

        #region Point Light 2 Attributes
        
        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epPointLight2Pos,
                                         epPointLight2Color;

        #region Color

        /// <summary>
        /// Last used point light 2 color.
        /// </summary>
        private static Color ?lastUsedPointLight2Color;
        /// <summary>
        /// Set point light 2 color.
        /// </summary>
        protected void SetPointLight2Color(Color _pointLight2Color)
        {
            if (lastUsedPointLight2Color != _pointLight2Color)
            {
                lastUsedPointLight2Color = _pointLight2Color;
                epPointLight2Color.SetValue(new Vector3(_pointLight2Color.R / 255f, _pointLight2Color.G / 255f, _pointLight2Color.B / 255f));
            }
        } // SetPointLight2Color

        #endregion

        #region Position

        /// <summary>
        /// Last used point light 2 position.
        /// </summary>
        private static Vector3 ?lastUsedPointLight2Pos;
        /// <summary>
        /// Set point light 2 position.
        /// </summary>
        protected void SetPointLight2Pos(Vector3 _pointLight2Pos)
        {
            if (lastUsedPointLight2Pos != _pointLight2Pos)
            {
                lastUsedPointLight2Pos = _pointLight2Pos;
                epPointLight2Pos.SetValue(new Vector3(_pointLight2Pos.X, _pointLight2Pos.Y, _pointLight2Pos.Z));
            } // if
        } // SetPointLight2Pos

        #endregion

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetPointLight2ParametersHandles()
        {
            epPointLight2Color = Effect.Parameters["PointLightColor2"];
            epPointLight2Pos = Effect.Parameters["PointLightPos2"];
        } // GetPointLight2ParametersHandles

        /// <summary>
        /// Set the point light parameters to the shader.
        /// </summary>
        public virtual void SetPointLight2Parameters(PointLight pointLight)
        {
            if (pointLight != null)
            {
                SetPointLight2Color(pointLight.Color);
                SetPointLight2Pos(pointLight.Position);
            }
            else
            {
                SetPointLight2Color(Color.Black);
            }
        } // SetPointLight2Parameters

        #endregion

        #region Point Light 3 Attributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epPointLight3Pos,
                                         epPointLight3Color;

        #region Color

        /// <summary>
        /// Last used point light 3 color.
        /// </summary>
        private static Color ?lastUsedPointLight3Color;
        /// <summary>
        /// Set point light 3 color.
        /// </summary>
        protected void SetPointLight3Color(Color _pointLight3Color)
        {
            if (lastUsedPointLight3Color != _pointLight3Color)
            {
                lastUsedPointLight3Color = _pointLight3Color;
                epPointLight3Color.SetValue(new Vector3(_pointLight3Color.R / 255f, _pointLight3Color.G / 255f, _pointLight3Color.B / 255f));
            }
        } // SetPointLight3Color

        #endregion

        #region Position

        /// <summary>
        /// Last used point light 3 position.
        /// </summary>
        private static Vector3 ?lastUsedPointLight3Pos;
        /// <summary>
        /// Set point light 3 position.
        /// </summary>
        protected void SetPointLight3Pos(Vector3 _pointLight3Pos)
        {
            if (lastUsedPointLight3Pos != _pointLight3Pos)
            {
                lastUsedPointLight3Pos = _pointLight3Pos;
                epPointLight3Pos.SetValue(new Vector3(_pointLight3Pos.X, _pointLight3Pos.Y, _pointLight3Pos.Z));
            } // if
        } // SetPointLight3Pos

        #endregion

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetPointLight3ParametersHandles()
        {
            epPointLight3Color = Effect.Parameters["PointLightColor3"];
            epPointLight3Pos = Effect.Parameters["PointLightPos3"];
        } // GetPointLight3ParametersHandles

        /// <summary>
        /// Set the point light parameters to the shader.
        /// </summary>
        public virtual void SetPointLight3Parameters(PointLight pointLight)
        {
            if (pointLight != null)
            {
                SetPointLight3Color(pointLight.Color);
                SetPointLight3Pos(pointLight.Position);
            }
            else
            {
                SetPointLight3Color(Color.Black);
            }
        } // SetPointLight3Parameters

        #endregion

        #region Directional Light 1 Attributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epDirectionalLight1Dir,
                                         epDirectionalLight1Color;

        /// <summary>
        /// Last used directional light 1 color.
        /// </summary>
        private static Color ?lastUsedDirectionalLight1Color;
        /// <summary>
        /// Set directional light 1 color.
        /// </summary>
        protected void SetDirectionalLight1Color(Color _directionalLight1Color)
        {
            if (lastUsedDirectionalLight1Color != _directionalLight1Color)
            {
                lastUsedDirectionalLight1Color = _directionalLight1Color;
                epDirectionalLight1Color.SetValue(new Vector3(_directionalLight1Color.R / 255f, _directionalLight1Color.G / 255f, _directionalLight1Color.B / 255f));
            }
        } // SetDirectionalLight1Color

        /// <summary>
        /// Last used directional light 1 direction.
        /// </summary>
        private static Vector3 ?lastUsedDirectionalLight1Dir;
        /// <summary>
        /// Set directional light 1 direction.
        /// </summary>
        protected void SetDirectionalLight1Dir(Vector3 _directionalLight1Dir)
        {
            if (lastUsedDirectionalLight1Dir != _directionalLight1Dir)
            {
                lastUsedDirectionalLight1Dir = _directionalLight1Dir;
                epDirectionalLight1Dir.SetValue(new Vector3(_directionalLight1Dir.X, _directionalLight1Dir.Y, _directionalLight1Dir.Z));
            }
        } // SetDirectionalLight1Dir

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetDirectionalLight1ParametersHandles()
        {
            epDirectionalLight1Color = Effect.Parameters["DirectionalLightColor"];
            epDirectionalLight1Dir = Effect.Parameters["DirectionalLightDir"];
        } // GetDirectionalLight1ParametersHandles

        /// <summary>
        /// Set the directional light parameters to the shader.
        /// </summary>
        public virtual void SetDirectionalLight1Parameters(DirectionalLight directionalLight)
        {
            if (directionalLight != null)
            {
                SetDirectionalLight1Color(directionalLight.Color);
                SetDirectionalLight1Dir(directionalLight.Direction);
            }
            else
            {
                SetDirectionalLight1Color(Color.Black);
            }
        } // SetDirectionalLight1Parameters

        #endregion

        #region Spot Light 1 Attributes

        #region Shader Parameters
        
        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epSpotLight1Pos,
                                         epSpotLight1Dir,
                                         epSpotLight1Cone,
                                         epSpotLight1Color,
                                         epSpotLight1Intensity;

        #region Color

        /// <summary>
        /// Last used spot light 1 color.
        /// </summary>
        private static Color ?lastUsedSpotLight1Color = null;
        /// <summary>
        /// Set spot light 1 color.
        /// </summary>
        protected void SetSpotLight1Color(Color _spotLight1Color)
        {
            if (lastUsedSpotLight1Color != _spotLight1Color)
            {
                lastUsedSpotLight1Color = _spotLight1Color;
                epSpotLight1Color.SetValue(new Vector3(_spotLight1Color.R / 255f, _spotLight1Color.G / 255f, _spotLight1Color.B / 255f));
            }
        } // SetSpotLight1Color

        #endregion

        #region Direction

        /// <summary>
        /// Last used spot light 1 direction.
        /// </summary>
        private static Vector3 ?lastUsedSpotLight1Dir = null;
        /// <summary>
        /// Set spot light 1 direction.
        /// </summary>
        protected void SetSpotLight1Dir(Vector3 _spotLight1Dir)
        {
            if (lastUsedSpotLight1Dir != _spotLight1Dir)
            {
                lastUsedSpotLight1Dir = _spotLight1Dir;
                epSpotLight1Dir.SetValue(new Vector3(_spotLight1Dir.X, _spotLight1Dir.Y, _spotLight1Dir.Z));
            }
        } // SetSpotLight1Dir

        #endregion

        #region Position

        /// <summary>
        /// Last used spot light 1 position.
        /// </summary>
        private static Vector3 ?lastUsedSpotLight1Pos = null;
        /// <summary>
        /// Set spot light 1 position.
        /// </summary>
        protected void SetSpotLight1Pos(Vector3 _spotLight1Pos)
        {
            if (lastUsedSpotLight1Pos != _spotLight1Pos)
            {
                lastUsedSpotLight1Pos = _spotLight1Pos;
                epSpotLight1Pos.SetValue(new Vector3(_spotLight1Pos.X, _spotLight1Pos.Y, _spotLight1Pos.Z));
            }
        } // SetPointLight1Pos

        #endregion

        #region Cone

        /// <summary>
        /// Last used Spot Light 1 Cone.
        /// </summary>
        private static float ?lastUsedSpotLight1Cone = null;
        /// <summary>
        /// Set Set Spot Light 1 Cone (value between 0 and 90)
        /// </summary>
        private static void SetSpotLight1Cone(float _spotLight1Cone)
        {
            if (lastUsedSpotLight1Cone != _spotLight1Cone && _spotLight1Cone >= 0.0f && _spotLight1Cone <= 90.0f)
            {
                lastUsedSpotLight1Cone = _spotLight1Cone;
                epSpotLight1Cone.SetValue(_spotLight1Cone);
            }
        } // SetSpotLight1Cone

        #endregion

        #region Intensity

        /// <summary>
        /// Last used Spot Light 1 Intensity.
        /// </summary>
        private static float ?lastUsedSpotLight1Intensity = null;
        /// <summary>
        /// Set Set Spot Light 1 Intensity (value between 0 and 10)
        /// </summary>
        private void SetSpotLight1Intensity(float _spotLight1Intensity)
        {
            if (lastUsedSpotLight1Intensity != _spotLight1Intensity && _spotLight1Intensity >= 0.0f && _spotLight1Intensity <= 10f)
            {
                lastUsedSpotLight1Intensity = _spotLight1Intensity;
                epSpotLight1Intensity.SetValue(_spotLight1Intensity);
            }
        } // SetSpotLight1Intensity

        #endregion

        #endregion

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        protected void GetSpotLight1ParametersHandles()
        {
            epSpotLight1Color = Effect.Parameters["SpotLightColor"];
            epSpotLight1Dir = Effect.Parameters["SpotLightDir"];
            epSpotLight1Pos = Effect.Parameters["SpotLightPos"];
            epSpotLight1Cone = Effect.Parameters["SpotLightCone"];
            epSpotLight1Intensity = Effect.Parameters["SpotLightIntensity"];
        } // GetSpotLight1ParametersHandles

        /// <summary>
        /// Set the spot light parameters to the shader.
        /// </summary>
        public virtual void SetSpotLight1Parameters(SpotLight spotLight)
        {
            if (spotLight != null)
            {
                SetSpotLight1Color(spotLight.Color);
                SetSpotLight1Dir(spotLight.Direction);
                SetSpotLight1Pos(spotLight.Position);
                SetSpotLight1Cone(spotLight.ApertureCone);
                SetSpotLight1Intensity(spotLight.Intensity);
            }
            else
            {
                SetSpotLight1Color(Color.Black);
            }
        } // SetSpotLight1Parameters

        #endregion

    } // MaterialCommonCode
} // XNAFinalEngine.Graphics