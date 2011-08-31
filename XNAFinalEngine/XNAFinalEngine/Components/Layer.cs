
#region Using directives
using System;
#endregion

namespace XnaFinalEngine.Components
{
    /// <summary>
    /// Layers can be used for selective rendering from cameras
    /// </summary>
    public class Layer
    {

        #region Variables

        /// <summary>
        /// Layer list.
        /// </summary>
        private static readonly Layer[] layerList = new Layer[32];
                               
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
        public int Mask { get; private set; }

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
        public static Layer GetLayerByNumber(int layerNumber)
        {
            if (layerNumber < 0 || layerNumber > 31)
                throw new Exception("Layer System: Unable to return layer from layer number. The layer number is out of range.");
            return layerList[layerNumber];
        } // GetLayerByNumber

        /// <summary>
        /// Get layer by mask.
        /// </summary>
        public static Layer GetLayerByMask(int layerMask)
        {
            // I have to try which is faster.
            /*for (int i = 0; i < 32; i++)
            {
                if (layerList[i].Mask == layerMask)
                    return layerList[i];
            }
            throw new Exception("Layer System: Unable to return layer from layer mask. The mask does not exist.");
            */           
            try
            {
                double doubleLayerNumber = Math.Sqrt(layerMask);
                int layerNumber = (int)doubleLayerNumber;
                if (doubleLayerNumber - layerNumber != 0)
                    throw new Exception();
                return GetLayerByNumber(layerNumber);
            }
            catch
            {                
                throw new Exception("Layer System: Unable to return layer from layer mask. The mask does not exist.");
            }
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

        #region Init Layers

        /// <summary>
        /// Init the layer array and base layer information.
        /// </summary>
        internal static void InitLayers()
        {
            for (int i = 0; i < 32; i++)
            {
                layerList[i] = new Layer();
                if (i == 0)
                    layerList[0].Name = "Default Layer";
                else
                    layerList[i].Name = "Layer-" + i;
                layerList[i].Number = i;
                layerList[i].Mask = (int)(Math.Pow(2, i));
            }
        } // InitLayers

        #endregion

    } // Layer
} // XnaFinalEngine.Components
