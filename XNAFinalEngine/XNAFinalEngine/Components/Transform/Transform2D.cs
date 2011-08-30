
#region Using directives
using Microsoft.Xna.Framework;
using System;
#endregion

namespace XnaFinalEngine.Components
{

    /// <summary>
    /// Every entity in a scene has a Transform.
    /// It's used to store and manipulate the position, rotation and scale of the object. 
    /// </summary>
    public class Transform2D : Component
    {

        #region Variables

        /// <summary>
        /// The position in local space.
        /// </summary>
        private Vector3 localPosition;

        /// <summary>
        /// The rotation in local space.
        /// </summary>
        private float localRotation;

        /// <summary>
        /// The scale in local space.
        /// </summary>
        private Vector2 localScale;

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
            get { return (GameObject2D)(parent.Owner); }
            set { parent = value.Transform; }
        } // Parent

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
        public Quaternion Rotation
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
                    return Quaternion.Concatenate(parent.Rotation, localRotation); // Matrix equivalent: localRotation * parent.Rotation
                return localRotation;
                // Alternative: WorldMatrix.Decompose(); But this is slower.
            }
            set
            {
                if (Parent != null)
                    localRotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateFromQuaternion(value) * Matrix.Invert(parent.WorldMatrix));
                else
                    localRotation = value;
            }
        } // Rotation

        public Vector3 Scale
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

    } // Transform2D
} // XnaFinalEngine.Components