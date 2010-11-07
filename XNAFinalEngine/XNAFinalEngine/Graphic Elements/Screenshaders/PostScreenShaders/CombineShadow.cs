
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
using System;
using System.IO;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.GraphicElements
{
	/// <summary>
    /// Combine the frame buffer with a shadow texture (shadow or SSAO)
	/// </summary>
    public class CombineShadows : ScreenShader
	{

		#region Variables
		
		/// <summary>
		/// Effect handles for window size and scene map.
		/// </summary>
        private EffectParameter epResolution,
                                epShadowMap,
			                    epSceneMap;
        
        #endregion

        #region Resolution

        /// <summary>
        /// Last used resolution
        /// </summary>
        private static Vector2? lastUsedResolution = null;
        /// <summary>
        /// Set resolution.
        /// </summary>
        private void SetResolution(Vector2 _resolution)
        {
            if (lastUsedResolution != _resolution)
            {
                lastUsedResolution = _resolution;
                epResolution.SetValue(_resolution);
            }
        } // SetResolution

        #endregion

        #region Constructor

        /// <summary>
        /// Combine the frame buffer with a shadow texture (shadow map or SSAO)
		/// </summary>
        public CombineShadows()
		{
            Effect = LoadShader("PostCombineShadows");
            Effect.CurrentTechnique = Effect.Techniques["ModelShader30"];
                        
            GetParametersHandles();
            
        } // CombineShadows

		#endregion

		#region Get parameters handles
		
        /// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected void GetParametersHandles()
		{
            try
            {
                epResolution = Effect.Parameters["windowSize"];
			    epSceneMap   = Effect.Parameters["sceneMap"];
                epShadowMap  = Effect.Parameters["shadowMap"];
            }
            catch
            {
                throw new Exception("Get the handles from the blur shader failed.");
            }
		} // GetParametersHandles

		#endregion

        #region Combine Shadows

        /// <summary>
        /// Combine the frame buffer with a shadow texture (shadow or SSAO)
		/// </summary>
        public void GenerateCombineShadows(RenderToTexture shadowMapTexture)
		{
			// Only apply the effect if the texture is valid
            if (shadowMapTexture == null || shadowMapTexture.XnaTexture == null)
                return;
            
            // Set parameters
            epShadowMap.SetValue(shadowMapTexture.XnaTexture);
            SetResolution(new Vector2(EngineCore.EngineManager.Width, EngineCore.EngineManager.Height));
            try
            {
                Effect.CurrentTechnique.Passes[0].Apply();

                ScreenPlane.Render();
            }
            catch (Exception e)
            {
                throw new Exception("Unable to render the combine shadow effect " + e.Message);
            }

        } // GenerateCombineShadows

		#endregion

    } // CombineShadows
} // XNAFinalEngine.GraphicElements
