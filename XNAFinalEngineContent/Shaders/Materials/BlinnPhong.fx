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

#include <..\Helpers\VertexAndFragmentDeclarations.fxh>
#include <..\Helpers\GammaLinearSpace.fxh>
#include <..\Helpers\RGBM.fxh>
#include <..\Helpers\ParallaxMapping.fxh>
#include <..\Helpers\SkinningCommon.fxh>

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 world         : World;
float4x4 worldIT       : WorldInverseTranspose;
float4x4 worldViewProj : WorldViewProjection;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel;
float3 diffuseColor;
float  specularIntensity;
float3 cameraPosition;

//////////////////////////////////////////////
///////////////// Options ////////////////////
//////////////////////////////////////////////

bool diffuseTextured;
bool specularTextured;
bool reflectionTextured;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture diffuseTexture : register(t0);
sampler2D diffuseSampler : register(s0) = sampler_state
{
	Texture = <diffuseTexture>;
	/*MinFilter = ANISOTROPIC;
	MagFilter = ANISOTROPIC;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;*/
};

texture diffuseAccumulationTexture : register(t1);
sampler2D diffuseAccumulationSampler : register(s1) = sampler_state
{
	Texture = <diffuseAccumulationTexture>;
	/*MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;*/
};

texture specularAccumulationTexture : register(t5);
sampler2D specularAccumulationSampler : register(s5) = sampler_state
{
	Texture = <specularAccumulationTexture>;
	/*MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;*/
};

texture specularTexture : register(t2);
sampler2D specularSampler : register(s2) = sampler_state
{
	Texture = <specularTexture>;
	/*ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MinFilter = ANISOTROPIC;
	MagFilter = ANISOTROPIC;
	MipFilter = LINEAR;*/
};

texture normalTexture : register(t3);
sampler2D normalSampler : register(s3) = sampler_state
{
	Texture = <normalTexture>;
    /*ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = ANISOTROPIC; //LINEAR;
	MINFILTER = ANISOTROPIC; //LINEAR;
	MIPFILTER = LINEAR;*/
};

texture reflectionTexture : register(t4);
samplerCUBE reflectionSampler : register(s4) = sampler_state
{
	Texture = <reflectionTexture>;
	/*MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = CLAMP;
	AddressV = CLAMP;*/
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position : POSITION;
	float2 uv		: TEXCOORD0;
	float4 postProj : TEXCOORD1;
	float3 normalWS : TEXCOORD2;
	float3 viewWS   : TEXCOORD3;
};

struct VS_OUTTangent
{
	float4 position         : POSITION0;    
    float2 uv			    : TEXCOORD0;
	float4 postProj         : TEXCOORD1;
    float2 parallaxOffsetTS : TEXCOORD2;
    float3 viewWS           : TEXCOORD3;
    float3x3 tangentToWorld : TEXCOORD4;	
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_OUT vs_main(in float4 position : POSITION, in float3 normal : NORMAL, in float2 uv : TEXCOORD0)
{
	VS_OUT output = (VS_OUT)0;

	output.position = mul(position, worldViewProj);
	output.postProj = output.position;	
	output.normalWS = mul(normal, worldIT);
	output.viewWS   = normalize(cameraPosition - mul(position, world));
	output.uv = uv;
	
	return output;
} // vs_main

VS_OUTTangent vs_mainWithTangent(WithTangentVS_INPUT input)
{
	VS_OUTTangent output = (VS_OUTTangent)0;

	output.position = mul(input.position, worldViewProj);
	output.postProj = output.position;
	output.uv = input.uv;	
		   
	// Generate the tanget space to view space matrix
	output.tangentToWorld[0] = mul(input.tangent,  worldIT);
	output.tangentToWorld[1] = mul(input.binormal, worldIT); // binormal = cross(input.tangent, input.normal)
	output.tangentToWorld[2] = mul(input.normal,   worldIT);

	output.viewWS   = normalize(cameraPosition - mul(input.position, world));

	// Compute the ray direction for intersecting the height field profile with current view ray.

	float3 viewTS = mul(output.tangentToWorld, output.viewWS);
         
	// Compute initial parallax displacement direction:
	float2 parallaxDirection = normalize(viewTS.xy);
       
	// The length of this vector determines the furthest amount of displacement:
	float fLength        = length( viewTS );
	float parallaxLength = sqrt( fLength * fLength -viewTS.z * viewTS.z ) / viewTS.z; 
       
	// Compute the actual reverse parallax displacement vector:
	// Need to scale the amount of displacement to account for different height ranges in height maps.
	// This is controlled by an artist-editable parameter heightMapScale.
	output.parallaxOffsetTS = parallaxDirection * parallaxLength * heightMapScale;

	return output;
} // vs_mainWithTangent

VS_OUT vs_Skinned(in float4 position : POSITION,
	                      in float3 normal   : NORMAL,
						  in float2 uv       : TEXCOORD0,
						  in int4 indices    : BLENDINDICES0,
						  in int4 weights  : BLENDWEIGHT0)
{
	VS_OUT output = (VS_OUT)0;

	SkinTransform(position, normal, indices, weights, 4);
	output.position = mul(position, worldViewProj);
	output.postProj = output.position;	
	output.normalWS = mul(normal, worldIT);
	output.viewWS   = normalize(cameraPosition - mul(position, world));
	output.uv = uv;
	
	return output;
} // vs_Skinned

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float2 PostProjectToScreen(float4 pos)
{
	float2 screenPosition = pos.xy / pos.w;
	// Screen position to uv coordinates.
	return (0.5f * (float2(screenPosition.x, -screenPosition.y) + 1));
}

float4 ps_main(in float2 uv : TEXCOORD0, in float4 positionProj : TEXCOORD1, in float3 normalWS : TEXCOORD2, in float3 viewWS : TEXCOORD3) : COLOR
{
	// Find the screen space texture coordinate & offset
	float2 lightMapUv = PostProjectToScreen(positionProj) + halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	
	// Diffuse contribution + specular contribution.
	float3 diffuseAccumulation = tex2D(diffuseAccumulationSampler, lightMapUv);
	float3 specularAccumulation = tex2D(specularAccumulationSampler, lightMapUv);

	viewWS = normalize(viewWS);
	normalWS = normalize(normalWS);
	
	// Final Color Calculations //
	float3 materialColor;
	float3 specular;
	// Albedo
	[branch]
	if (diffuseTextured)
	{
    	materialColor = tex2D(diffuseSampler, uv).rgb;
	}
	else
	{
		materialColor = diffuseColor;
	}
	// Specular
	[branch]
	if (specularTextured)
	{
    	specular = tex2D(specularSampler, uv).rgb * specularIntensity;
	}
	else
	{
		specular = specularIntensity;
	}
	// Reflection
	[branch]
	if (reflectionTextured)
	{
		float3 reflectionDir = normalize(reflect(viewWS, normalWS));	
		[branch]
		if (isRGBM)
			specular *= RgbmLinearToFloatLinear(GammaToLinear(texCUBE(reflectionSampler, reflectionDir).rgba));
		else
			specular *= GammaToLinear(texCUBE(reflectionSampler, reflectionDir).rgb);
	}
	// Final color (in linear space)
	return float4(GammaToLinear(materialColor) * diffuseAccumulation.rgb + specular * specularAccumulation.rgb,  1);
} // ps_main

float4 ps_mainWithParrallax(VS_OUTTangent input) : COLOR
{
	// Find the screen space texture coordinate & offset
	float2 lightMapUv = PostProjectToScreen(input.postProj) + halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	
	// Diffuse contribution + specular contribution.
	float3 diffuseAccumulation = tex2D(diffuseAccumulationSampler, lightMapUv);
	float3 specularAccumulation = tex2D(specularAccumulationSampler, lightMapUv);

	input.viewWS = normalize(input.viewWS);

	float2 uv = CalculateParallaxUV(input.uv, input.parallaxOffsetTS, input.viewWS, input.tangentToWorld, normalSampler);
	
	// Final Color Calculations //
	float3 materialColor;
	float3 specular;
	// Albedo
	[branch]
	if (diffuseTextured)
	{
    	materialColor = GammaToLinear(tex2D(diffuseSampler, uv).rgb);
	}
	else
	{
		materialColor = diffuseColor;
	}
	// Specular
	[branch]
	if (specularTextured)
	{
    	specular = tex2D(specularSampler, uv).rgb * specularIntensity;
	}
	else
	{
		specular = specularIntensity;
	}		
	// Reflection
	[branch]
	if (reflectionTextured)
	{	
		float3 normalWS = 2.0 * tex2D(normalSampler, uv).rgb - 1;
		normalWS = normalize(mul(normalWS, input.tangentToWorld));

		float3 reflectionDir = normalize(reflect(input.viewWS, normalWS));
		[branch]
		if (isRGBM)
			specular *= RgbmLinearToFloatLinear(GammaToLinear(texCUBE(reflectionSampler, reflectionDir).rgba));
		else
			specular *= GammaToLinear(texCUBE(reflectionSampler, reflectionDir).rgb);
	}
	// Final color (in linear space)
	return float4(materialColor * diffuseAccumulation.rgb + specular * specularAccumulation.rgb,  1);
} // ps_mainWithParrallax

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique BlinnPhongSimple
{
    pass P0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader  = compile ps_3_0 ps_main();
    }
} // BlinnPhongWithTexture

technique BlinnPhongWithParrallax
{
    pass P0
    {
        VertexShader = compile vs_3_0 vs_mainWithTangent();
        PixelShader  = compile ps_3_0 ps_mainWithParrallax();
    }
} // BlinnPhongWithTangent

technique BlinnPhongSkinned
{
    pass P0
    {
        VertexShader = compile vs_3_0 vs_Skinned();
        PixelShader  = compile ps_3_0 ps_main();
    }
} // BlinnPhongSkinned