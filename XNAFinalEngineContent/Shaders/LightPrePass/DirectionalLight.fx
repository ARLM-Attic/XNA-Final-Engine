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

#include <..\GBuffer\GBufferReader.fxh>
#include <..\Helpers\GammaLinearSpace.fxh>
#include <..\Helpers\Discard.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel;
float3 frustumCorners[4];

float3 lightColor;
float3 lightDirection;
float  lightIntensity = 1;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

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

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position		: POSITION;
	float2 uv			: TEXCOORD0;
	float3 frustumRay	: TEXCOORD1;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

float3 FrustumRay(in float2 uv)
{
	float  index = uv.x + (uv.y * 2);
	return frustumCorners[index];
}

VS_OUT vs_main(in float4 position : POSITION, in float2 uv : TEXCOORD)
{
	VS_OUT output = (VS_OUT)0;
	
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	output.uv = uv; 

	output.frustumRay = FrustumRay(uv);
	
	return output;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

// This shader works in view space.
float4 ps_main(uniform bool hasShadows, in float2 uv : TEXCOORD0, in float3 frustumRay : TEXCOORD1) : COLOR0
{
	// Process the shadow map value.
	float shadowTerm = 1.0;
	
	if (hasShadows) // No need for [branch], this is a uniform value.
	{
		shadowTerm = tex2D(shadowSampler, uv).r;
		[branch]
		if (shadowTerm == 0)
		{
			Discard();
		}
	}

	// Reconstruct position from the depth value, making use of the ray pointing towards the far clip plane	
	float depth = tex2D(depthSampler, uv).r;

	[branch]
	if (depth == 1)
	{
		Discard();
	}
	
	float3 N = SampleNormal(uv);
	
	// Light vector
	float3 L = -lightDirection;
	
	// N dot L lighting term
	float NL = max(dot(L, N), 0); // Avoid negative values.

	[branch]
	if (NL == 0)
	{
		Discard();
	}

	float3 position = frustumRay * depth; // To convert this position into world space it only needs to add the camera position (in the pixel shader), and the frustumray multiply by the camera orientation (in the vertex shader).
	float3 V = normalize(-position);
	
	// Reflexion vector (mirror lobe)
    //float3 R = normalize(reflect(-L, N));
    // Compute specular light
    //float specular = pow(saturate(dot(R, V)), lightSpecularPower);
	
	// In "Experimental Validation of Analytical BRDF Models" (Siggraph2004) the autors arrive to the conclusion that half vector lobe is better than mirror lobe.
	float3 H  = normalize(V + L);
	// Compute specular light
    float specular = pow(saturate(dot(N, H)), DecompressSpecularPower(tex2D(motionVectorSpecularPowerSampler, uv).b));

	// Fill the light buffer:
	// R: Color.r * N.L // The color need to be in linear space and right now it's in gamma.
	// G: Color.g * N.L
	// B: Color.b * N.L
	// A: Specular Term * N.L (Look in Shader X7 to know why N * L is necesary in this last channel)
	// Also in Shader X7 talk about a new channel so that the material shininess could be controled better.
	// http://diaryofagraphicsprogrammer.blogspot.com/2008/03/light-pre-pass-renderer.html
	return float4(GammaToLinear(lightColor), specular) * NL *  lightIntensity * shadowTerm;
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique DirectionalLight
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main(false);
	}
} // DirectionalLight

technique DirectionalLightWithShadows
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main(true);
	}
} // DirectionalLight