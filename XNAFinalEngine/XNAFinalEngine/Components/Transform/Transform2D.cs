
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
using Microsoft.Xna.Framework;
#endregion

namespace XnaFinalEngine.Components
{

    /// <summary>
    /// Every entity in a scene has a Transform.
    /// It's used to store and manipulate the position, rotation and scale of the object.
    /// This tranform works in screen space.
    /// </summary>
    public class Transform2D : Transform
    {

        #region Variables

        /// <summary>
        /// The position in local space.
        /// </summary>
        private Vector3 localPosition;

        /// <summary>
        /// The rotation in local space and in degrees unit.
        /// </summary>
        private float localRotation;

        /// <summary>
        /// The scale in local space.
        /// </summary>
        private float localScale = 1;

        /// <summary>
        /// The parent of this transform component.
        /// In effect this field stores the game object parent.
        /// </summary>
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
            set { parent = value.Transform; }
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
        public float LocalScale
        {
            get { return localScale; }
            set
            {
                localScale = value;
                UpdateLocalMatrix();
            }
        } // LocalScale

        #endregion

        #region World Space
        
        /// <summary>
        /// The position in world space.
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
                    localPosition = Vector3.Transform(value, Matrix.Invert(parent.WorldMatrix));
                else
                    localPosition = value;
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
                // Alternative: WorldMatrix.Decompose(); But this is slower.
            }
            set
            {
                if (Parent != null)
                    localRotation = value - parent.Rotation;
                else
                    localRotation = value;
            }
        } // Rotation

        public float Scale
        {
            get
            {
                if (Parent != null)
                    return localScale * parent.Scale;
                return localScale;
                // Alternative: WorldMatrix.Decompose(); But this is slower.
            }
        } // Scale
        
        #endregion

        #endregion

        #region Transformations

        #region Translate


        #endregion

        #region Rotate

        #endregion

        #endregion

        #region Update Matrices

        /// <summary>
        /// Update local matrix
        /// If the local position, local rotation or local scale changes its value then the local matrix has to be updated.
        /// This method also has to update the world matrix.
        /// </summary>
        private void UpdateLocalMatrix()
        {
            // Don't use the property LocalMatrix to avoid an unnecessary decompose.
            localMatrix = Matrix.CreateScale(new Vector3(localScale, localScale, 1)) * Matrix.CreateRotationZ(localRotation * 3.1416f / 180f);
            localMatrix.Translation = localPosition; // * Matrix.CreateTranslation(localPosition);
            UpdateWorldMatrix();
        } // UpdateLocalMatrix

        /// <summary>
        /// Update the world matrix.
        /// If the parent's world matrix or the local matrix changes its value then the world matrix has to be updated.
        /// This method generates an event.
        /// </summary>
        private void UpdateWorldMatrix()
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
} // XnaFinalEngine.Components