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

#include <..\Helpers\GammaLinearSpace.fxh>
#include <..\Helpers\RGBM.fxh>

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 ViewITProj;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float alphaBlending = 1.0f;
float intensity = 1;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture CubeMapTexture : register(t0);
samplerCUBE CubeMapSampler : register(s0) = sampler_state
{
    Texture = <CubeMapTexture>;
    /*MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
	AddressU = CLAMP;
	AddressV = CLAMP;
	AddressW = CLAMP;*/
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct CubeVertexOutput
{
   	float4 Position	: POSITION;
    float3 UV		: TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

CubeVertexOutput CubeVS(float4 position : POSITION)
{
	CubeVertexOutput OUT;
	OUT.Position = mul(position, ViewITProj).xyww;
    OUT.UV = position.xyz;
	return OUT;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 CubePS(CubeVertexOutput IN) : COLOR
{   
	// RGB Gamma
	float3 rgb = texCUBE(CubeMapSampler, IN.UV).rgb;

	return float4(GammaToLinear(rgb) * intensity, alphaBlending);
}

float4 CubePSRGBM(CubeVertexOutput IN) : COLOR
{   
    // RGBM (gamma)
	float4 rgbm = texCUBE(CubeMapSampler, IN.UV).rgba;
		
	// RGBM (linear)
	rgbm = GammaToLinear(rgbm);
	
	// RGBM To Linear
	return float4(RgbmLinearToFloatLinear(rgbm) * intensity, alphaBlending);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique Skybox
{
	pass p0
	{
		VertexShader = compile vs_3_0 CubeVS();
		PixelShader  = compile ps_3_0 CubePS();
	}
}

technique SkyboxRGBM
{
	pass p0
	{
		VertexShader = compile vs_3_0 CubeVS();
		PixelShader  = compile ps_3_0 CubePSRGBM();
	}
}