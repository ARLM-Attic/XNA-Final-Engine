
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
        private Vector3 localScale;

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
        
        #endregion

        #region Transformations

        #region Translate

        
        #endregion

        #region Rotate

        #endregion

        #endregion
        
    } // Transform2D
} // XnaFinalEngine.Components