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

float3 textureSize;
float3 textureSizeInv;

float4x4 worldViewProj;
float4x4 worldView;
float  farPlane;
float2 halfPixel;
float3 lightPosition;
float3 lightColor;
float  invLightRadius;
float  lightIntensity;

float3x3 viewI;

//////////////////////////////////////////////
//////////////// Textures ////////////////////
//////////////////////////////////////////////

texture  cubeShadowTexture: register(t3);
samplerCUBE cubeShadowSampler : register(s3) = sampler_state
{
    Texture = <cubeShadowTexture>;
    /*MinFilter = Point;//Linear;
    MagFilter = Point;//Linear;
    MipFilter = None;
    AddressU = Clamp;
    AddressV = Clamp;*/
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

float Lerp(float x, float y, float s)
{
	return x*(1-s) + y*s;
}

// This shader works in view space.
PixelShader_OUTPUT ps_main(VS_OUT input, uniform bool hasShadows)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
	
    // Obtain screen position
    input.screenPosition.xy /= input.screenPosition.w;
    // Obtain textureCoordinates corresponding to the current pixel
	// The screen coordinates are in [-1,1]*[1,-1]
    // The texture coordinates need to be in [0,1]*[0,1]
	// http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	// http://diaryofagraphicsprogrammer.blogspot.com.ar/2008/09/calculating-screen-space-texture.html
    float2 uv = 0.5f * (float2(input.screenPosition.x, -input.screenPosition.y) + 1) + halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
    
	// Reconstruct position from the depth value, making use of the ray pointing towards the far clip plane	
	float depth = tex2D(depthSampler, uv).r;
					
	// This is a ray constructed using the camera frustum.
    // Because it will be interpolated for the current pixel we can use
    // this to reconstruct the position of the surface we want to light.
    float3 frustumRayVS = input.viewPosition.xyz * (farPlane / -input.viewPosition.z);

	// Reconstruct the view space position of the surface to light.
    float3 positionVS = depth * frustumRayVS;
	
    // Surface-to-light vector (in view space)
    float3 L = lightPosition - positionVS; // Don't normalize, the attenuation function needs the distance.	
		
	float4 normalCompressed = tex2Dlod(normalSampler, float4(uv, 0, 0));
	float3 N = DecompressNormal(normalCompressed.xyz);

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

	// Process the shadow map value.
	float shadowTerm = 1.0;
		
	if (hasShadows) // No need for [branch], this is a uniform value.
	{	
		float3 direction = mul(normalize(L), viewI);
		direction.z = -direction.z;
		float limit = length(L) * invLightRadius - 0.01f;

		// Without filter
		shadowTerm = (limit < texCUBE(cubeShadowSampler, -normalize(direction)).x) ? 1.0f: 0.0f;

		// From Floating Point Cube Maps (ShaderX2 article)
		// I try Poison distribution, but it was a subtle improvement.
		// Multiply coordinates by the texture size
		float3 texPos = -normalize(direction) * textureSize;
		// Compute first integer coordinates
		float3 texPos0 = floor(texPos + float3(0.5, 0.5, 0.5));
		// Compute second integer coordinates
		float3 texPos1 = texPos0 + float3(1.0, 1.0, 1.0);
		// Perform division on integer coordinates
		texPos0 = texPos0 * textureSizeInv;
		texPos1 = texPos1 * textureSizeInv;
		// Compute contributions for each coordinate
		float3 blend = frac(texPos + float3(0.5, 0.5, 0.5));
		// Construct 8 new coordinates
		float3 texPos000 = texPos0;
		float3 texPos001 = float3(texPos0.x, texPos0.y, texPos1.z);
		float3 texPos010 = float3(texPos0.x, texPos1.y, texPos0.z);
		float3 texPos011 = float3(texPos0.x, texPos1.y, texPos1.z);
		float3 texPos100 = float3(texPos1.x, texPos0.y, texPos0.z);
		float3 texPos101 = float3(texPos1.x, texPos0.y, texPos1.z);
		float3 texPos110 = float3(texPos1.x, texPos1.y, texPos0.z);
		float3 texPos111 = texPos1;
		// Sample cube map
		float C000 = (limit < texCUBE(cubeShadowSampler, texPos000).r) ? 1.0f: 0.0f;
		float C001 = (limit < texCUBE(cubeShadowSampler, texPos001).r) ? 1.0f: 0.0f;
		float C010 = (limit < texCUBE(cubeShadowSampler, texPos010).r) ? 1.0f: 0.0f;
		float C011 = (limit < texCUBE(cubeShadowSampler, texPos011).r) ? 1.0f: 0.0f;
		float C100 = (limit < texCUBE(cubeShadowSampler, texPos100).r) ? 1.0f: 0.0f;
		float C101 = (limit < texCUBE(cubeShadowSampler, texPos101).r) ? 1.0f: 0.0f;
		float C110 = (limit < texCUBE(cubeShadowSampler, texPos110).r) ? 1.0f: 0.0f;
		float C111 = (limit < texCUBE(cubeShadowSampler, texPos111).r) ? 1.0f: 0.0f;

		// Compute final pixel value by lerping everything
		shadowTerm = Lerp(Lerp(Lerp(C000, C010, blend.y), Lerp(C100, C110, blend.y), blend.x), Lerp(Lerp(C001, C011, blend.y), Lerp(C101, C111, blend.y), blend.x), blend.z);

		[branch]
		if (shadowTerm == 0)
		{
			Discard();
			return (PixelShader_OUTPUT)0;
		}
	}
		
	// Fill the light buffer:
	// R: Color.r * N.L // The color need to be in linear space and right now it's in gamma.
	// G: Color.g * N.L
	// B: Color.b * N.L
	// A: Specular Term * N.L (Look in Shader X7 to know why N * L is necesary in this last channel or use your brain, it is easy actually.)
	// http://diaryofagraphicsprogrammer.blogspot.com/2008/03/light-pre-pass-renderer.html	
	output.diffuse = float4(GammaToLinear(lightColor) * attenuation * lightIntensity * NL * shadowTerm, 0);
	output.specular = float4(output.diffuse.rgb * specular, 0);
	return output;
} // ps_main

PixelShader_OUTPUT ps_mainStencil(VS_OUT input)
{
	return (PixelShader_OUTPUT)0;	
} // ps_mainStencil

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique PointLight
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main(false);
	}
} // PointLight

technique PointLightWithShadows
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main(true);
	}
} // PointLightWithShadows

technique PointLightStencil
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_mainStencil();
	}
} // PointLightStencil