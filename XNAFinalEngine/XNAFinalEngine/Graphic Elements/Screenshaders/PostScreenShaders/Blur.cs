
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
using XNAFinalEngine.UI;
#endregion

namespace XNAFinalEngine.GraphicElements
{
	/// <summary>
	/// Blurs a texture.
	/// </summary>
    public class Blur : ScreenShader
	{

		#region Variables

		/// <summary>
		/// Auxiliary render target.
		/// </summary>
        private RenderToTexture blurMapTempTexture;
        
        #endregion

        #region Properties

		/// <summary>
		/// Blur map texture
		/// </summary>
		public RenderToTexture BlurMapTexture { get; private set; }

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private EffectParameter epWindowSize,
                                epBlurWidth,
                                epSceneMap,
                                epBlurMap;

        #region Blur Width

        /// <summary>
        /// Blur Width
        /// </summary>
        private float blurWidth = 2.0f;

        /// <summary>
        /// Blur Width
        /// </summary>
        public float BlurWidth
        {
            get { return blurWidth; }
            set { blurWidth = value; }
        } // BlurWidth

        /// <summary>
        /// Last used blur width
        /// </summary>
        private static float? lastUsedBlurWidth = null;
        /// <summary>
        /// Set Blur Width (between 0 and 10)
        /// </summary>
        private void SetBlurWidth(float _blurWidth)
        {
            if (lastUsedBlurWidth != _blurWidth && _blurWidth >= 0.0f && _blurWidth <= 10.0f)
            {
                lastUsedBlurWidth = _blurWidth;
                epBlurWidth.SetValue(_blurWidth);
            }
        } // SetBlurWidth

		#endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Blurs a texture.
		/// </summary>
        public Blur(RenderToTexture.SizeType _rendeTargetSize = RenderToTexture.SizeType.FullScreen)
		{
            Effect = LoadShader("PostBlur");
            Effect.CurrentTechnique = Effect.Techniques["Blur"];

            GetParametersHandles();

            BlurMapTexture = new RenderToTexture(_rendeTargetSize, false, false, 0);
            blurMapTempTexture = new RenderToTexture(_rendeTargetSize, false, false, 0);

            LoadUITestElements();

        } // Blur

		#endregion

		#region Get parameters handles

		/// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected void GetParametersHandles()
		{
            try
            {
			    epWindowSize = Effect.Parameters["windowSize"];
                epBlurWidth = Effect.Parameters["BlurWidth"];
			    epSceneMap = Effect.Parameters["sceneMap"];
			    epBlurMap = Effect.Parameters["blurMap"];
            }
            catch
            {
                throw new Exception("Get the handles from the blur shader failed.");
            }
		} // GetParametersHandles

		#endregion

        #region Set parameters

        /// <summary>
        /// Set to the shader the specific atributes of this effect.
        /// </summary>
        private void SetParameters()
        {
            SetBlurWidth(blurWidth);
        } // SetParameters

        #endregion

        #region Generate blur
        
        /// <summary>
		/// Generate the blur effect.
		/// </summary>
		public void GenerateBlur(RenderToTexture sceneMapTexture)
		{   
            
            // Only apply if the texture is valid
			if (sceneMapTexture == null || sceneMapTexture.XnaTexture == null)
				return;

            SetParameters();
			epWindowSize.SetValue(new float[] { sceneMapTexture.Width, sceneMapTexture.Height });
			epSceneMap.SetValue(sceneMapTexture.XnaTexture);

            try
            {
                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    if (pass.Name == "BlurHorizontal")
                    {
                        blurMapTempTexture.EnableRenderTarget();
                    }
                    else
                    {
                        BlurMapTexture.EnableRenderTarget();
                    }

                    pass.Apply();
                    ScreenPlane.Render();

                    if (pass.Name == "BlurHorizontal")
                    {
                        blurMapTempTexture.DisableRenderTarget();
                        epBlurMap.SetValue(blurMapTempTexture.XnaTexture);
                    }
                    else
                    {
                        BlurMapTexture.DisableRenderTarget();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to render the blur effect " + e.Message);
            }
        } // GenerateBlur

		#endregion

        #region Test

        #region Variables

        private UISliderNumeric uiBlurWidth;

        #endregion

        /// <summary>
        /// Load the different elements of the user interface needed to modify the shader.
        /// </summary>
        public void LoadUITestElements()
        {

            uiBlurWidth = new UISliderNumeric("Blur Width", new Vector2(EngineManager.Width - 390, 110), 0, 10, 0.1f, BlurWidth);
        } // LoadUITestElements

        /// <summary>
        /// It allows modifying the shader/material in real time with the help of the engine UI for testing purposes.
        /// </summary>
        public void Test()
        {

            #region Reset Parameters

            // Si los parametros se han modificado es mejor tener los nuevos valores
            uiBlurWidth.CurrentValue = BlurWidth;

            #endregion
            
            Primitives.Draw2DSolidPlane(new Rectangle(EngineManager.Width - 400, 0, 400, EngineManager.Height), new Color(0.0f, 0.0f, 0.0f, 1f), new Color(0.0f, 0.0f, 0.0f, 0.0f));
            FontArial14.Render("Blur Parameters", new Vector2(EngineManager.Width - 380, 40), Color.White);
            uiBlurWidth.UpdateAndRender();

            BlurWidth = uiBlurWidth.CurrentValue;
        } // Test

        #endregion

	} // Blur
} // XNAFinalEngine.GraphicElements
