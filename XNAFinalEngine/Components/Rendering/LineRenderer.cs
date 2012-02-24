
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
using XNAFinalEngine.Helpers;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Line Renderer.
    /// </summary>
    /// <remarks>
    /// Lines are normally not used, and if they are used there are few lines or the performance is not critical.
    /// </remarks>
    public class LineRenderer : Renderer
    {

        #region Variables

        private PrimitiveType primitiveType;

        #endregion

        #region Properties

        /// <summary>
        /// The array of vertex to connect.
        /// </summary>
        /// <remarks>
        /// If the component works in 2D the third value of the position still indicates the vertex’s depth.
        /// </remarks>
        public VertexPositionColor[] Vertices { get; set; }

        public PrimitiveType PrimitiveType
        {
            get { return primitiveType; }
            set
            {
                // These three types reuse vertices, so we can't flush properly without more complex logic.
                // Since that's a bit too complicated for this sample, we'll simply disallow them.
                if (primitiveType == PrimitiveType.LineStrip || primitiveType == PrimitiveType.TriangleStrip)
                {
                    throw new NotSupportedException("Line Renderer: The specified primitiveType is not supported.");
                }
                primitiveType = value;
            }
        } // PrimitiveType

        /// <summary>
        /// If the texture is null then the system will be draw a simple 1 pixel wide line.
        /// </summary>
        public Texture Texture { get; set; }

        /// <summary>
        /// Line width (only valid when texture is not null).
        /// </summary>
        public float Width { get; set; }
        
        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Default values
            Vertices = null;
            Texture = null;
            Width = 10;
            PrimitiveType = PrimitiveType.LineList;
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            base.Uninitialize();
        } // Uninitialize

        #endregion

        #region Pool

        // Make new pools for triangle list and line list in both 2D and 3D. TODO!!

        // Pool for this type of 2D components.
        private static readonly Pool<LineRenderer> componentPool2D = new Pool<LineRenderer>(20);

        /// <summary>
        /// Pool for this type of 2D components.
        /// </summary>
        internal static Pool<LineRenderer> ComponentPool2D { get { return componentPool2D; } }

        // Pool for this type of 3D components.
        private static readonly Pool<LineRenderer> componentPool3D = new Pool<LineRenderer>(20);

        /// <summary>
        /// Pool for this type of 3D components.
        /// </summary>
        internal static Pool<LineRenderer> ComponentPool3D { get { return componentPool3D; } }

        #endregion

    } // LineRenderer
} // XNAFinalEngine.Components
