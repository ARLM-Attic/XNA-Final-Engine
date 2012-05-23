
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Every entity in a scene has a Transform.
    /// It's used to store and manipulate the position, rotation and scale of the object.
    /// </summary>
    public abstract class Transform : Component
    {

        #region Variables

        /// <summary>
        /// Local matrix.
        /// </summary>
        protected Matrix localMatrix = Matrix.Identity;

        /// <summary>
        /// World matrix.
        /// </summary>
        protected Matrix worldMatrix = Matrix.Identity;

        #endregion

        #region Properties

        /// <summary>
        /// Local Matrix.
        /// </summary>
        public virtual Matrix LocalMatrix
        {
            get { return localMatrix; }
            set
            {
                // Override it!!!
            }
        } // LocalMatrix

        /// <summary>
        /// World matrix.
        /// </summary>
        public virtual Matrix WorldMatrix
        {
            get { return worldMatrix; }
            set 
            {
                // Override it!!!
            }
        } // WorldMatrix

        #endregion

        #region Events

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// This is a very common event. Not even the object is passed to improve memory performance.
        /// </summary>
        internal delegate void WorldMatrixEventHandler(Matrix worldMatrix);

        /// <summary>
        /// Raised when the transform's world matrix changes.
        /// </summary>
        internal event WorldMatrixEventHandler WorldMatrixChanged;

        protected void RaiseWorldMatrixChanged()
        {
            if (WorldMatrixChanged != null)
            {
                WorldMatrixChanged(worldMatrix);
            }
        } // RaiseWorldMatrixChanged

        #endregion

        #region On Parent World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected virtual void OnParentWorldMatrixChanged(Matrix worldMatrix)
        {
            UpdateWorldMatrix();
        } // OnWorldMatrixChanged

        #endregion

        #region Update Matrices

        /// <summary>
        /// Update local matrix
        /// If the local position, local rotation or local scale changes its value then the local matrix has to be updated.
        /// This method also has to update the world matrix.
        /// </summary>
        protected abstract void UpdateLocalMatrix();

        /// <summary>
        /// Update the world matrix.
        /// If the parent's world matrix or the local matrix changes its value then the world matrix has to be updated.
        /// This method generates an event.
        /// </summary>
        protected abstract void UpdateWorldMatrix();

        #endregion

    } // Transform
} // XNAFinalEngine.Components