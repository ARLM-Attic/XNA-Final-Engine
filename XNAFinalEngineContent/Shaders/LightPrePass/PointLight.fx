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

float2 halfPixel;

float farPlane;

float3 lightPosition;

float3 lightColor;

float invLightRadius;

float lightIntensity;

bool insideBoundingLightObject;

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
float4 ps_main(VS_OUT input) : COLOR0
{
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
		
	[branch]
	if (depth == 1)
	{
		Discard();
	}

	float lightDepth = -input.viewPosition.z / farPlane;
	// Optimization. We can't implement stencil optimizations, but at least this will allow us to avoid the normal map fetch and some other calculations.
	if (insideBoundingLightObject)
	{		
		[branch]
		if (depth > lightDepth)
		{
			Discard();
		}
	}
	else
	{		
		[branch]
		if (depth < lightDepth) 
		// || depth > lightDepth + (lightRadius / farPlane)) // With this we can discard more fragments, but I have to convert lightRadius/farplane to uniform (global) first.
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
		
	float4 normalCompressed = tex2Dlod(normalSampler, float4(uv, 0, 0));
	float3 N = DecompressNormal(normalCompressed.xyz);

    // Compute diffuse light
    float NL = max(dot(N, normalize(L)), 0);

	[branch]
	if (NL == 0)
	{
		Discard();
	}

	// Compute attenuation
	float attenuation = Attenuation(L, invLightRadius);

	// In "Experimental Validation of Analytical BRDF Models" (Siggraph2004) the autors arrive to the conclusion that half vector lobe is better than mirror lobe.
	float3 V = normalize(-positionVS);
	float3 H  = normalize(V + normalize(L));
	// Compute specular light
    float specular = pow(saturate(dot(N, H)), DecompressSpecularPower(normalCompressed.w));

    /*// Reflexion vector
	// Camera-to-surface vector
    float3 V = normalize(-positionVS);
    float3 R = reflect(-V, N);
	// Compute specular light
    float specular = pow(saturate(dot(L, R)), specularPower);*/
		
	// Fill the light buffer:
	// R: Color.r * N.L // The color need to be in linear space and right now it's in gamma.
	// G: Color.g * N.L
	// B: Color.b * N.L
	// A: Specular Term * N.L (Look in Shader X7 to know why N * L is necesary in this last channel)
	// Also in Shader X7 talk about a new channel so that the material shininess could be controled better.
	// http://diaryofagraphicsprogrammer.blogspot.com/2008/03/light-pre-pass-renderer.html	    
	return float4(GammaToLinear(lightColor), specular) * attenuation * lightIntensity * NL;
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique PointLight
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main();
	}
} // PointLight
