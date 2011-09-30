/***********************************************************************************************************************************************
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 worldViewProj : WorldViewProjection;

//////////////////////////////////////////////
//////////////// Surface /////////////////////
//////////////////////////////////////////////

float3 diffuseColor;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture diffuseTexture;

sampler2D diffuseSampler = sampler_state
{
	Texture = <diffuseTexture>;
	MinFilter = ANISOTROPIC;
	MagFilter = ANISOTROPIC;
	MipFilter = Linear;
	AddressU = MIRROR;
	AddressV = MIRROR;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct vertexInput
{
    float4 position	: POSITION;
    float4 uv		: TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct vertexOutput
{
    float4 position	: POSITION;
};

struct vertexOutputWT
{
    float4 position	: POSITION;
	float2 uv		: TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

vertexOutput VSConstantWithoutTexture(float4 position : POSITION)
{	
    vertexOutput output;
    output.position = mul(position, worldViewProj);
    return output;
}

vertexOutputWT VSConstantWithTexture(vertexInput input)
{	
    vertexOutputWT output;
    output.position = mul(input.position, worldViewProj);	
	output.uv = input.uv;
    return output;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 PSConstantWithoutTexture() : COLOR
{
    return float4(GammaToLinear(diffuseColor), 1);
}

float4 PSConstantWithTexture(vertexOutputWT input) : COLOR
{
    return float4(GammaToLinear(tex2D(diffuseSampler, input.uv).rgb), 1);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique ConstantWithoutTexture
{
	pass P0
	{
		VertexShader = compile vs_3_0 VSConstantWithoutTexture();
		PixelShader = compile ps_3_0 PSConstantWithoutTexture();
	}
}

technique ConstantWithTexture
{
	pass P0
	{
		VertexShader = compile vs_3_0 VSConstantWithTexture();
		PixelShader = compile ps_3_0 PSConstantWithTexture();
	}
}