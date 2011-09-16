
#region License
/*

 Based in the class Camera.cs from RacingGame.
 License: Microsoft_Permissive_License

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Graphics
{
	/// <summary>
    /// A base camera. All cameras have to inherate from this class.    
	/// Based on Quaternions to perform all the rotations required.	
    /// The projection matrix is automatically updated when a parameter changes.
	/// </summary>
    public abstract class Camera
    {

        #region Constants

        /// <summary>
        /// 2 Pi
        /// </summary>
        protected const float TwoPi = (float)Math.PI * 2;

        /// <summary>
        /// Pi/2
        /// </summary>
        protected const float HalfPi = (float)Math.PI / 2;

        #endregion

        #region Enumerates

        /// <summary>
        /// Move directions for controling the camera.
        /// X is for moving left/right, Y moves up/down and Z goes in/out.
        /// </summary>
        public enum MoveDirections
        {
            /// <summary>
            /// X direction (move left/right)
            /// </summary>
            X,
            /// <summary>
            /// Y direction (move up/down)
            /// </summary>
            Y,
            /// <summary>
            /// Z direction (move in/out)
            /// </summary>
            Z
        } // enum MoveDirections

        /// <summary>
        /// Rotation axis for controling the camera.
        /// Pitch is for moving head up/down,
        /// Roll rotates around nose
        /// and Yaw is the rotation to left/right.
        /// </summary>
        public enum RotationAxis
        {
            /// <summary>
            /// Pitch is for moving head up/down.
            /// </summary>
            Pitch,
            /// <summary>
            /// Roll rotates around nose.
            /// </summary>
            Roll,
            /// <summary>
            /// Yaw is the rotation to left/right.
            /// </summary>
            Yaw
        } // enum RotationAxis

        #endregion

        #region Variables

        /// <summary>
        /// Field of view, NearPlane, FarPlane.
        /// </summary>
        private float nearPlane = 0.1f,
                      farPlane = 20000.0f,
                      fieldOfView = (float)Math.PI / 5.0f;

	    /// <summary>
		/// Quaternion used for rotation.
		/// </summary>
		private Quaternion orientation = Quaternion.Identity;
        
        /// <summary>
        /// View Matrix.
        /// </summary>
	    private Matrix viewMatrix;

		#endregion

		#region Properties

        #region Projection

        /// <summary>
        /// Field of View.
        /// Unit: radians.
        /// </summary>
        public float FieldOfView
        {
            get { return fieldOfView; }
            set { fieldOfView = value; }
        } // FieldOfView

        /// <summary>
        /// Near Plane
        /// </summary>
        public float NearPlane
        {
            get { return nearPlane; }
            set { nearPlane = value; }
        } // NearPlane

        /// <summary>
        /// Far Plane
        /// </summary>
        public float FarPlane 
        {
            get { return farPlane;  }
            set { farPlane = value; }
        } // FarPlane

        /// <summary> 
        /// Unlike perspective projection, in orthographic projection there is no perspective foreshortening.
        /// </summary>
        public bool OrthographicProjection { get; set; }

        #endregion

        #region Matrices

	    /// <summary>
	    /// View matrix.
	    /// </summary>
	    public Matrix ViewMatrix
	    {
	        get { return viewMatrix; }
            set
            {
                Vector3 scale, position;
                viewMatrix = value;
                Matrix.Invert(viewMatrix).Decompose(out scale, out orientation, out position); // Position comes from the inverse view matrix.
                Position = position;
                viewMatrix.Decompose(out scale, out orientation, out position); // Orientation comes from the view matrix.
            }
	    } // ViewMatrix
        
	    /// <summary>
	    /// Projection matrix.
        /// We can set the projection matrix.
        /// But if a parameter changes or the projection matrix is manually updated then this value will be replaced.
	    /// </summary>
	    public Matrix ProjectionMatrix { get; set; }
        
        #endregion

        #region Position and Orientation

	    /// <summary>
	    /// Camera's position.
	    /// </summary>
	    public Vector3 Position { get; set; }
        
		/// <summary>
		/// Camera's orientation.
		/// </summary>
		public Quaternion Orientation
        {
            get { return orientation; }
            set { value.Normalize(); orientation = value; }
		} // Orientation

        /// <summary>
        /// Look at position. This value needs to be administrated correctly by the derivate classes. 
        /// It can be an aproximate value.
        /// </summary>
        public virtual Vector3 LookAtPosition { get; set; }

	    /// <summary>
	    /// Up Vector.
	    /// </summary>
	    public Vector3 UpVector
	    {
            get { return Matrix.CreateFromQuaternion(Orientation).Up; }
	    } // UpVector

		/// <summary>
		/// Get current x axis with help of the current view matrix.
		/// </summary>
		public Vector3 XAxis { get { return new Vector3(ViewMatrix.M11, ViewMatrix.M21, ViewMatrix.M31); } }

		/// <summary>
		/// Get current y axis with help of the current view matrix.
		/// </summary>
		public Vector3 YAxis { get { return new Vector3(ViewMatrix.M12, ViewMatrix.M22, ViewMatrix.M32); } }

		/// <summary>
		/// Get current z axis with help of the current view matrix.
		/// </summary>
		public Vector3 ZAxis { get { return new Vector3(ViewMatrix.M13, ViewMatrix.M23, ViewMatrix.M33); } }

        #endregion

        #endregion

        #region Look at

        /// <summary>
		/// Helper method to get quaternion and position.
		/// </summary>
		public void SetLookAt(Vector3 cameraPosition, Vector3 lookAtPosition, Vector3 upVector)
		{   
			// Build a look at matrix and get the quaternion from that.
            Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(cameraPosition, lookAtPosition, upVector));
            Position = cameraPosition;
            LookAtPosition = lookAtPosition;
		} // SetLookAt

		#endregion

		#region Rotate methods

        /// <summary>
        /// Rotate around pitch, roll or yaw axis in local mode. The current orientation is important for the axis calculation.
        /// </summary>
        /// <param name="axis">Rotation axis</param>
        /// <param name="angle">Angle</param>
        public void RotateLocal(RotationAxis axis, float angle)
		{
            if (axis == RotationAxis.Yaw)
            {
                Orientation = Quaternion.CreateFromYawPitchRoll(angle, 0, 0) * Orientation;
            }
            else
            {
                if (axis == RotationAxis.Pitch)
                {
                    Orientation = Quaternion.CreateFromYawPitchRoll(0, angle, 0) * Orientation;
                }
                else
                {
                    Orientation = Quaternion.CreateFromYawPitchRoll(0, 0, angle) * Orientation;
                }
            }
		} // RotateLocal

        /// <summary>
        /// Rotate around pitch, roll or yaw axis. The current orientation doesn'st matter for the axis calculation.
        /// </summary>
        /// <param name="axis">Rotation axis</param>
        /// <param name="angle">Angle</param>
        public void RotateGlobal(RotationAxis axis, float angle)
        {
            if (axis == RotationAxis.Yaw)
            {
                Orientation *= Quaternion.CreateFromYawPitchRoll(angle, 0, 0);
            }
            else
            {
                if (axis == RotationAxis.Pitch)
                {
                    Orientation *= Quaternion.CreateFromYawPitchRoll(0, angle, 0);
                }
                else
                {
                    Orientation *= Quaternion.CreateFromYawPitchRoll(0, 0, angle);
                }
            }
        } // RotateGlobal

		#endregion

		#region Translate method

		/// <summary>
		/// Translate into camera X, Y or Z axis with a specfic amount.
		/// </summary>
		/// <param name="amount">Amount</param>
		/// <param name="direction">Direction</param>
		public void TranslateLocal(float amount, MoveDirections direction)
		{
			Vector3 dir = direction == MoveDirections.X ? XAxis :
				          direction == MoveDirections.Y ? YAxis :
                                                          ZAxis;
            Position += dir * amount;
		} // Translate

		#endregion
        
        #region Update view matrix

        /// <summary>
		/// Update view matrix
		/// </summary>
		public void UpdateViewMatrix()
		{
			Orientation.Normalize();
			Matrix rotMatrix = Matrix.CreateFromQuaternion(Orientation);
            viewMatrix = Matrix.CreateTranslation(-Position) * rotMatrix;
		} // UpdateViewMatrix()

		#endregion

        #region Update projection matrix

        /// <summary>
        /// Update projection matrix.
        /// </summary>
        protected void UpdateProjectionMatrix()
        {
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfView, 1.6f, nearPlane, farPlane);
        } // UpdateProjectionMatrix

        #endregion

		#region Update

		/// <summary>
		/// Update camera, the engine calls it every frame in the update stage.
		/// </summary>
        public virtual void Update()
		{
			// Override it!!!!
		} // Update

		#endregion

        #region Bounding Frustum

        /// <summary>
        /// Camera Far Plane Bounding Frustum (in view space). 
        /// With the help of the bounding frustum, the position can be cheaply reconstructed from a depth value.
        /// </summary>
        public Vector3[] BoundingFrustum()
        {
            BoundingFrustum boundingFrustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);
            Vector3[] cornersWorldSpace = boundingFrustum.GetCorners();
            Vector3[] cornersViewSpace = new Vector3[4];
            // Transform form world space to view space
            for (int i = 0; i < 4; i++)
            {
                cornersViewSpace[i] = Vector3.Transform(cornersWorldSpace[i + 4], viewMatrix);
            }

            // Swap the last 2 values.
            Vector3 temp = cornersViewSpace[3];
            cornersViewSpace[3] = cornersViewSpace[2];
            cornersViewSpace[2] = temp;

            return cornersViewSpace;
        } // BoundingFrustum

        #endregion

        #region Calculate Orientation

        /// <summary>
        /// Calculate Orientation.
        /// </summary>
        public static Quaternion CalculateOrientation(Vector3 cameraPosition, Vector3 lookAtPosition, Vector3 upVector)
        {
            return Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(cameraPosition, lookAtPosition, upVector));
        } // CalculateOrientation

        #endregion

        #region Get yaw pitch and roll from quaternion

        public Vector3 GetYawPitchRollFromQuaternion(Quaternion quaternion)
        {
            Vector3 yawPitchRoll = new Vector3
            {
                X = (float)(Math.Asin(-2 * (quaternion.X * quaternion.Z - quaternion.W * quaternion.Y))),
                Y = (float)(Math.Atan2(2 * (quaternion.Y * quaternion.Z + quaternion.W * quaternion.X),
                                           quaternion.W * quaternion.W - quaternion.X * quaternion.X -
                                           quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z)),
                Z = (float)(Math.Atan2(2 * (quaternion.X * quaternion.Y + quaternion.W * quaternion.Z),
                                           quaternion.W * quaternion.W + quaternion.X * quaternion.X -
                                           quaternion.Y * quaternion.Y - quaternion.Z * quaternion.Z))
            };
            return yawPitchRoll;
        } // GetYawPitchRollFromQuaternion

        #endregion

    } // Camera
} // XNAFinalEngine.Graphics
