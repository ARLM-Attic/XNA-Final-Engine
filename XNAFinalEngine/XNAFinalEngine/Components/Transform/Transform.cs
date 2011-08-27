
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
    public class Transform : Component
    {

        #region Enumerates

        /// <summary>
        /// The coordinate space in which to operate.
        /// </summary>
        public enum Space
        {
            /// <summary>
            /// Applies transformation relative to the local coordinate system
            /// </summary>
            Local,
            /// <summary>
            /// Applies transformation relative to the world coordinate system
            /// </summary>
            World
        } // Space

        #endregion
        
        #region Variables

        /// <summary>
        /// Local matrix.
        /// </summary>
        private Matrix localMatrix = Matrix.Identity;

        /// <summary>
        /// World matrix.
        /// </summary>
        private Matrix worldMatrix = Matrix.Identity;

        /// <summary>
        /// The position in local space.
        /// </summary>
        private Vector3 localPosition;

        /// <summary>
        /// The rotation in local space.
        /// </summary>
        private Quaternion localRotation;

        /// <summary>
        /// The scale in local space.
        /// </summary>
        private Vector3 localScale;

        /// <summary>
        /// The parent of this transform component.
        /// In effect this field stores the game object parent.
        /// </summary>
        private Transform parent;

        #endregion

        #region Properties

        #region Parent

        /// <summary>
        /// The parent of this transform component.
        /// In effect this field stores the game object parent.
        /// </summary>
        internal GameObject Parent
        {
            get { return parent.GameObject; }
            set { parent = value.Transform; }
        } // Parent

        #endregion

        #region Matrices

        /// <summary>
        /// Local Matrix.
        /// </summary>
        public Matrix LocalMatrix
        {
            get { return localMatrix; }
            set
            {
                localMatrix = value;
                localMatrix.Decompose(out localScale, out localRotation, out localPosition);                
                UpdateWorldMatrix();
            }
        } // LocalMatrix

        /// <summary>
        /// World matrix. 
        /// This method actually changes the local matrix but the game object hierarchy is taken into consideration.
        /// </summary>
        public Matrix WorldMatrix
        {
            get { return worldMatrix; }
            set { LocalMatrix = value * Matrix.Invert(parent.WorldMatrix); }
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
        public Quaternion LocalRotation
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
        public Vector3 LocalScale
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

        #region Forward, Up, Right

        /// <summary>
        /// Forward vector in world space.
        /// </summary>
        public Vector3 Forward { get { return WorldMatrix.Forward; } }

        /// <summary>
        /// Up vector in world space.
        /// </summary>
        public Vector3 Up { get { return WorldMatrix.Up; } }

        /// <summary>
        /// Right vector in world space.
        /// </summary>
        public Vector3 Right { get { return WorldMatrix.Right; } }

        #endregion

        #endregion

        #region Transformations

        #region Translate

        /// <summary>
        /// Translate the game object.
        /// In local space mode the object will be translated using its local coordinate system.
        /// I.e. the game’object’s world rotation will be the coordinate system used.
        /// In world space mode the object will be translated using the world coordinate system.
        /// I.e no rotation will affect this translation.
        /// Default space: local space.
        /// </summary>
        public void Translate(Vector3 translation, Space space = Space.Local)
        {
            if (space == Space.Local)
            {
                LocalPosition = LocalPosition + Vector3.Transform(translation, Matrix.CreateFromQuaternion(localRotation));
            }
            else
            {
                Position = Position + translation;
            }
        } // Translate
        
        /// <summary>
        /// Translate the game object.
        /// In local space mode the object will be translated using its local coordinate system.
        /// I.e. the game’object’s world rotation will be the coordinate system used.
        /// In world space mode the object will be translated using the world coordinate system.
        /// I.e no rotation will affect this translation.
        /// Default space: local space.
        /// </summary>
        public void Translate(float x, float y, float z, Space space = Space.Local)
        {
            Translate(new Vector3(x, y, z), space);
        } // Translate

        /// <summary>
        /// The movement is applied relative to Transform's local coordinate system.
        /// </summary>
        public void Translate(Vector3 translation, Transform relativeTo)
        {
            if (relativeTo == null)
                throw new Exception("Transform component exception: relativeTo value is null");
            LocalPosition = LocalPosition + Vector3.Transform(Vector3.Transform(translation, Matrix.CreateFromQuaternion(relativeTo.Rotation)), Matrix.Invert(parent.WorldMatrix));
        } // Translate

        #endregion

        #region Rotate

        /// <summary>
        /// Rotate the game object. 
        /// Default space: local space.
        /// </summary>
        public void Rotate(Vector3 rotation, Space space = Space.Local)
        {
            if (space == Space.Local)
            {
                LocalRotation = Quaternion.Concatenate(LocalRotation, Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z));
            }
            else
            {
                Rotation = Quaternion.Concatenate(Rotation, Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z));
            }
        } // Rotate

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
            localMatrix = Matrix.CreateScale(localScale) * Matrix.CreateFromQuaternion(localRotation);
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
            // TODO!!!

        } // UpdateWorldMatrix

        #endregion

    } // Transform
} // XnaFinalEngine.Components