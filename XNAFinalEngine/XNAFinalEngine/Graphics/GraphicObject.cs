
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
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// A graphic object has a model or mesh, a material, and all the characteristics or properties of an object.
    /// </summary>
    public class GraphicObject : Object
    {

        #region Variables
        
        /// <summary>
        /// The underlying mesh.
        /// </summary>
        private readonly Model model;

        /// <summary>
        /// The shader or material used in the model.
        /// </summary>
        private Material material;

        #region Lights Associated

        /// <summary>
        /// Point Lights associated with the object.
        /// </summary>
        private PointLight[] pointLight;

        /// <summary>
        /// Directional Lights associated with the object.
        /// </summary>
        private DirectionalLight[] directionalLight;

        /// <summary>
        /// Spot Lights associated with the object.
        /// </summary>
        private SpotLight[] spotLight;

        #endregion

        #region Bounding Volumes

        /// <summary>
        /// The matrix when it bounding sphere was calculated.
        /// </summary>
        private Matrix? matrixWhenBoundingSphereWasCalculated;

        /// <summary>
        /// The matrix when it bounding box was calculated.
        /// </summary>
        private Matrix? matrixWhenBoundingBoxWasCalculated;

        /// <summary>
        /// The object's bounding sphere.
        /// </summary>
        private BoundingSphere boundingSphere;

        /// <summary>
        /// The object's bounding box.
        /// </summary>
        private BoundingBox boundingBox;

        #endregion
                
        #endregion

        #region Properties

        /// <summary>
        /// The shader or material used in the model.
        /// </summary>
        public Material Material
        {
            get { return material; }
            set { material = value; }
        } // Material

        /// <summary>
        /// The underlying mesh.
        /// </summary>
        public Model Model { get { return model; } }

        #region Bounding Volumes

        /// <summary>
        /// The bounding Sphere of the model.
        /// The bounding volume is re calculated when the position of the object changes.
        /// </summary>
        public override BoundingSphere BoundingSphere
        {
            get
            {
                Matrix worlMatrix = WorldMatrix;
                if (!matrixWhenBoundingSphereWasCalculated.HasValue || matrixWhenBoundingSphereWasCalculated.Value != worlMatrix)
                {
                    matrixWhenBoundingSphereWasCalculated = worlMatrix;
                    boundingSphere = model.BoundingSphere(worlMatrix);
                }
                return boundingSphere;
            }
        } // BoundingSphere

        /// <summary>
        /// The bounding Sphere of the model.
        /// Este bounding volume no se recalcula. Solo se traslada y escala. El resultado no es perfecto.
        /// Pero es suficientemente bueno y es altamente superior en performance.
        /// </summary>
        public override BoundingSphere BoundingSphereOptimized
        {
            get
            {   
                Matrix worlMatrix = WorldMatrix;
                if (!matrixWhenBoundingSphereWasCalculated.HasValue || matrixWhenBoundingSphereWasCalculated.Value != worlMatrix)
                {   
                    return model.BoundingSphereOptimized(worlMatrix);
                }
                return boundingSphere;
            }
        } // BoundingSphereOptimized
        
        /// <summary>
        /// The bounding Box of the model. Aligned to the X, Y, Z planes.
        /// The bounding volume is re calculated when the position of the object changes.
        /// </summary>
        public override BoundingBox BoundingBox
        {
            get
            {
                Matrix worlMatrix = WorldMatrix;
                if (!matrixWhenBoundingBoxWasCalculated.HasValue || matrixWhenBoundingBoxWasCalculated.Value != worlMatrix)
                {
                    matrixWhenBoundingBoxWasCalculated = worlMatrix;
                    boundingBox = model.BoundingBox(worlMatrix);
                }
                return boundingBox;
            }
        } // BoundingBox

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Create the object. Consist of a 3D Mesh, a position and a material.
        /// </summary>
        public GraphicObject(Model _model, Material _material)
        {
            model = _model;
            material = _material;
            // Set of the inicial position, rotation and scaling (use default values)
            UpdateObjectMatrix();
        } // GraphicObject

        /// <summary>
        /// Create the object. Consist of a 3D Mesh, a position and a material.
        /// </summary>
        public GraphicObject(string _model, Material _material)
        {
            model = new XModel(_model);
            material = _material;
            // Set of the inicial position, rotation and scaling (use default values)
            UpdateObjectMatrix();
        } // GraphicObject

        #endregion
        
        #region Lights Managment
        
        /// <summary>
        /// Associate a light to the object
        /// </summary>
        public override void AssociateLight(Light light)
        {
            if (light is PointLight)
            {
                if (pointLight != null)
                {
                    PointLight[] newPointLight = new PointLight[pointLight.Length + 1];
                    int i;
                    for (i = 0; i < (pointLight.Length); i++)
                    {
                        if (pointLight[i] == light) break;
                        newPointLight[i] = pointLight[i];
                    }
                    if (i == pointLight.Length)
                    {
                        newPointLight[pointLight.Length] = (PointLight)light;
                        pointLight = newPointLight;
                    }
                }
                else
                {
                    pointLight = new PointLight[1];
                    pointLight[0] = (PointLight)light;
                }
            } // if(is PointLight)
            else if (light is DirectionalLight)
            {
                if (directionalLight != null)
                {
                    DirectionalLight[] newDirectionalLight = new DirectionalLight[directionalLight.Length + 1];
                    int i;
                    for (i = 0; i < (directionalLight.Length); i++)
                    {
                        if (directionalLight[i] == light) break;
                        newDirectionalLight[i] = directionalLight[i];
                    }
                    if (i == directionalLight.Length)
                    {
                        newDirectionalLight[directionalLight.Length] = (DirectionalLight)light;
                        directionalLight = newDirectionalLight;
                    }
                }
                else
                {
                    directionalLight = new DirectionalLight[1];
                    directionalLight[0] = (DirectionalLight)light;
                }
            } // if(is DirectionalLight)
            else if (light is SpotLight)
            {
                if (spotLight != null)
                {
                    SpotLight[] newSpotLight = new SpotLight[spotLight.Length + 1];
                    int i;
                    for (i = 0; i < (spotLight.Length); i++)
                    {
                        if (spotLight[i] == light) break;
                        newSpotLight[i] = spotLight[i];
                    }
                    if (i == spotLight.Length)
                    {
                        newSpotLight[spotLight.Length] = (SpotLight)light;
                        spotLight = newSpotLight;
                    }
                }
                else
                {
                    spotLight = new SpotLight[1];
                    spotLight[0] = (SpotLight)light;
                }
            } // if(is SpotLight)
        } // AssociateLight

        /// <summary>
        /// DisassociateLight the light
        /// </summary>
        public override void DisassociateLight(Light light)
        {
            if (light is PointLight)
            {
                if (pointLight != null)
                {
                    PointLight[] newPointLight = new PointLight[pointLight.Length];
                    int i;
                    int cont = 0;
                    for (i = 0; i < (pointLight.Length); i++)
                    {
                        if (pointLight[i] != light)
                        {
                            if (i != pointLight.Length - 1 || i != cont)
                            {
                                newPointLight[cont] = pointLight[i];
                            }
                            cont++;
                        }
                    }
                    if (i != cont)
                    {
                        pointLight = cont == 0 ? null : newPointLight;
                    }
                }
            } // if(is PointLight)
            else if (light is DirectionalLight)
            {
                if (directionalLight != null)
                {
                    DirectionalLight[] newDirectionalLight = new DirectionalLight[directionalLight.Length];
                    int i;
                    int cont = 0;
                    for (i = 0; i < (directionalLight.Length); i++)
                    {
                        if (directionalLight[i] != light)
                        {
                            if (i != directionalLight.Length - 1 || i != cont)
                            {
                                newDirectionalLight[cont] = directionalLight[i];
                            }
                            cont++;
                        }
                    }
                    if (i != cont)
                    {
                        directionalLight = cont == 0 ? null : newDirectionalLight;
                    }
                }
            } // if(is DirectionalLight)
            else if (light is SpotLight)
            {
                if (spotLight != null)
                {
                    SpotLight[] newSpotLight = new SpotLight[spotLight.Length];
                    int i;
                    int cont = 0;
                    for (i = 0; i < (spotLight.Length); i++)
                    {
                        if (spotLight[i] != light)
                        {
                            if (i != spotLight.Length - 1 || i != cont)
                            {
                                newSpotLight[cont] = spotLight[i];
                            }
                            cont++;
                        }
                    }
                    if (i != cont)
                    {
                        spotLight = cont == 0 ? null : newSpotLight;
                    }
                }
            } // if(is SpotLight)
        } // DisassociateLight

        #endregion

        #region Render

        /// <summary>
        /// Render the object.
        /// </summary>
        public override void Render()
        {   
            material.Render(WorldMatrix, pointLight, directionalLight, spotLight, model);
        } // Render
                
        /// <summary>
        /// Render the object aplying a specific material. The picker uses this.
        /// </summary>
        public override void Render(Material _material)
        {   
            _material.Render(WorldMatrix, pointLight, directionalLight, spotLight, model);
        } // Render
        
        #endregion

    } // GraphicObject
} // XNAFinalEngine.GraphicElements
