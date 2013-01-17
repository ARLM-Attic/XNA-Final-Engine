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

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 ViewITProj;
float4x4 World   : World;
float4x4 ViewInv : ViewInverse;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float3 LightDirection = float3(0, -0.05, 1);
float fDensity = 0.0218;
float SunLightness = 50;
float sunRadiusAttenuation = 8000;
float largeSunLightness = 0.5;
float largeSunRadiusAttenuation = 48;
// Day To Sunset Sharpness (Controls the curve from Day to Sunset)
float dayToSunsetSharpness = 4;
float hazeTopAltitude = 5;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture diffuseTexture : register(t0);
sampler2D diffuseSampler : register(s0) = sampler_state
{
	Texture = <diffuseTexture>;
	/*MinFilter = ANISOTROPIC;
	MagFilter = ANISOTROPIC;
	MipFilter = NONE;
	AddressU = CLAMP;
	AddressV = CLAMP;*/
};

texture SkyTextureNight : register(t5);
sampler SurfSamplerSkyTextureNight : register(s5) = sampler_state
{
	Texture = <SkyTextureNight>;
	/*MinFilter = Anisotropic;	
	MagFilter = Anisotropic;
	MipFilter = Linear;
	AddressU = mirror; 
	AddressV = mirror;*/
};

texture SkyTextureSunset : register(t6);
sampler SurfSamplerSkyTextureSunset : register(s6) = sampler_state
{
	Texture = <SkyTextureSunset>;
	/*MinFilter = Anisotropic;	
	MagFilter = Anisotropic;
	MipFilter = Linear;
	AddressU = mirror; 
	AddressV = mirror;*/
};

texture SkyTextureDay : register(t7);
sampler SurfSamplerSkyTextureDay : register(s7) = sampler_state
{
	Texture = <SkyTextureDay>;		
	/*MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
	AddressU = mirror; 
	AddressV = mirror;*/
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VertexOutput
{
   	float4 Position	            : POSITION;
    float2 UV		            : TEXCOORD0;
	float3 WorldLightVec		: TEXCOORD1;
	float3 WorldEyeDirection	: TEXCOORD2;  	
  	half   Fog 					: TEXCOORD3; // Group this to worldLightVec
  	half   EyeAltitude 			: TEXCOORD4; // Group this to WorldEyeDirection
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VertexOutput skydomeVS(float4 position : POSITION, float2 uv : TEXCOORD0)
{
	VertexOutput OUT;
	OUT.Position = mul(position, ViewITProj).xyww;
    OUT.UV = uv;
		
	OUT.WorldLightVec = normalize(-LightDirection); // Could be easily optimized.
		
	OUT.WorldEyeDirection = ViewInv[3].xyz - position;
	
	// Camera Altitude
	OUT.EyeAltitude = 5;//ViewInv[3].y;
	  	  
 	float dist = length(OUT.WorldEyeDirection);
	OUT.Fog = (1.f/exp(pow(dist * fDensity, 2)));

	return OUT;
} // skydomeVS

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 skydomePS(VertexOutput IN) : COLOR
{   
	// RGB Gamma	
	float4 diffuseColor = tex2Dlod(diffuseSampler, float4(IN.UV, 0, 0)).rgba;
	diffuseColor.rgb = GammaToLinear(diffuseColor.rgb);
	
	// Calculate light/eye/normal vectors	
	float3 eyeVec = normalize(IN.WorldEyeDirection);
	float3 lightVec = normalize(IN.WorldLightVec);
	
	// Calculate sun highlight...	
	float sunHighlight = pow(max(0.00001f, dot(lightVec, -eyeVec)), sunRadiusAttenuation) * SunLightness;	
	// Calculate a wider sun highlight 
	float largeSunHighlight = pow(max(0.00001f, dot(lightVec, -eyeVec)), largeSunRadiusAttenuation) * largeSunLightness;
	
	// Calculate 2D angle between pixel to eye and sun to eye
	float3 flatLightVec = normalize(float3(lightVec.x, 0, lightVec.z));
	float3 flatEyeVec = normalize(float3(eyeVec.x, 0, eyeVec.z));
	float diff = dot(flatLightVec, -flatEyeVec);	
	
	// Based on camera altitude, the haze will look different and will be lower on the horizon.
	// This is simulated by raising YAngle to a certain power based on the difference between the
	// haze top and camera altitude. 
	// This modification of the angle will show more blue sky above the haze with a sharper separation.
	// Lerp between 0.25 and 1.25
	float val = lerp(0.25, 1.25, min(1, hazeTopAltitude / max(0.0001, IN.EyeAltitude)));
	// Apply the power to sharpen the edge between haze and blue sky
	float YAngle = saturate(pow(max(0.00001f, -eyeVec.y), val));
	
	// Fetch the 3 colors we need based on YAngle and angle from eye vector to the sun
	float3 fogColorDay    = GammaToLinear(tex2D(SurfSamplerSkyTextureDay,    float2(1 - (diff + 1) * 0.5, 1 - YAngle)));
	float3 fogColorSunset = pow(GammaToLinear(tex2D(SurfSamplerSkyTextureSunset, float2(1 - (diff + 1) * 0.5, 1 - YAngle))), 1);
	float3 fogColorNight  = GammaToLinear(tex2D(SurfSamplerSkyTextureNight,  float2(1 - (diff + 1) * 0.5, 1 - YAngle)));
	
	float3 fogColor;
	
	// If the light is above the horizon, then interpolate between day and sunset
	// Otherwise between sunset and night
	//[branch]
	if (lightVec.y > 0)
	{
		// Transition is sharpened with dayToSunsetSharpness to make a more realistic cut 
		// between day and sunset instead of a linear transition
		fogColor = lerp(fogColorDay, fogColorSunset, min(1, pow(abs(1 - lightVec.y), dayToSunsetSharpness)));
	}
	else
	{
		// Slightly different scheme for sunset/night.
		fogColor = lerp(fogColorSunset, fogColorNight, min(1, -lightVec.y * 4));
	}

	float3 colorOutput;
	
	// Add sun highlights
	fogColor += sunHighlight * fogColor + fogColor * largeSunHighlight;
    
	// Make sun brighter for the skybox...	
	colorOutput = pow(fogColor, 1.2);

	float intensity = pow(max(colorOutput.x, max(colorOutput.y, colorOutput.z)), 2);
	colorOutput = diffuseColor.rgb * diffuseColor.a * intensity + colorOutput * (1 - diffuseColor.a);
			
	return float4(colorOutput, 1);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique Skydome
{
	pass p0
	{
		VertexShader = compile vs_3_0 skydomeVS();
		PixelShader  = compile ps_3_0 skydomePS();
	}
}