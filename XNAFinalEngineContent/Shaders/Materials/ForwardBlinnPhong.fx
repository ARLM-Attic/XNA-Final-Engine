/***********************************************************************************************************************************************
Copyright (c) 2008-2013, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
#include <..\Helpers\Attenuation.fxh>

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 worldIT       : WorldInverseTranspose;
float4x4 worldViewProj : WorldViewProjection;
float4x4 world         : World;

//////////////////////////////////////////////
//////////////// Parameters //////////////////
//////////////////////////////////////////////

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
bool   hasShadows;
float2 halfPixel;
float3 pointLightPos;
float3 pointLightColor;
float  pointLightIntensity;
float  invPointLightRadius;
float3 pointLightPos2 ;
float3 pointLightColor2;
float  pointLightIntensity2 ;
float  invPointLightRadius2;
float3 spotLightPos;
float3 spotLightDirection;
float3 spotLightColor;
float  spotLightIntensity;
float  spotLightInnerAngle;
float  spotLightOuterAngle;
float  invSpotLightRadius;

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

texture shadowTexture : register(t3);
sampler2D shadowSampler : register(s3) = sampler_state
{
	Texture = <shadowTexture>;
    /*ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;*/
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

struct vertexOutput
{
    float4 position		  : POSITION;
    float4 screenPosition : TEXCOORD0;  
    float3 normalWS	 	  : TEXCOORD1;
    float3 viewWS         : TEXCOORD2;
	float2 uv             : TEXCOORD3;
    float3 pointLightVec  : TEXCOORD4;
	float3 pointLightVec2 : TEXCOORD5;
    float3 spotLightVec   : TEXCOORD6;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

vertexOutput vs_main(in float4 position : POSITION, in float3 normal : NORMAL, in float2 uv : TEXCOORD0)
{	
    vertexOutput output;
	
    output.position = mul(position, worldViewProj);
	output.screenPosition = output.position;
    output.normalWS = mul(normal, worldIT).xyz;
	
	float3 positionWS = mul(position, world);	

	// Light information	
    output.pointLightVec  = pointLightPos  - positionWS;
	output.pointLightVec2 = pointLightPos2 - positionWS;
    output.spotLightVec   = spotLightPos   - positionWS;
	
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
    float3 specular = 0;

	// Ambient Light //
	diffuse = ambientColor;	
	[branch]
	if (hasAmbientSphericalHarmonics)
		diffuse += SampleSH(normalWS);
	diffuse *= ambientIntensity;
    // Directional Light //
	float shadowTerm = 1.0;	
	[branch]
	if (hasShadows)
	{
		// Obtain screen position
		input.screenPosition.xy /= input.screenPosition.w;

		// Obtain textureCoordinates corresponding to the current pixel
		// The screen coordinates are in [-1,1]*[1,-1]
		// The texture coordinates need to be in [0,1]*[0,1]
		// http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
		// http://diaryofagraphicsprogrammer.blogspot.com.ar/2008/09/calculating-screen-space-texture.html
		float2 shadowUv = 0.5f * (float2(input.screenPosition.x, -input.screenPosition.y) + 1) + halfPixel;
		
		// TODO: Shadow calculations.		
	}
	float LN  = max(dot(-directionalLightDirection, normalWS), 0);
	float3 H  = normalize(viewWS - directionalLightDirection);
	float HN  = dot(H, normalWS);
	float3 diffuseContribution = GammaToLinear(directionalLightColor) * LN *  directionalLightIntensity * shadowTerm;
    diffuse += diffuseContribution;	
    specular = pow(saturate(dot(normalWS, H)), specularPower) * diffuseContribution;
	
	// Point Lights //
    float3 L       = normalize(input.pointLightVec);
    H              = normalize(viewWS + L);
    HN             = dot(H, normalWS);
    LN             = dot(L, normalWS);    	
	float attenuation = Attenuation(L, invPointLightRadius);
	diffuseContribution = LN * attenuation * pointLightIntensity * GammaToLinear(pointLightColor);
    diffuse        += diffuseContribution;
    specular       += pow(saturate(HN), specularPower) * diffuseContribution;

	L              = normalize(input.pointLightVec2);
    H              = normalize(viewWS + L);
    HN             = dot(H, normalWS);
    LN             = dot(L, normalWS);
	attenuation = Attenuation(L, invPointLightRadius2);
	diffuseContribution = LN * attenuation * pointLightIntensity * GammaToLinear(pointLightColor2);
    diffuse        += diffuseContribution;
    specular       += pow(saturate(HN), specularPower) * diffuseContribution;
	
    // Spot Light // 
   	L = normalize(input.spotLightVec);
	// Cone Attenuation	
    float2 cosAngles = cos(float2(spotLightOuterAngle, spotLightInnerAngle) * 0.5f);
    float DL         = dot(-spotLightDirection, L);
    DL              *= smoothstep(cosAngles[0], cosAngles[1], DL);
	// Light Attenuation
	attenuation = Attenuation(input.spotLightVec, invSpotLightRadius);
	// Compute specular light
	H = normalize(viewWS + L);
    LN = dot(L, normalWS);
    HN = dot(H, normalWS); 
	diffuseContribution = DL * LN * attenuation * spotLightIntensity * GammaToLinear(spotLightColor);
	diffuse  +=  diffuseContribution;
    specular +=  pow(saturate(HN), specularPower) * diffuseContribution;

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
	// Albedo
	float3 materialColor = tex2D(diffuseSampler, input.uv).rgb + diffuseColor; // Faster than if and a branch.			
	// View depend reflections are more realistic.
	float  envContribution = 1.0 - 0.5 * saturate(dot(normalWS, viewWS));
	// Final color (in linear space)
	return float4(GammaToLinear(materialColor) * diffuse + specular * envContribution * specularIntensity, alphaBlending);
} // PSBlinn

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique ForwardBlinnPhong
{
    pass P0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader  = compile ps_3_0 ps_main();
    }
} // ForwardBlinnPhong