
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
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.GraphicElements
{
	/// <summary>
    /// A base camera. All cameras need to inherate from this class.    
	/// Based on Quaternions to perform all the rotations required.	
    /// The projection matrix is automatically updated with a parameter change.
    /// The view matrix is automatically updated in the update stage.
    /// Pensar si no es mejor actualizar la matriz de vision por cada cambio. TODO
	/// </summary>
    public abstract class Camera
    {

        #region Constants

        /// <summary>
        /// 2 Pi
        /// </summary>
        protected const float TwoPI = (float)Math.PI * 2;

        /// <summary>
        /// Pi/2
        /// </summary>
        protected const float HalfPI = (float)Math.PI / 2;

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
        /// Field of view, NearPlane, FarPlane, AspectRatio.
        /// </summary>
        private float nearPlane = 1f,
                      farPlane = 1000.0f,
                      fieldOfView = (float)Math.PI / 8.0f;

        /// <summary>
        /// View and projection martix.
        /// </summary>
        private Matrix viewMatrix, projectionMatrix;

		/// <summary>
		/// Camera position.
		/// </summary>
		private Vector3 position = new Vector3(0,0,0);

		/// <summary>
		/// Quaternion used for rotation.
		/// </summary>
		private Quaternion orientation = Quaternion.Identity;

        /// <summary>
        /// Camera up vector. It can change, depends of the camera orientation.
        /// </summary>
        private Vector3 vectorUp = Vector3.Up;

        /// <summary>
        /// Used for velocity.
        /// </summary>
        private Vector3 lastPosition = Vector3.Zero;

		#endregion

		#region Properties

        #region Projection

        /// <summary>
        /// Field of View
        /// </summary>
        public float FieldOfView
        {
            get { return fieldOfView; }
            set
            {
                fieldOfView = value;
                BuildProjectionMatrix();
            }
        }

        /// <summary>
        /// Near Plane
        /// </summary>
        public float NearPlane
        {
            get { return nearPlane; }
            set
            {
                nearPlane = value;
                BuildProjectionMatrix();
            }
        }

        /// <summary>
        /// Far Plane
        /// </summary>
        public float FarPlane 
        {
            get { return farPlane;  }
            set
            {
                farPlane = value;
                BuildProjectionMatrix();
            }
        }

        #endregion

        #region Matrices

        /// <summary>
        /// View matrix. Se puede setear la matriz, pero solo se mantiene dicha matriz hasta la proxima actualizacion.
        /// Esta funcionalidad solo sirve para dar flexibilidad en una situacion atipica.
        /// </summary>
        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        } // ViewMatrix		

        /// <summary>
        /// Projection matrix. Se puede setear la matriz, pero solo se mantiene dicha matriz hasta la proxima actualizacion.
        /// Esta funcionalidad solo sirve para dar flexibilidad en una situacion atipica.
        /// </summary>
        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
            set { projectionMatrix = value; }
        } // ProjectionMatrix

        #endregion

        #region Position and Orientation

        /// <summary>
        /// Camera's position.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set
            {
                lastPosition = position;
                position = value;
            }
        } // Position

		/// <summary>
		/// Camera's orientation.
		/// </summary>
		public Quaternion Orientation
        {
            get { return orientation; }
            set { value.Normalize(); orientation = value; }
		} // Orientation

        /// <summary>
        /// Look at position. This value needs to be administrated correctly by the derivate classes. It can be an aproximate value, because this is helpfull for shadows and other stuff.
        /// </summary>
        public virtual Vector3 LookAtPosition { get; set; }

		/// <summary>
		/// Get current x axis with help of the current view matrix.
		/// </summary>
		public Vector3 XAxis { get { return new Vector3(viewMatrix.M11, viewMatrix.M21, viewMatrix.M31); } }

		/// <summary>
		/// Get current y axis with help of the current view matrix.
		/// </summary>
		public Vector3 YAxis { get { return new Vector3(viewMatrix.M12, viewMatrix.M22, viewMatrix.M32); } }

		/// <summary>
		/// Get current z axis with help of the current view matrix.
		/// </summary>
		public Vector3 ZAxis { get { return new Vector3(viewMatrix.M13, viewMatrix.M23, viewMatrix.M33); } }

        #endregion

        #endregion

        #region Constructor

		/// <summary>
		/// Create camera.
		/// </summary>
		public Camera(Vector3 _position, Vector3 _lookPosition = new Vector3())
		{
			// Calculate orientation and position
			SetLookAt(_position, _lookPosition, vectorUp);
            BuildProjectionMatrix();
		} // Camera

		#endregion
        
		#region Look at

		/// <summary>
		/// Helper method to get quaternion and position.
		/// </summary>
		public void SetLookAt(Vector3 _position, Vector3 _lookPosition, Vector3 upVector)
		{   
			// Build a look at matrix and get the quaternion from that
            Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(_position, _lookPosition, upVector));
            Position = _position;
            LookAtPosition = _lookPosition;
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

        #region Interpolation TODO!!!!!!!!!!!

        /// <summary>
        /// 
        /// </summary>
        private List<Interpolation> activeInterpolations = new List<Interpolation>();

        /// <summary>
        /// Estructura que guarda la informacioN necesaria para la interpolacion de posiciones de una camara.
        /// </summary>
        private struct Interpolation
        {
            // El cronometro que nos indica en que posicion de la interpolacion nos encontramos.
            public Chronometer chronometer;

            public Quaternion initialQuaternion;

            public Quaternion newQuaternion;

            public double duration;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreatesInterpolation(Quaternion _newQuaternion, double _duration)
        {
            Interpolation interpolation = new Interpolation();

            interpolation.chronometer = new Chronometer();
            interpolation.initialQuaternion = Orientation;
            interpolation.newQuaternion = _newQuaternion;
            interpolation.duration = _duration;
            interpolation.chronometer.Start();

            activeInterpolations.Add(interpolation);
        } // CreatesInterpolation

        /// <summary>
        /// Actualizo la lista de interpolaciones. En general son pequeños movimientos de camara para hacer la escena menos estatica.
        /// </summary>
        public void UpdateInterpolations()
        {
            List<Interpolation> interpolationsToErase = new List<Interpolation>();
            if (activeInterpolations.Count > 0)
                Orientation = Quaternion.Identity;
            foreach (Interpolation interpolation in activeInterpolations)
            {
                // Si la interpolacion termino es hora de borrarla.
                if (interpolation.chronometer.ElapsedTime > interpolation.duration)
                {
                    interpolationsToErase.Add(interpolation);
                }
                if (Orientation == Quaternion.Identity)
                {
                    Orientation = Quaternion.Slerp(interpolation.initialQuaternion, interpolation.newQuaternion, (float)(interpolation.chronometer.ElapsedTime / interpolation.duration));
                }
                else
                {
                    Orientation = Quaternion.Slerp(Orientation, Quaternion.Slerp(interpolation.initialQuaternion, interpolation.newQuaternion, (float)(interpolation.chronometer.ElapsedTime / interpolation.duration)), 0.5f);
                }
            }
            foreach (Interpolation interpolation in interpolationsToErase)
            {
                activeInterpolations.Remove(interpolation);
            }
        } // UpdateInterpolation

        #endregion

        #region Update view matrix

        /// <summary>
		/// Update view matrix
		/// </summary>
		public void UpdateViewMatrix()
		{
            // Actualizo la lista de interpolaciones. En general son pequeños movimientos de camara para hacer la escena menos estatica.
            UpdateInterpolations();
         
			Orientation.Normalize();
			Matrix rotMatrix = Matrix.CreateFromQuaternion(Orientation);
			
            viewMatrix = Matrix.CreateTranslation(-Position) * rotMatrix;
		} // UpdateViewMatrix()
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

        #region Build projection matrix

        /// <summary>
        /// Build projection matrix
        /// </summary>
        public void BuildProjectionMatrix()
        {
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfView, EngineManager.AspectRatio, nearPlane, farPlane);
        } // BuildProjectionMatrix

        #endregion

    } // Camera
} // XNAFinalEngine.GraphicElements
