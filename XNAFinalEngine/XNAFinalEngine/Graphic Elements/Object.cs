
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using System;
using System.Collections.Generic;
#endregion

namespace XNAFinalEngine.GraphicElements
{
	/// <summary>
    /// This abstract class is the father of the 3D objects (the container objects and the graphic objects).
    /// An object has a position, rotation, scale, lights, animations, and bounding volumes, and can be rendering.
    ///
    /// Important: if we are referring to position, rotation and scale the meaning of global is that these values take in consideration
    /// the hierarchy of the object, meanwhile local doesn’t.
    /// If we are referring to the transformations of these values the meaning of global is that these transformations are
    /// applied before the previous transformations, and local after.
	/// </summary>
    public abstract class Object
	{
        
        #region Variables

        /// <summary>
        /// The father of this object. An object only exists in a unique position, therefore can have only one father.
        /// </summary>
        protected ContainerObject father = null;

        /// <summary>
        /// The current active animations in use by the object.
        /// </summary>
        protected List<Animation> activeAnimations = new List<Animation>();

        /// <summary>
        /// Object's velocity (affected by animations).
        /// </summary>
        protected Vector3 velocity = Vector3.Zero;

        #region Place in the world

        /// <summary>
        /// Position, rotation and scaling of the object (local).
        /// </summary>
        protected Matrix objectMatrix;

        /// <summary>
        /// Position of the object (local).
        /// </summary>
        protected Vector3 position = Vector3.Zero;

        /// <summary>
        /// Rotation of the object (local).
        /// </summary>
        protected Matrix rotation = Matrix.Identity;

        /// <summary>
        /// Scale of the object (local).
        /// </summary>
        protected Vector3 scale = new Vector3(1, 1, 1);

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// The father of this object. An object only exists in a unique position, therefore can have only one father.
        /// </summary>
        public ContainerObject Father
        {
            get { return father; }
            set { father = value; }
        }

        /// <summary>
        /// Object's velocity (affected by animations).
        /// </summary>
        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        #region Place in the world
        
        /// <summary>
        /// Position, rotation and scaling of the object (local).
        /// </summary>
        public Matrix LocalMatrix
        {
            get { return objectMatrix; }
            set
            {
                Quaternion quaternion;
                objectMatrix = value;
                objectMatrix.Decompose(out scale, out quaternion, out position);
                rotation = Matrix.CreateFromQuaternion(quaternion);
            }
        }

        /// <summary>
        /// Position, rotation and scaling of the object (global).
        /// </summary>
        public Matrix WorldMatrix
        {
            get
            {
                if (father != null)
                {
                    return objectMatrix * father.WorldMatrix;
                }
                else
                    return objectMatrix;
            }
        } // WorldMatrix

        /// <summary>
        /// Position of the object (local).
        /// </summary>
        public Vector3 LocalPosition { get { return position; } }

        /// <summary>
        /// Position of the object (global). Takes rotations and scale in consideration.
        /// </summary>
        public Vector3 WorldPosition
        {
            get
            {
                if (father != null)
                    return WorldMatrix.Translation; 
                else
                    return position;
            }
        } // WorldPosition

        /// <summary>
        /// Rotation of the object (local).
        /// </summary>
        public Matrix LocalRotation { get { return rotation; } }

        /// <summary>
        /// Rotation of the object (global). Scales and translation doesn't affect it.
        /// </summary>
        public Matrix WorldRotation
        {
            get
            {
                if (father != null)
                    return rotation * father.WorldRotation; 
                else
                    return rotation;
            }
        } // WorldPosition

        /// <summary>
        /// Scale of the object (local)
        /// </summary>
        public Vector3 LocalScale { get { return scale; } }

        /// <summary>
        /// Scale of the object (global). Rotations and translation doesn't affect it.
        /// </summary>
        public Vector3 WorldScale
        {
            get
            {
                if (father != null)
                    return scale * father.WorldScale;
                else
                    return scale;
            }
        } // WorldScale

        #endregion

        #region Bounding Volumes

        /// <summary>
        /// The object's bounding sphere.
        /// The bounding volume is re calculated when the position of the object changes.
        /// </summary>
        public virtual BoundingSphere BoundingSphere { get { return new BoundingSphere(); /*Overrite it!*/ } }

        /// <summary>
        /// The object's bounding sphere.
        /// This bounding volume isn’t recalculated, it only translate and scale. 
        /// The result isn’t perfect but is good enough and is far superior in performance.
        /// </summary>
        public virtual BoundingSphere BoundingSphereOptimized { get { return new BoundingSphere(); /*Overrite it!*/ } }

        /// <summary>
        /// Object's center point. Uses the bouding Sphere Optimized.
        /// </summary>
        public Vector3 CenterPoint { get { return BoundingSphereOptimized.Center; } }

        /// <summary>
        /// The object's bounding box. Aligned to the X, Y, Z planes.
        /// For that reason the boxes are always re calculated.
        /// </summary>
        public virtual BoundingBox BoundingBox { get { return new BoundingBox(); /*Overrite it!*/ } }
                
        #endregion

        #endregion

        #region Position, Rotation and Scaling of the object

        /// <summary>
        /// Generates the Matrix worldObject using the vectors position, rotation and scaling.
        /// </summary>
        protected void UpdateObjectMatrix()
        {
            objectMatrix = Matrix.CreateScale(scale);
            objectMatrix *= rotation;
            objectMatrix *= Matrix.CreateTranslation(position);
        } // UpdateObjectMatrix

        #region Translate

        /// <summary>
        /// Moves the object in absolute form (no relative).
        /// </summary>
        public void TranslateAbs(float x, float y, float z)
        {
            position.X = x;
            position.Y = y;
            position.Z = z;
            UpdateObjectMatrix();
        } // TranslateAbs

        /// <summary>
        /// Moves the object in absolute form (no relative).
        /// </summary>
        public void TranslateAbs(Vector3 _position)
        {
            position = _position;
            UpdateObjectMatrix();
        } // TranslateAbs

        /// <summary>
        /// Moves the object in relative form (no absolute).
        /// </summary>
        public void TranslateRel(float x, float y, float z)
        {
            position.X += x;
            position.Y += y;
            position.Z += z;
            UpdateObjectMatrix();
        }  // TranslateRel

        /// <summary>
        /// Moves the object in relative form (no absolute).
        /// </summary>
        public void TranslateRel(Vector3 _position)
        {
            position += _position;
            UpdateObjectMatrix();
        }  // TranslateRel

        /// <summary>
        /// Moves the object in relative form (no absolute). 
        /// Uses local coordinates. In other words the operation is influenced by the object’s rotation.
        /// </summary>
        public void TranslateRelLocal(float x, float y, float z)
        {
            TranslateRelLocal(new Vector3(x, y, z));
        }  // TranslateRelLocal

        /// <summary>
        /// Moves the object in relative form (no absolute).
        /// Uses local coordinates. In other words the operation is influenced by the object’s rotation.
        /// </summary>
        public void TranslateRelLocal(Vector3 _position)
        {
            Quaternion quaternion;

            objectMatrix = Matrix.CreateScale(scale);
            objectMatrix *= Matrix.CreateTranslation(_position); // Lo muevo ahora, asi la rotacion lo afecta a continuacion.
            objectMatrix *= rotation;
            objectMatrix *= Matrix.CreateTranslation(position);
            // Re armo los valores de escala, posicion y rotacion
            LocalMatrix.Decompose(out scale, out quaternion, out position);
            rotation = Matrix.CreateFromQuaternion(quaternion);
        }  // TranslateRelLocal

        #endregion

        #region Scale

        /// <summary>
        /// Scale the object in absolute form (no relative).
        /// </summary>
        public void ScaleAbs(float _scale)
        {
            scale.X = scale.Y = scale.Z = _scale;
            UpdateObjectMatrix();
        }  // ScaleAbs

        /// <summary>
        /// Scale the object in absolute form (no relative).
        /// </summary>
        public void ScaleAbs(float x, float y, float z)
        {
            scale.X = x;
            scale.Y = y;
            scale.Z = z;
            UpdateObjectMatrix();
        }  // ScaleAbs

        /// <summary>
        /// Scale the object in absolute form (no relative).
        /// </summary>
        public void ScaleAbs(Vector3 _scale)
        {
            scale = _scale;
            UpdateObjectMatrix();
        }  // ScaleAbs

        /// <summary>
        /// Scale the object in relative form (no absolute). Sumando el valor dado.
        /// </summary>
        public void ScaleRelAdd(float _scale)
        {
            scale.X *= _scale;
            scale.Y *= _scale;
            scale.Z *= _scale;
            UpdateObjectMatrix();
        } // ScaleRelAdd

        /// <summary>
        /// Scale the object in relative form (no absolute). Multiplicando el valor dado.
        /// </summary>
        public void ScaleRelMultiply(float _scale)
        {
            scale.X *= _scale;
            scale.Y *= _scale;
            scale.Z *= _scale;
            UpdateObjectMatrix();
        } // ScaleRelMultiply

        /// <summary>
        /// Scale the object in relative form (no absolute) by adding the value.
        /// </summary>
        public void ScaleRelAdd(float x, float y, float z)
        {
            scale.X += x;
            scale.Y += y;
            scale.Z += z;
            UpdateObjectMatrix();
        } // ScaleRelAdd

        /// <summary>
        /// Scale the object in relative form (no absolute) by multiplying the value.
        /// </summary>
        public void ScaleRelMultiply(float x, float y, float z)
        {
            scale.X *= x;
            scale.Y *= y;
            scale.Z *= z;
            UpdateObjectMatrix();
        } // ScaleRelMultiply

        /// <summary>
        /// Scale the object in relative form (no absolute) by adding the value.
        /// </summary>
        public void ScaleRelAdd(Vector3 _scale)
        {
            scale *= _scale;
            UpdateObjectMatrix();
        } // ScaleRelAdd

        /// <summary>
        /// Scale the object in relative form (no absolute) by multiplying the value.
        /// </summary>
        public void ScaleRelMultiply(Vector3 _scale)
        {
            scale *= _scale;
            UpdateObjectMatrix();
        } // ScaleRelMultiply

        #endregion

        #region Rotate

        /// <summary>
        /// Rotate the object in absolute form (no relative). En grados.
        /// </summary>
        public void RotateAbs(float x, float y, float z)
        {   
            rotation = Matrix.CreateFromYawPitchRoll(y * (float)Math.PI / 180.0f,
                                                     x * (float)Math.PI / 180.0f,
                                                     z * (float)Math.PI / 180.0f);
            UpdateObjectMatrix();
        } // RotateAbs

        /// <summary>
        /// Rotate the object in absolute form (no relative). En grados.
        /// </summary>
        public void RotateAbs(Vector3 _rotation)
        {
            rotation = Matrix.CreateFromYawPitchRoll(_rotation.Y * (float)Math.PI / 180.0f,
                                                     _rotation.X * (float)Math.PI / 180.0f,
                                                     _rotation.Z * (float)Math.PI / 180.0f);
            UpdateObjectMatrix();
        } // RotateAbs

        /// <summary>
        /// Rotate the object in relative form (no absolute). En grados.
        /// </summary>
        public void RotateRel(float x, float y, float z)
        {
            rotation *= Matrix.CreateFromYawPitchRoll(y * (float)Math.PI / 180.0f,
                                                      x * (float)Math.PI / 180.0f,
                                                      z * (float)Math.PI / 180.0f);
            UpdateObjectMatrix();
        } // RotateRel

        /// <summary>
        /// Rotate the object in relative form (no absolute). En grados.
        /// </summary>
        public void RotateRel(Vector3 _rotation)
        {
            rotation *= Matrix.CreateFromYawPitchRoll(_rotation.Y * (float)Math.PI / 180.0f,
                                                      _rotation.X * (float)Math.PI / 180.0f,
                                                      _rotation.Z * (float)Math.PI / 180.0f);
            UpdateObjectMatrix();
        } // RotateRel

        /// <summary>
        /// Rotate the object in relative form (no absolute). En grados. Utiliza coordenadas locales.
        /// </summary>
        public void RotateRelLocal(float x, float y, float z)
        {
            rotation = Matrix.CreateFromYawPitchRoll(y * (float)Math.PI / 180.0f,
                                                     x * (float)Math.PI / 180.0f,
                                                     z * (float)Math.PI / 180.0f)
                       * rotation;
            UpdateObjectMatrix();
        } // RotateRelLocal

        /// <summary>
        /// Rotate the object in relative form (no absolute). En grados. Utiliza coordenadas locales.
        /// </summary>
        public void RotateRelLocal(Vector3 _rotation)
        {
            rotation = Matrix.CreateFromYawPitchRoll(_rotation.Y * (float)Math.PI / 180.0f,
                                                     _rotation.X * (float)Math.PI / 180.0f,
                                                     _rotation.Z * (float)Math.PI / 180.0f)
                       * rotation;
            UpdateObjectMatrix();
        } // RotateRelLocal

        /// <summary>
        /// Rotate the object in absolute form (no relative) using a quaternion.
        /// </summary>
        public void RotateAbsQuaternion(Quaternion quaternion)
        {
            rotation = Matrix.CreateFromQuaternion(quaternion);

            #region Interesting code for quaternions
            /*rotation.Y = (float)Math.Atan2(rotationMatrix.M11, rotationMatrix.M21) * 180.0f / (float)Math.PI;
            rotation.X = (float)Math.Atan2(Math.Sqrt(rotationMatrix.M32 * rotationMatrix.M32 + rotationMatrix.M33 * rotationMatrix.M33), -rotationMatrix.M31) * 180.0f / (float)Math.PI;
            rotation.Z = (float) Math.Atan2(rotationMatrix.M32, rotationMatrix.M33) * 180.0f / (float)Math.PI;*/
            #endregion

            UpdateObjectMatrix();
        } // RotateAbsQuaternion

        /// <summary>
        /// Rotate the object in absolute form (no relative) using a rotation matrix.
        /// </summary>
        public void RotateAbs(Matrix rot)
        {
            rotation = rot;
            UpdateObjectMatrix();
        } // RotateAbs

        /// <summary>
        /// Rotate the object in relative form (no absolute) using a rotation matrix.
        /// </summary>
        public void RotateRel(Matrix rot)
        {
            rotation *= rot;
            UpdateObjectMatrix();
        } // RotateRel

        /// <summary>
        /// Rotate the object in relative form (no absolute) using a rotation matrix. Utiliza coordenadas locales.
        /// </summary>
        public void RotateRelLocal(Matrix rot)
        {
            rotation = rot * rotation;
            UpdateObjectMatrix();
        } // RotateRelLocal

        /// <summary>
        /// Rotate the object to face the camera
        /// </summary>
        public void RotateBillboard()
        {
            rotation = Matrix.CreateBillboard(position, ApplicationLogic.Camera.Position, Vector3.Up, null);
            UpdateObjectMatrix();
        } // RotateBillboard

        #endregion

        #endregion

        #region Lights Managment

        /// <summary>
        /// Associates a light to the object.
        /// </summary>
        public virtual void AssociateLight(Light light)
        {
            // Overrit it!
        } // AssociateLight

        /// <summary>
        /// Disassociates  a light to the object.
        /// </summary>
        public virtual void DisassociateLight(Light light)
        {
            // Overrit it!
        } // DeassociateLight
        
        #endregion

        #region Animation Managment
        
        /// <summary>
        /// Associates an animation to the active animations list of the object.
        /// </summary>
        public void AssociateAnimation(Animation animation)
        {
            activeAnimations.Add(animation);
            animation.AssociateObject(this);
        } // AssociateAnimation

        /// <summary>
        /// Disassociates an existing animation to the active animations list of the object.
        /// </summary>
        public void DeassociateAnimation(Animation animation)
        {   
            activeAnimations.Remove(animation);
            //animation.DeassociateObject(this); // Be careful with this, it can’t use like this.
        } // DeassociateAnimation

        /// <summary>
        /// Disassociates all active animations.
        /// </summary>
        public void DeassociateAllAnimations()
        {
            foreach (Animation animation in activeAnimations)
            {
                animation.DeassociateObject(this);
            }
            activeAnimations = new List<Animation>();
        } // DeassociateAllAnimations
        
        #endregion

        #region Render

        /// <summary>
        /// Render the object.
        /// </summary>
        public virtual void Render()
        {
            // Overrite it!
        }

        /// <summary>
        /// Render the object applying a specific material. The picker uses this.
        /// </summary>        
        public virtual void Render(Material material)
        {
            // Overrite it!
        }

        #endregion
        
	} // Object
} // XNAFinalEngine.GraphicElements
