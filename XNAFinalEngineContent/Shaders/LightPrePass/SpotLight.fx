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

#include <..\GBuffer\GBufferReader.fxh>
#include <..\Helpers\GammaLinearSpace.fxh>
#include <..\Helpers\Discard.fxh>
#include <..\Helpers\Attenuation.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float4x4 worldViewProj;
float4x4 worldView;
float4x4 viewToLightViewProj;

float2 halfPixel;
float farPlane;

float3 lightPosition;
float3 lightDirection;
float3 lightColor;
float invLightRadius;
float lightIntensity;
float lightInnerAngle;
float lightOuterAngle;

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

struct PixelShader_OUTPUT
{
    float4 diffuse          : COLOR0;
    float4 specular         : COLOR1;
};

//////////////////////////////////////////////
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
PixelShader_OUTPUT ps_main(uniform bool hasShadows, uniform bool hasLightMask, VS_OUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;

    // Obtain screen position
    input.screenPosition.xy /= input.screenPosition.w;

    // Obtain textureCoordinates corresponding to the current pixel
	// The screen coordinates are in [-1,1]*[1,-1]
    // The texture coordinates need to be in [0,1]*[0,1]
	// http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	// http://diaryofagraphicsprogrammer.blogspot.com.ar/2008/09/calculating-screen-space-texture.html
    float2 uv = 0.5f * (float2(input.screenPosition.x, -input.screenPosition.y) + 1) + halfPixel;
    
	// Process the shadow map value.
	float shadowTerm = 1.0;
	
	if (hasShadows) // No need for [branch], this is a uniform value.
	{
		shadowTerm = tex2D(shadowSampler, uv).r;	
		[branch]
		if (shadowTerm == 0)
		{
			Discard();
			return (PixelShader_OUTPUT)0;
		}
	}

	// Reconstruct position from the depth value, making use of the ray pointing towards the far clip plane	
	float depth = tex2D(depthSampler, uv).r;
	
	// This is a ray constructed using the camera frustum.
    // Because it will be interpolated for the current pixel we can use
    // this to reconstruct the position of the surface we want to light.
    float3 frustumRayVS = input.viewPosition.xyz * (farPlane / -input.viewPosition.z);

	// Reconstruct the view space position of the surface to light.
    float3 positionVS = depth * frustumRayVS;
	
	if (hasLightMask)
	{
		// Determine the depth of the pixel with respect to the light
		float4 positionLightCS = mul(float4(positionVS, 1), viewToLightViewProj);
		
		float depthLightSpace = positionLightCS.z / positionLightCS.w; // range 0 to 1
	
		// Transform from light space to shadow map texture space.
		float2 shadowTexCoord = 0.5 * positionLightCS.xy / positionLightCS.w + float2(0.5f, 0.5f);
		shadowTexCoord.y = 1.0f - shadowTexCoord.y;

		// This could be easily modified to support color texture projection.
		shadowTerm *= tex2D(lightMaskSampler, shadowTexCoord).r;
		
		/*[branch]
		if (shadowTerm == 0)
		{
			Discard();
		}*/
	}

    // Surface-to-light vector (in view space)
    float3 L = lightPosition - positionVS; // Don't normalize, the attenuation function needs the distance.	
	
	float4 normalCompressed = tex2Dlod(normalSampler, float4(uv, 0, 0));
	float3 N = DecompressNormal(normalCompressed.xyz);
	
	// Cone attenuation
	float DL          = dot(-lightDirection, normalize(L));
	float2 cosAngles  = cos(float2(lightOuterAngle, lightInnerAngle) * 0.5f);     
    DL               *= smoothstep(cosAngles[0], cosAngles[1], DL);
			
    // Compute diffuse light
    float NL = max(dot(N, normalize(L)), 0);

	[branch]
	if (NL == 0)
	{
		Discard();
		return (PixelShader_OUTPUT)0;
	}
	
	// Compute attenuation
	float attenuation = Attenuation(L, invLightRadius);

	// In "Experimental Validation of Analytical BRDF Models" (Siggraph2004) the autors arrive to the conclusion that half vector lobe is better than mirror lobe.
	float3 V = normalize(-positionVS);
	float3 H = normalize(V + normalize(L));
	// Compute specular light
    float specular = pow(saturate(dot(N, H)), DecompressSpecularPower(normalCompressed.w));

		// Fill the light buffer:
	// R: Color.r * N.L // The color need to be in linear space and right now it's in gamma.
	// G: Color.g * N.L
	// B: Color.b * N.L
	// A: Specular Term * N.L (Look in Shader X7 to know why N * L is necesary in this last channel or use your brain, it is easy actually.)
	// http://diaryofagraphicsprogrammer.blogspot.com/2008/03/light-pre-pass-renderer.html
	output.diffuse = float4(GammaToLinear(lightColor) * DL * attenuation * lightIntensity * NL * shadowTerm, 0);
	output.specular = float4(output.diffuse.rgb * specular, 0);
	return output;
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique SpotLight
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main(false, false);
	}
} // SpotLight

technique SpotLightWithMask
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main(false, true);
	}
} // SpotLight

technique SpotLightWithShadows
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main(true, false);
	}
} // SpotLight

technique SpotLightWithShadowsWithMask
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main(true, true);
	}
} // SpotLight
