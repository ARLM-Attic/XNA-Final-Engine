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
#include <..\Helpers\RGBM.fxh>
#include <..\Helpers\SphericalHarmonics.fxh>

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 worldIT       : WorldInverseTranspose;
float4x4 worldViewProj : WorldViewProjection;
float4x4 world         : World;

//////////////////////////////////////////////
//////////////// Parameters //////////////////
//////////////////////////////////////////////

float2 halfPixel;

float3 cameraPosition;

float3 diffuseColor;

float specularIntensity;

float specularPower;

float3 ambientColor;

float ambientIntensity;

float alphaBlending;

bool diffuseTextured;

//////////////////////////////////////////////
///////////////// Lights /////////////////////
//////////////////////////////////////////////

float3 directionalLightDirection;
float3 directionalLightColor;
float  directionalLightIntensity;
float3 pointLightPos = float3(-15, 50, -30);
float3 pointLightColor = float3(0.9, 0.9, 0.9);
float  pointLightIntensity = 0.35;

//////////////////////////////////////////////
///////////////// Options ////////////////////
//////////////////////////////////////////////

bool reflectionTextured;

bool hasDirectionalShadow;

bool hasAmbientSphericalHarmonics;

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

texture reflectionTexture : ENVIRONMENT;

samplerCUBE reflectionSampler = sampler_state
{
	Texture = <reflectionTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture shadowTexture;

sampler2D shadowSampler = sampler_state
{
	Texture = <shadowTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct vertexOutput
{
    float4 position		: POSITION;
    float4 positionProj : TEXCOORD0;  
    float3 normalWS		: TEXCOORD1;
    float3 viewWS       : TEXCOORD2;
	float2 uv           : TEXCOORD3;
    float3 pointLightVec: TEXCOORD4;
    //float3 spotLightVec : TEXCOORD5;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

vertexOutput vs_mainWithoutTexture(in float4 position : POSITION, in float3 normal : NORMAL)
{	
    vertexOutput output;
	
    output.position = mul(position, worldViewProj);
	output.positionProj = output.position;
    output.normalWS = mul(normal, worldIT).xyz;
	
	float3 positionWS = mul(position, world);

	// Light information
	float3 Pw = mul(position, world).xyz;		    	// world coordinates
    output.pointLightVec = pointLightPos - Pw;
    /*output.SpotLightVec  = SpotLightPos - Pw;*/

	// Texture coordinates
	output.uv = float2(0, 0);
		
	// Eye position - P
    output.viewWS = normalize(cameraPosition - positionWS);
    
    return output;
} // VSBlinn

vertexOutput vs_mainWithTexture(in float4 position : POSITION, in float3 normal : NORMAL, in float2 uv : TEXCOORD0)
{	
    vertexOutput output;
	
    output.position = mul(position, worldViewProj);
	output.positionProj = output.position;
    output.normalWS = mul(normal, worldIT).xyz;
	
	float3 positionWS = mul(position, world);

	// Light information
	float3 Pw = mul(position, world).xyz;		    	// world coordinates
    output.pointLightVec = pointLightPos - Pw;
    /*output.SpotLightVec  = SpotLightPos - Pw;*/
	
    // Texture coordinates
	output.uv = uv;
	
	// Eye position - P
    output.viewWS = normalize(cameraPosition - positionWS);
    
    return output;
} // VSBlinn

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 ps_main(vertexOutput input) : COLOR
{
	float3 normalWS  = normalize(input.normalWS);
    float3 viewWS    = normalize(input.viewWS);
    float3 diffuse;  // = float3(0,0,0);
    float3 specular; // = float3(0,0,0);	

	// Ambient Light //
	[branch]
	if (hasAmbientSphericalHarmonics)
		diffuse = ambientColor + SampleSH(normalWS) * ambientIntensity;
	else
		diffuse = ambientColor;
	// Albedo
	float3 materialColor;
	[branch]
	if (diffuseTextured)
	{
    	materialColor = tex2D(diffuseSampler, input.uv).rgb;
	}
	else
	{
		materialColor = diffuseColor;
	}
    // Directional Light //		
	float shadowTerm = 1.0;
	/*if (hasDirectionalShadow)
	{
		shadowTerm = tex2D(shadowSampler, PostProjectToScreen(input.positionProj) + halfPixel); // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	}*/
	float NL = max(dot(-directionalLightDirection, normalWS), 0);
    diffuse += GammaToLinear(directionalLightColor) * NL *  directionalLightIntensity * shadowTerm;
	// In "Experimental Validation of Analytical BRDF Models" (Siggraph2004) the autors arrive to the conclusion that half vector lobe is better than mirror lobe.
	float3 H  = normalize(viewWS - directionalLightDirection);
    specular = pow(saturate(dot(normalWS, H)), specularPower) * NL *  directionalLightIntensity * specularIntensity * shadowTerm;
	
	// Point Light //
    float3 L       = normalize(input.pointLightVec);
    H       = normalize(viewWS + L);
    float  HdN     = dot(H, normalWS);
    float  LdN     = dot(L, normalWS);
    float4 litVec  = lit(LdN, HdN, specularPower);
    diffuse        += litVec.y * GammaToLinear(pointLightColor) * pointLightIntensity;
    specular       += specularIntensity * litVec.z * pointLightIntensity;
	/*
    // Spot Light // 
   	/*L			     = normalize(IN.SpotLightVec);    
    float CosSpotAng = cos(SpotLightCone*(float)(3.141592/180.0));
    float DdL        = dot(normalize(-SpotLightDir), L);
    DdL              = ((DdL - CosSpotAng)/(((float)1.0) - CosSpotAng));
    if (DdL > 0)
    {
		H = normalize(V + L);
    	LdN = dot(L, N);    	
    	HdN = dot(H,N);
    	litVec = lit(LdN, HdN, SpecExponent);
		diffContrib +=  litVec.y * SpotLightIntensity * DdL * SpotLightColor;
    	specContrib +=  litVec.y * SpotLightIntensity * litVec.z * SpecIntensity * SpotLightColor;
    }*/
    
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
	return float4(GammaToLinear(materialColor) * diffuse + specular, alphaBlending);
} // PSBlinn

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique ForwardBlinnPhongWithoutTexture
{
    pass P0
    {
        VertexShader = compile vs_3_0 vs_mainWithoutTexture();
        PixelShader  = compile ps_3_0 ps_main();
    }
} // BlinnPhongWithoutTexture

technique ForwardBlinnPhongWithTexture
{
    pass P0
    {
        VertexShader = compile vs_3_0 vs_mainWithTexture();
        PixelShader  = compile ps_3_0 ps_main();
    }
} // BlinnPhongWithTexture

//#include <SkinnedBlinnPhong.fxh>

/*technique SkinnedForwardBlinnPhong
{
    pass P0
    {
        VertexShader = compile vs_3_0 SkinnedWithoutTexture();
        PixelShader  = compile ps_3_0 PSBlinn();
    }
} // SkinnedForwardBlinnPhong*/