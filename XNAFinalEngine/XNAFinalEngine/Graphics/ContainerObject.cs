
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// A container object helps to form an object’s hierarchy, and in the same time facilitated the assignment of common parameters to various objects.
    /// A container has a list of graphic objects and a list of container objects.
    /// The children calculate their place in the world with fathers’ world matrix.
    /// </summary>
    public class ContainerObject : Object
    {
        
        #region Variables

        /// <summary>
        /// The container objects that are children to this container.
        /// </summary>
        private readonly List<ContainerObject> containerObjectsChildren = new List<ContainerObject>();
       
        /// <summary>
        /// The graphic objects that are children to this container.
        /// </summary>
        private readonly List<GraphicObject> graphicObjectsChildren = new List<GraphicObject>();

        #endregion

        #region Properties

        /// <summary>
        /// The container objects that are children to this container.
        /// </summary>
        public List<ContainerObject> ContainerObjectsChildren { get { return containerObjectsChildren; } }

        /// <summary>
        /// The graphic objects that are children to this container.
        /// </summary>
        public List<GraphicObject> GraphicObjectsChildren { get { return graphicObjectsChildren; } }
        
        #region Bounding Volumes

        /// <summary>
        /// The object's bounding sphere.
        /// The bounding volume is re calculated when the position of the object changes.
        /// </summary>
        public override BoundingSphere BoundingSphere
        {
            get
            {
                BoundingSphere boundingSphere = new BoundingSphere();
                foreach (GraphicObject graphicObj in graphicObjectsChildren)
                {
                    boundingSphere = BoundingSphere.CreateMerged(boundingSphere, graphicObj.BoundingSphere);
                }
                // Go through his children
                foreach (ContainerObject containerObject in containerObjectsChildren)
                {
                    boundingSphere = BoundingSphere.CreateMerged(boundingSphere, containerObject.BoundingSphere);
                }
                return boundingSphere;
            }
        } // BoundingSphere

        /// <summary>
        /// The object's bounding sphere.
        /// This bounding volume isn’t recalculated, it only translate and scale. 
        /// The result isn’t perfect but is good enough and is far superior in performance.
        /// </summary>
        public override BoundingSphere BoundingSphereOptimized
        {
            get
            {
                BoundingSphere boundingSphere = new BoundingSphere();
                foreach (GraphicObject graphicObj in graphicObjectsChildren)
                {
                    boundingSphere = BoundingSphere.CreateMerged(boundingSphere, graphicObj.BoundingSphereOptimized);
                }
                // Go through his children
                foreach (ContainerObject containerObject in containerObjectsChildren)
                {
                    boundingSphere = BoundingSphere.CreateMerged(boundingSphere, containerObject.BoundingSphereOptimized);
                }
                return boundingSphere;
            }
        } // BoundingSphereOptimized

        /// <summary>
        /// The object's bounding Box. Aligned to the X, Y, Z planes.
        /// For that reason the boxes are always re calculated.
        /// </summary>
        public override BoundingBox BoundingBox
        {
            get
            {
                BoundingBox boundingBox = new BoundingBox();
                foreach (GraphicObject graphicObj in graphicObjectsChildren)
                {
                    boundingBox = BoundingBox.CreateMerged(boundingBox, graphicObj.BoundingBox);
                }
                // Go through his children
                foreach (ContainerObject containerObject in containerObjectsChildren)
                {
                    boundingBox = BoundingBox.CreateMerged(boundingBox, containerObject.BoundingBox);
                }
                return boundingBox;
            }
        } // BoundingBox

        #endregion
        
        #endregion

        #region Constructor

        /// <summary>
        /// Create a empty container object.
        /// </summary>
        public ContainerObject()
        {
            // Set of the inicial position, rotation and scaling
            UpdateObjectMatrix();
        } // ContainerObject

        #endregion

        #region Add and Remove Objects

        /// <summary>
        /// Remove a object from this container.
        /// </summary>
        public void RemoveObject(Object obj)
        {
            if (obj is GraphicObject)
            {
                graphicObjectsChildren.Remove((GraphicObject)obj);
            }
            else
            {
                containerObjectsChildren.Remove((ContainerObject)obj);
            }
            obj.Father = null;
        } // RemoveObject

        /// <summary>
        /// Add a object to this container.
        /// </summary>
        public void AddObject(Object obj)
        {
            if (obj is GraphicObject)
            {
                graphicObjectsChildren.Add((GraphicObject)obj);
            }
            else
            {
                containerObjectsChildren.Add((ContainerObject)obj);
            }
            obj.Father = this;
        } // AddObject
        
        #endregion

        #region Lights Managment

        /// <summary>
        /// Associate a light to all current childrens.
        /// </summary>
        public override void AssociateLight(Light light)
        {
            foreach (GraphicObject graphicObj in graphicObjectsChildren)
            {
                graphicObj.AssociateLight(light);
            }
            // Go through his children
            foreach (ContainerObject containerObject in containerObjectsChildren)
            {
                containerObject.AssociateLight(light);
            }
        } // AssociateLight

        /// <summary>
        /// Disassociate a light to all current childrens.
        /// </summary>
        public override void DisassociateLight(Light light)
        {
            foreach (GraphicObject graphicObj in graphicObjectsChildren)
            {
                graphicObj.DisassociateLight(light);
            }
            // Go through his children
            foreach (ContainerObject containerObject in containerObjectsChildren)
            {
                containerObject.DisassociateLight(light);
            }
        } // DisassociateLight

        #endregion

        #region Set Material to all childrens

        /// <summary>
        /// Set a material to all current childrens.
        /// </summary>
        public void SetMaterialToAllChildrens(Material material)
        {   
            foreach (GraphicObject graphicObj in graphicObjectsChildren)
            {
                graphicObj.Material = material;
            }
            // Go through his children
            foreach (ContainerObject containerObject in containerObjectsChildren)
            {
                containerObject.SetMaterialToAllChildrens(material);
            }
        } // SetMaterialToAllChildrens

        #endregion

        #region Render

        /// <summary>
        /// Render the object.
        /// </summary>
        public override void Render()
        {
            foreach (GraphicObject graphicObj in graphicObjectsChildren)
            {
                graphicObj.Render();
            }
            // Go through his children
            foreach (ContainerObject containerObject in containerObjectsChildren)
            {
                containerObject.Render();
            }
        } // Render

        /// <summary>
        /// Render the object applying a specific material. The picker uses this.
        /// </summary>
        public override void Render(Material _material)
        {
            foreach (GraphicObject graphicObj in graphicObjectsChildren)
            {
                graphicObj.Render(_material);
            }
            // Go through his children
            foreach (ContainerObject containerObject in containerObjectsChildren)
            {
                containerObject.Render(_material);
            }
        } // Render

        #endregion

    } // ContainerObject
} // XNAFinalEngine.GraphicElements