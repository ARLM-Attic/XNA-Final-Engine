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

#include <..\GBuffer\GBufferReader.fx>
#include <..\Helpers\GammaLinearSpace.fxh>
#include <..\Helpers\SphericalHarmonics.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel;

float4x4 viewI;

float intensity = 0.1f;

float ambientOcclusionStrength;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture ambientOcclusionTexture;

sampler2D ambientOcclusionSample = sampler_state
{
    Texture = <ambientOcclusionTexture>;
    MinFilter = Point;
    MagFilter = Point;
	MipFilter = none;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture lightMapTexture;

sampler2D lightMapSample = sampler_state
{
    Texture = <lightMapTexture>;
    MinFilter = Point;
    MagFilter = Point;
	MipFilter = none;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position		: POSITION;
	float2 uv			: TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_OUT vs_main(in float4 position : POSITION, in float2 uv : TEXCOORD)
{
	VS_OUT output = (VS_OUT)0;
	
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	output.uv = uv; 
	
	return output;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

// This shader works in view space.
float4 ps_mainSH(in float2 uv : TEXCOORD0) : COLOR0
{	
	float3 N = SampleNormal(uv);

	// Normal (view space) to world space
	N = normalize(mul(N, viewI));
	
	return float4(SampleSH(N) * intensity, 0);
} // ps_mainSH

// This shader works in view space.
float4 ps_mainSHSSAO(in float2 uv : TEXCOORD0) : COLOR0
{	
	float3 N = SampleNormal(uv);

	// Normal (view space) to world space
	N = normalize(mul(N, viewI));	

	float ssao = tex2D(ambientOcclusionSample, uv).r;
		
	return float4(SampleSH(N) * intensity * pow(ssao, ambientOcclusionStrength), 0);
} // ps_mainSHSSAO

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique AmbientLightSH
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_mainSH();
	}
} // AmbientLightSH

technique AmbientLightSHSSAO
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_mainSHSSAO();
	}
} // AmbientLightSHSSAO