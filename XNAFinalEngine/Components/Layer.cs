
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
using System;
#endregion

namespace XNAFinalEngine.Components
{
    /// <summary>
    /// Layers can be used for selective updating and rendering.
    /// </summary>
    /// <remarks>
    /// All layers are active by default. 
    /// However, the cameras’ culling masks by default have all layers active except the last one (normally used by the editor).
    /// </remarks>
    public class Layer
    {

        #region Variables
        
        // All layers.
        private static readonly Layer[] layerList = new Layer[32];

        private bool active = true;
        private bool visible = true;

        private static uint activeLayers;
        private static uint visibleLayers;
                               
        #endregion

        #region Properties

        /// <summary>
        /// Get the layer's number.
        /// A layer number is in the range [0...31]
        /// This helps to sort the layers and also reference the layer in the layer list.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Layer's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Layer Mask.
        /// </summary>
        public uint Mask { get; private set; }

        /// <summary>
        /// Is it active?
        /// </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                // The last layer is reserved and can not be disable.
                if (Number == 31)
                    return;
                active = value;
                // Update the active layers mask.
                if (value)
                    ActiveLayers = ActiveLayers | Mask;
                else
                    ActiveLayers = ActiveLayers & ~Mask;
            }
        } // Active

        /// <summary>
        /// Is it visible?
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set
            {
                // The last layer is reserved and can not be disable.
                if (Number == 31)
                    return;
                visible = value;
                // Update the visible layers mask.
                if (value)
                    VisibleLayers = VisibleLayers | Mask;
                else
                    VisibleLayers = VisibleLayers & ~Mask;
            }
        } // Visible
        
        /// <summary>
        /// Indicates the active layers in a mask format.
        /// The active layers are updated in each update stage.
        /// </summary>
        public static uint ActiveLayers
        {
            get { return activeLayers; }
            set
            {
                // The last layer is reserved and can not be disable.
                activeLayers = value | GetLayerByNumber(31).Mask;
            }
        } // ActiveLayers
        
        /// <summary>
        /// Indicates the visible layers in a mask format.
        /// The visible layers are rendered in each frame.
        /// </summary>
        public static uint VisibleLayers
        {
            get { return visibleLayers; }
            set
            {
                // The last layer is reserved and can not be disable.
                visibleLayers = value | GetLayerByNumber(31).Mask;
            }
        } // VisibleLayers

        /// <summary>
        /// The culling mask of the current camera.
        /// This is used in conjunction with the ActiveLayers property to indicate if a particular mask is active.
        /// </summary>
        public static uint CurrentCameraCullingMask { get; internal set; }

        #endregion

        #region Initialize Layers

        /// <summary>
        /// Init the layer array and base layer information.
        /// </summary>
        static Layer()
        {
            for (int i = 0; i < 32; i++)
            {
                layerList[i] = new Layer();
                if (i == 0)
                    layerList[0].Name = "Default Layer";
                else if (i == 31)
                    layerList[i].Name = "Editor Layer";
                else
                    layerList[i].Name = "Layer-" + i;
                layerList[i].Number = i;
                layerList[i].Mask = (uint)(Math.Pow(2, i));
            }
            ActiveLayers = uint.MaxValue;
            VisibleLayers = uint.MaxValue;
            CurrentCameraCullingMask = uint.MaxValue;
        } // Layer

        #endregion

        #region Constructor

        /// <summary>
        /// Layer private constructor.
        /// </summary>
        private Layer() { }

        #endregion
        
        #region Get Layer

        /// <summary>
        /// Get layer by number.
        /// </summary>
        /// <param name="layerNumber">In the range [0, 31]</param>
        public static Layer GetLayerByNumber(int layerNumber)
        {
            if (layerNumber < 0 || layerNumber > 31)
                throw new Exception("Layer System: Unable to return layer from layer number. The layer number is out of range.");
            return layerList[layerNumber];
        } // GetLayerByNumber

        /// <summary>
        /// Get layer by mask.
        /// </summary>
        public static Layer GetLayerByMask(uint layerMask)
        {
            
            for (int i = 0; i < 32; i++)
            {
                if (layerList[i].Mask == layerMask)
                    return layerList[i];
            }
            throw new Exception("Layer System: Unable to return layer from layer mask. The mask does not exist.");
            /*        
            // I should have to implement a faster version. // TODO!!!
            try
            {
                double doubleLayerNumber = ?????
                int layerNumber = (int)doubleLayerNumber;
                if (doubleLayerNumber - layerNumber != 0)
                    throw new Exception();
                return GetLayerByNumber(layerNumber);
            }
            catch
            {                
                throw new Exception("Layer System: Unable to return layer from layer mask. The mask does not exist.");
            }*/
        } // GetLayerByNumber

        /// <summary>
        /// Get layer by name.
        /// </summary>        
        public static Layer GetLayerByName(string layerName)
        {
            for (int i = 0; i < 32; i++)
            {
                if (layerList[i].Name == layerName)
                    return layerList[i];
            }
            throw new Exception("Layer System: Unable to return layer from layer name. The name does not exist.");        
        } // GetLayerByName

        #endregion

        #region Is Active and Visible

        /// <summary>
        /// Indicates if the layer is active.
        /// </summary>
        public static bool IsActive(uint layerMask)
        {
            return (layerMask & ActiveLayers) != 0;
        } // IsActive

        /// <summary>
        /// Indicates if the layer is visible (includes the current camera culling mask in the answer)
        /// </summary>
        public static bool IsVisible(uint layerMask)
        {
            return (layerMask & CurrentCameraCullingMask & VisibleLayers) != 0;
        } // IsVisible

        #endregion

    } // Layer
} // XNAFinalEngine.Components
