
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Components
{
    /// <summary>
    /// Display a texture into the HUD.
    /// This component works with 2D and 3D game objects.
    /// </summary>
    public class HudTexture : HudElement
    {

        #region Variables

        // To allow the checking.
        private Rectangle? sourceRectangle;

        #endregion

        #region Properties

        /// <summary>
        /// The Font to use when rendering the text.
        /// </summary>
        public Texture Texture { get; set; }

        /// <summary>
        /// A rectangle that specifies (in texels) the source texels from a texture.
        /// </summary>
        /// <remarks>Use null to draw the entire texture.</remarks>
        public Rectangle? SourceRectangle 
        {
            get
            {
                if (sourceRectangle.HasValue &&
                    (sourceRectangle.Value.Width + sourceRectangle.Value.X > Texture.Width ||
                     sourceRectangle.Value.Height + sourceRectangle.Value.Y > Texture.Height))
                    throw new InvalidOperationException("HUD Texture: the source rectangle is out of bounds.");
                return sourceRectangle;
            }
            set { sourceRectangle = value; }
        } // SourceRectangle

        /// <summary>
        /// A rectangle that specifies (in screen coordinates) the destination for drawing the sprite.  
        /// </summary>
        /// <remarks>
        /// If this rectangle is not the same size as the source rectangle, the sprite will be scaled to fit.
        /// Also, if this value is null then the transform component will be used to position the texture.</remarks>
        public Rectangle? DestinationRectangle { get; set; }
        
        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Default values.
            Texture = null;
        } // Initialize

        #endregion

        #region Pool

        // Pool for this type of 2D components.
        private static readonly Pool<HudTexture> componentPool2D = new Pool<HudTexture>(20);

        /// <summary>
        /// Pool for this type of 2D components.
        /// </summary>
        internal static Pool<HudTexture> ComponentPool2D { get { return componentPool2D; } }

        // Pool for this type of 3D components.
        private static readonly Pool<HudTexture> componentPool3D = new Pool<HudTexture>(20);

        /// <summary>
        /// Pool for this type of 3D components.
        /// </summary>
        internal static Pool<HudTexture> ComponentPool3D { get { return componentPool3D; } }

        #endregion

    } // HudTexture
} // XNAFinalEngine.Components