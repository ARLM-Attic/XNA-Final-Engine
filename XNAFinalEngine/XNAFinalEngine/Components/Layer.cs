
using System.Collections.Specialized;

namespace XnaFinalEngine.Components
{
    public struct Layer
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
        /// This helps to sort the layers and also reference the layer in the layer list.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Layer's name.
        /// </summary>
        public string Name { get; set; }

        public BitVector32 LayerBitMask { get; set; }

        #endregion

        #region Constructor

        #endregion

        /// <summary>
        /// Get layer by number.
        /// </summary>
        public static Layer GetLayerByNumber(int layerNumber)
        {
            return layerList[layerNumber];
        } // GetLayerByNumber

        /// <summary>
        /// Init the layer array and base layer information.
        /// </summary>
        internal static void InitLayers()
        {
            for (int i = 0; i < 32; i++)
            {
                if (i == 0)
                    layerList[0].Name = "Default Layer";
                else
                    layerList[i].Name = "Layer-" + i;
                layerList[i].Number = i;
            }
        } // InitLayers

    } // Layer
} // XnaFinalEngine.Components
