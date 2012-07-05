
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.UserInterface;
#endregion

namespace XNAFinalEngine.Editor
{
    internal static class PrimitiveControls
    {
        
        /// <summary>
        /// Creates the configuration controls of this asset.
        /// </summary>
        public static void AddControls(Sphere asset, Window owner, bool assetCreation)
        {
            GroupBox groupGeneral = CommonControls.Group("General", owner);

            var radiusTextBox = CommonControls.TextBox("Radius", groupGeneral, asset.Radius.ToString(), asset, "Radius");
            var stacksTextBox = CommonControls.TextBox("Stacks", groupGeneral, asset.Stacks.ToString(), asset, "Stacks");
            var slicesTextBox = CommonControls.TextBox("Slices", groupGeneral, asset.Slices.ToString(), asset, "Slices");

            groupGeneral.AdjustHeightFromChildren();

        } // AddControls

        /// <summary>
        /// Creates the configuration controls of this asset.
        /// </summary>
        public static void AddControls(Box asset, Window owner, bool assetCreation)
        {
            GroupBox groupGeneral = CommonControls.Group("General", owner);

            var widthTextBox = CommonControls.TextBox("Width", groupGeneral, asset.Width.ToString(), asset, "Width");
            var heightTextBox = CommonControls.TextBox("Height", groupGeneral, asset.Height.ToString(), asset, "Height");
            var depthTextBox = CommonControls.TextBox("Depth", groupGeneral, asset.Depth.ToString(), asset, "Depth");

            groupGeneral.AdjustHeightFromChildren();

        } // AddControls


        /// <summary>
        /// Creates the configuration controls of this asset.
        /// </summary>
        public static void AddControls(Plane asset, Window owner, bool assetCreation)
        {
            GroupBox groupGeneral = CommonControls.Group("General", owner);

            var widthTextBox = CommonControls.TextBox("Width", groupGeneral, asset.Width.ToString(), asset, "Width");
            var heightTextBox = CommonControls.TextBox("Height", groupGeneral, asset.Height.ToString(), asset, "Height");

            groupGeneral.AdjustHeightFromChildren();

        } // AddControls

        /// <summary>
        /// Creates the configuration controls of this asset.
        /// </summary>
        public static void AddControls(Cone asset, Window owner, bool assetCreation)
        {
            GroupBox groupGeneral = CommonControls.Group("General", owner);

            var radiusTextBox = CommonControls.TextBox("Radius", groupGeneral, asset.Radius.ToString(), asset, "Radius");
            var lengthTextBox = CommonControls.TextBox("Length", groupGeneral, asset.Length.ToString(), asset, "Length");
            var slicesTextBox = CommonControls.TextBox("Slices", groupGeneral, asset.Slices.ToString(), asset, "Slices");

            groupGeneral.AdjustHeightFromChildren();

        } // AddControls

        /// <summary>
        /// Creates the configuration controls of this asset.
        /// </summary>
        public static void AddControls(Cylinder asset, Window owner, bool assetCreation)
        {
            GroupBox groupGeneral = CommonControls.Group("General", owner);

            var radiusTextBox = CommonControls.TextBox("Radius", groupGeneral, asset.Radius.ToString(), asset, "Radius");
            var lengthTextBox = CommonControls.TextBox("Length", groupGeneral, asset.Length.ToString(), asset, "Length");
            var slicesTextBox = CommonControls.TextBox("Slices", groupGeneral, asset.Slices.ToString(), asset, "Slices");

            groupGeneral.AdjustHeightFromChildren();

        } // AddControls

    } // PrimitiveControls
} // XNAFinalEngine.Editor