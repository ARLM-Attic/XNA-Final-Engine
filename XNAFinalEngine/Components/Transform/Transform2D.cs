
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
    /// This tranform works in screen space.
    /// </summary>
    public class Transform2D : Transform
    {

        #region Variables
        
        // The position in local space.
        private Vector3 localPosition;

        // The rotation in local space and in degrees unit.
        private float localRotation;

        // The scale in local space.
        private Vector2 localScale = new Vector2(1, 1);
        
        // The parent of this transform component.
        // In effect this field stores the game object parent.
        private Transform2D parent;

        #endregion

        #region Properties

        #region Parent

        /// <summary>
        /// The parent of this transform component.
        /// In effect this field stores the game object parent.
        /// </summary>
        internal GameObject2D Parent
        {
            get
            {
                if (parent != null)
                    return (GameObject2D)(parent.Owner);
                return null;
            }
            set
            {
                if (parent != null)
                    parent.WorldMatrixChanged -= OnParentWorldMatrixChanged;
                if (value == null)
                    parent = null;
                else
                {
                    parent = value.Transform;
                    parent.WorldMatrixChanged += OnParentWorldMatrixChanged;
                }
                UpdateWorldMatrix();
            }
        } // Parent

        #endregion

        #region Matrices

        /// <summary>
        /// Local Matrix.
        /// </summary>
        public override Matrix LocalMatrix
        {
            get { return localMatrix; }
        } // LocalMatrix

        /// <summary>
        /// World matrix.
        /// </summary>
        public override Matrix WorldMatrix
        {
            get { return worldMatrix; }
        } // WorldMatrix

        #endregion

        #region Local Space

        /// <summary>
        /// The position in local space.
        /// </summary>
        public Vector3 LocalPosition
        {
            get { return localPosition; }
            set
            {
                localPosition = value;
                UpdateLocalMatrix();
            }
        } // LocalPosition

        /// <summary>
        /// The rotation in local space.
        /// </summary>
        public float LocalRotation
        {
            get { return localRotation; }
            set
            {
                localRotation = value;
                UpdateLocalMatrix();
            }
        } // LocalRotation

        /// <summary>
        /// The scale in local space.
        /// </summary>
        public Vector2 LocalScale
        {
            get { return localScale; }
            set
            {
                localScale = value;
                if (localScale.X <= 0.0001f)
                    localScale.X = 0.0001f;
                if (localScale.Y <= 0.0001f)
                    localScale.Y = 0.0001f;
                UpdateLocalMatrix();
            }
        } // LocalScale

        #endregion

        #region World Space
        
        /// <summary>
        /// The position in screen space.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                if (Parent != null)
                    return worldMatrix.Translation; // Vector3.Transform(localPosition, parent.WorldMatrix); // Never use this: position + Father.WorldPosition;
                return localPosition;
            }
            set
            {
                if (Parent != null)
                    LocalPosition = Vector3.Transform(value, Matrix.Invert(parent.WorldMatrix));
                else
                    LocalPosition = value;
            }
        } // Position

        /// <summary>
        /// The rotation in world space.
        /// </summary>
        public float Rotation
        {
            // http://forums.create.msdn.com/forums/p/28491/158543.aspx
            // The following two pieces of code are equivalent:
            // Matrix ComposeOrientation(Quaternion a, Quaternion b)
            // {
            //     Matrix Ma = Matrix.CreateFromQuaternion(a);
            //     Matrix Mb = Matrix.CreateFromQuaternion(b);
            //     Matrix Mc = Ma * Mb;
            //     return Mc;
            // } 
            // Matrix ComposeOrientation(Quaternion a, Quaternion b)
            // {
            //     Quaternion Qc = b * a;
            //     Matrix Mc = Matrix.CreateFromQuaternion(Qc);
            //     return Mc;
            // }
            get
            {
                if (Parent != null)
                    return localRotation + parent.Rotation; // Quaternion.Concatenate(parent.Rotation, localRotation); // Matrix equivalent: localRotation * parent.Rotation
                return localRotation;
            }
            set
            {
                if (Parent != null)
                    LocalRotation = value - parent.Rotation;
                else
                    LocalRotation = value;
            }
        } // Rotation

        public Vector2 Scale
        {
            get
            {
                if (Parent != null)
                    // Alternative: WorldMatrix.Decompose(); But this is slower.
                    return localScale * parent.Scale;
                return localScale;
                
            }
        } // Scale
        
        #endregion

        #endregion

        #region Transformations

        #region Translate

        // TODO!!

        #endregion

        #region Rotate

        // TODO!!

        #endregion

        #endregion

        #region Update Matrices

        /// <summary>
        /// Update local matrix
        /// If the local position, local rotation or local scale changes its value then the local matrix has to be updated.
        /// This method also has to update the world matrix.
        /// </summary>
        protected override void UpdateLocalMatrix()
        {
            // Don't use the property LocalMatrix to avoid an unnecessary decompose.
            localMatrix = Matrix.CreateScale(new Vector3(localScale.X, localScale.Y, 1)) * Matrix.CreateRotationZ(localRotation * 3.1416f / 180f);
            localMatrix.Translation = localPosition;
            UpdateWorldMatrix();
        } // UpdateLocalMatrix

        /// <summary>
        /// Update the world matrix.
        /// If the parent's world matrix or the local matrix changes its value then the world matrix has to be updated.
        /// This method generates an event.
        /// </summary>
        protected override void UpdateWorldMatrix()
        {
            if (Parent != null)
                worldMatrix = localMatrix * parent.WorldMatrix;
            else
                worldMatrix = localMatrix;
            // Raise event
            RaiseWorldMatrixChanged();
        } // UpdateWorldMatrix

        #endregion

    } // Transform2D
} // XNAFinalEngine.Components