/***********************************************************************************************************************************************
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

Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
************************************************************************************************************************************************/

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

// Lookup Table
float scale; // (lutSize - 1.0) / lutSize
float offset; // 1.0 / (2.0 * lutSize)

// Adjust Levels
float inputBlack;
float inputWhite;
float inputGamma;
float outputBlack;
float outputWhite;

// Adjust Levels Individual Channels.
float3 inputBlackRGB;
float3 inputWhiteRGB;
float3 inputGammaRGB;
float3 outputBlackRGB;
float3 outputWhiteRGB;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture3D firstlookupTableTexture : register(t5);;
sampler3D firstlookupTableSampler : register(s5) = sampler_state
{
	Texture = <firstlookupTableTexture>;
	// Trilinear filter is mandatory for this texture. http://http.developer.nvidia.com/GPUGems2/gpugems2_chapter24.html
	/*MipFilter = LINEAR;
	MagFilter = LINEAR;
	MinFilter = LINEAR;*/
};

texture3D secondlookupTableTexture: register(t6); ;
sampler3D secondlookupTableSampler: register(s6) = sampler_state
{
	Texture = <secondlookupTableTexture>;
	// Trilinear filter is mandatory for this texture. http://http.developer.nvidia.com/GPUGems2/gpugems2_chapter24.html
	/*MipFilter = LINEAR;
	MagFilter = LINEAR;
	MinFilter = LINEAR;*/
};

//////////////////////////////////////////////
//////////////// Functions ///////////////////
//////////////////////////////////////////////

float3 AdjustLevels(float3 color)
{
	float3 inputLevels = pow(saturate(color - inputBlack) / (inputWhite - inputBlack), inputGamma);
	return float3(inputLevels * (outputWhite - outputBlack) + outputBlack);
} // AdjustLevels

float3 AdjustLevelsIndividualChannels(float3 color)
{
	float3 inputLevels = saturate(color - inputBlackRGB) / (inputWhiteRGB - inputBlackRGB);
	 inputLevels = float3(pow(inputLevels.r, inputGammaRGB.r), pow(inputLevels.g , inputGammaRGB.g), pow(inputLevels.b , inputGammaRGB.b));
	return float3(inputLevels * (outputWhiteRGB - outputBlackRGB) + outputBlackRGB);
} // AdjustLevelsIndividualChannels

float3 TransformColor(float3 color, sampler3D lookupTableSampler)
{
	//Edge offset (see http://http.developer.nvidia.com/GPUGems2/gpugems2_chapter24.html)
	//float3 scale = (lutSize - 1.0) / lutSize;
	//float3 offset = 1.0 / (2.0 * lutSize);
		
	return tex3D(lookupTableSampler, scale * color + offset);
} // TransformColor