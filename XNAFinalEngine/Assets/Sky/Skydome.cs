
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

namespace XNAFinalEngine.Assets
{
	/// <summary>
	/// The sky is render in a dome, a primitive that has more detail in the horizon and has a shape more suitable for shading.
	/// </summary>
	/// <remarks>
	/// In a skydome implementation we can:
	/// • Use a big and detail texture. We can achieve an extremely realistic looking, but the texture is difficult to make and you can’t have a dynamic sky.
	///   Also, if it contains clouds, they must be very distant clouds; otherwise as the viewer moves around a landscape, they would notice the same clouds staying overhead.
	///   You can create dynamic cloud for the close ones, but they are difficult to blend with the static clouds from the skydome.
	///   Codemaster’s Ego engine (Dirt and Grid) uses this technique and with Low Dynamic Range (LDR) textures.
	/// • Create a procedural sky. In general, daytime sky color is a gradual fade from "horizon color" to "zenith color" overhead.
	///   For example, a clear blue sky usually goes from a light cyan at the horizon to a rich medium-blue. 
	///   At dusk and dawn, there is an additional fade of warm color from a broad stretch of the east/west horizon to the zenith.
	///   By choosing an attractive set of colors, we can just interpolate between them for all times of day and night.
	///   The colors of a sunset are typically: yellow at the edge, then orange-red, then purple-slate blue.
	///   All clouds should be rendered using a dynamic technique.
	/// • Use a hybrid approach. A big texture represents some information of the sky and the rest is procedural.
	///   For example Ubisoft Romania’s H.A.W.X. uses a big texture with different information in each of the four channels; 
	///   this information mixed with some parameters produces the general look of the sky, far clouds included. 
	///   Criterion’s Hot Pursuit uses a LDR textures but the fourth channel is used (I think) to create a mask that mix the procedural and texture parts.
	/// 
	/// I will try to implement the Criterion method, but any method is valid. You should implement the right one for you game.
	/// 
	/// Important: if you use a 1 pixel texture with 0 in the fourth channel you have a procedural sky,
	/// if you use a Codemaster type of texture you will have a complete texture approach. 
	/// </remarks>
    public class Skydome : Sky
	{

        #region Variables

        // The count of materials for naming purposes.
        private static int nameNumber = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Cube map texture.
        /// </summary>
        public Texture Texture { get; set; }

        #endregion

        #region Constructor

        public Skydome()
        {
            Name = "Skydome-" + nameNumber;
            nameNumber++;
        } // Skydome

        #endregion

    } // Skydome
} // XNAFinalEngine.Assets
