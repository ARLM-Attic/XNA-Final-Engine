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

float4x4 worldViewProj;
float4x4 worldView;
float4x4 viewToLightViewProj;

float2 halfPixel;
float farPlane;
bool hasLightMask;

float3 lightPosition;
float3 lightDirection;
float3 lightColor;
float lightRadius;
float lightIntensity;
float lightInnerAngle;
float lightOuterAngle;	

bool insideBoundingLightObject;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture lightMaskTexture : register(t4);
sampler2D lightMaskSampler : register(s4) = sampler_state
{
	Texture = <lightMaskTexture>;
    /*ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = NONE;*/
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

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position			: POSITION;
	float4 screenPosition 	: TEXCOORD0;
	float4 viewPosition     : TEXCOORD1;
};

/////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_OUT vs_main(in float4 position : POSITION)
{
	VS_OUT output = (VS_OUT)0;
	
	output.position = mul(position, worldViewProj);
	
	output.screenPosition = output.position;

	output.viewPosition = mul(position, worldView);

    return output;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

// This shader works in view space.
float4 ps_main(uniform bool hasShadows, VS_OUT input) : COLOR0
{
    // Obtain screen position
    input.screenPosition.xy /= input.screenPosition.w;

    // Obtain textureCoordinates corresponding to the current pixel
	// The screen coordinates are in [-1,1]*[1,-1]
    // The texture coordinates need to be in [0,1]*[0,1]
    float2 uv = 0.5f * (float2(input.screenPosition.x, -input.screenPosition.y) + 1) + halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
    
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

	// Optimization. We can't implement stencil optimizations, but at least this will allow us to avoid the normal map fetch and some other calculations.
	if (insideBoundingLightObject)
	{
		[branch]
		if (depth > -input.viewPosition.z / farPlane)
		{
			Discard();
		}
	}
	else
	{
		[branch]
		if (depth < -input.viewPosition.z / farPlane)
		{
			Discard();
		}
	}
	
	// This is a ray constructed using the camera frustum.
    // Because it will be interpolated for the current pixel we can use
    // this to reconstruct the position of the surface we want to light.
    float3 frustumRayVS = input.viewPosition.xyz * (farPlane / -input.viewPosition.z);

	// Reconstruct the view space position of the surface to light.
    float3 positionVS = depth * frustumRayVS;
	
    // Surface-to-light vector (in view space)
    float3 L = lightPosition - positionVS; // Don't normalize, the attenuation function needs the distance.	
	float3 N = SampleNormal(uv);
	
	// Cone attenuation
	float DL           = dot(-lightDirection, normalize(L));
	float2 cosAngles = cos(float2(lightOuterAngle, lightInnerAngle) * 0.5f);     
    DL *= smoothstep(cosAngles[0], cosAngles[1], DL);
			
    // Compute diffuse light
    float NL = max(dot(N, normalize(L)), 0);
	
	// Compute attenuation
		// Basic attenuation
		//float attenuation = saturate(1.0f - dot(L, L) / pow(lightRadius, 2)); // length(L)
		// Natural light attenuation (http://fools.slindev.com/viewtopic.php?f=11&t=21&view=unread#unread)
		float attenuationFactor = 30;
		float attenuation = dot(L, L) / pow(lightRadius, 2);
		attenuation = 1 / (attenuation * attenuationFactor + 1);
		// Second we move down the function therewith it reaches zero at abscissa 1:
		attenuationFactor = 1/ (attenuationFactor + 1); //att_s contains now the value we have to subtract
		attenuation = max(attenuation - attenuationFactor, 0); // The max fixes a bug.
		// Finally we expand the equation along the y-axis so that it starts with a function value of 1 again.
		attenuation /= 1 - attenuationFactor;

	// In "Experimental Validation of Analytical BRDF Models" (Siggraph2004) the autors arrive to the conclusion that half vector lobe is better than mirror lobe.
	float3 V = normalize(-positionVS);
	float3 H  = normalize(V + normalize(L));
	// Compute specular light
    float specular = pow(saturate(dot(N, H)), DecompressSpecularPower(tex2D(motionVectorSpecularPowerSampler, uv).b));
	
	[branch]
	if (hasLightMask)
	{
		// Determine the depth of the pixel with respect to the light
		float4 positionLightCS = mul(float4(positionVS, 1), viewToLightViewProj);
		
		float depthLightSpace = positionLightCS.z / positionLightCS.w; // range 0 to 1
	
		// Transform from light space to shadow map texture space.
		float2 shadowTexCoord = 0.5 * positionLightCS.xy / positionLightCS.w + float2(0.5f, 0.5f);
		shadowTexCoord.y = 1.0f - shadowTexCoord.y;

		shadowTerm *= tex2D(lightMaskSampler, shadowTexCoord).r;
		// This could be easily modified to support color texture projection.
	}

	// Fill the light buffer:
	// R: Color.r * N.L // The color need to be in linear space and right now it's in gamma.
	// G: Color.g * N.L
	// B: Color.b * N.L
	// A: Specular Term * N.L (Look in Shader X7 to know why N * L is necesary in this last channel)
	// Also in Shader X7 talk about a new channel so that the material shininess could be controled better.
	// http://diaryofagraphicsprogrammer.blogspot.com/2008/03/light-pre-pass-renderer.html	    
	return float4(GammaToLinear(lightColor), specular) * DL * attenuation * lightIntensity * NL * shadowTerm;
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique SpotLight
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main(false);
	}
} // SpotLight

technique SpotLightWithShadows
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main(true);
	}
} // SpotLight
