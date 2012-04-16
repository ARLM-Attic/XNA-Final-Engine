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

// References:
// http://content.gpwiki.org/index.php/D3DBook:High-Dynamic_Range_Rendering
// http://pixelstoomany.wordpress.com/2008/07/05/another-day-another-hdr-rendering-trick-and-some-hope-for-the-future/
// http://mynameismjp.wordpress.com/2010/04/30/a-closer-look-at-tone-mapping/
// High Dynamic Range Imaging - Acquisition, Display, and Image-Based Lighting (Reinhard, Ward, Pattanaik)

#include <ColorCorrection.fxh>
#include <Filmgrain.fxh>
#include <ToneMapping.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel;

// Auto Exposure
bool autoExposure;
float tau;
float timeDelta; // frame time delta in seconds.
float luminanceLowThreshold;
float luminanceHighThreshold;
// Manual exposure 
float lensExposure; // fraction of light to display

// Bloom scale.
float bloomScale;
bool bloomEnabled;
bool adjustLevelsEnabled;
bool adjustLevelsIndividualChannelsEnabled;

// Lookup Tables
bool colorCorrectOneLutEnabled;
bool colorCorrectTwoLutEnabled;
float lerpLookupTablesAmount;
float lerpOriginalColorAmount;

// Film grain
bool filmGrainEnabled;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture sceneTexture : register(t9);
sampler2D sceneSampler : register(s9) = sampler_state
{
	Texture = <sceneTexture>;
	/*MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;*/
};

texture2D bloomTexture : register(t10);
sampler2D bloomSampler : register(s10) = sampler_state
{
	Texture = <bloomTexture>;
    /*MipFilter = LINEAR;
	MagFilter = ANISOTROPIC;
	MinFilter = ANISOTROPIC;*/
};

texture lastLuminanceTexture : register(t12);
sampler2D lastLuminanceSampler : register(s12) = sampler_state
{
	Texture = <lastLuminanceTexture>;
	/*MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;*/
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
//////////////// Functions ///////////////////
//////////////////////////////////////////////

float3 ExposureColor(float3 color, float2 uv)
{
	float exposure;
	[branch]
	if (autoExposure)
	{
		// From MJP http://mynameismjp.wordpress.com/ License: Microsoft_Permissive_License
	 	// The mip maps are used to obtain a median luminance value.
		float avgLuminance = exp(tex2Dlod(lastLuminanceSampler, float4(uv, 0, 10)).x); // This could be used to convert this global operator to local.
		// Use geometric mean
		avgLuminance = max(avgLuminance, 0.001f);		
		float keyValue = 1.03f - (2.0f / (2 + log10(avgLuminance + 1)));
		float linearExposure = (keyValue / avgLuminance);
		exposure = max(linearExposure, 0.0001f);
	}
    else		
		exposure = exp2(lensExposure);

	// Multiply the incomming light by the lens exposure value. Think of this in terms of a camera:
	// Exposure time on a camera adjusts how long the camera collects light on the main sensor.
	// This is a simple multiplication factor of the incomming light.	
	return color * exposure;
} // ExposureColor

// Apply the bloom effect.
float3 Bloom(float3 color, float2 uv)
{	
	// From Xen License: Microsoft_Permissive_License

	float4 bloomSample = tex2D(bloomSampler, uv);
	
	// Because the bloom texture is 8bit RGBA and is heavily blurred, it's very tricky to store the difference
	// between a moderate bloom and an extremely bright bloom spot (such as the sun).
	// So, the alpha channel of the bloom texture stores a 'bloom booster', storing bloom intensity * 0.1, so extra bright bloom is kept.
	
	float bloomBoost = bloomSample.a * 10;
	float3 bloom = bloomSample.rgb * bloomScale * (1 + bloomBoost);
	
	// Add bloom, using additive.
	// Except, in order to prevent the bloom from blowing out, multiply it by the inverse
	// of the colour it's being added to. So the output won't go above 1.	
	return bloom * saturate(1 - color.rgb);
} // Bloom

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
} // vs_main

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

// Creates the luminance map for the scene
float4 psLuminanceMap(in float2 uv : TEXCOORD0) : COLOR0
{
    // Sample the input
    float3 color = tex2D(sceneSampler, uv);
   
    // calculate the luminance using a weighted average
    float luminance = max(dot(color, float3(0.299f, 0.587f, 0.114f)), 0.0001f);

	// To avoid underexposure or overexposure I use thresholds. Simple but it works.	
	luminance = max(luminance, luminanceLowThreshold);
	luminance = min(luminance, luminanceHighThreshold);
                
    return float4(luminance, 1.0f, 1.0f, 1.0f);
} // psLuminanceMap

// Slowly adjusts the scene luminance based on the previous scene luminance
float4 psAdaptLuminance(in float2 uv : TEXCOORD0) : COLOR0
{
    float lastLum = exp(tex2D(lastLuminanceSampler, uv).r);
    float currentLum = tex2D(sceneSampler, uv).r;	
		       
    // Adapt the luminance using Pattanaik's technique    
    float adaptedLum = lastLum + (currentLum - lastLum) * (1 - exp(-timeDelta * tau));
    
    return float4(log(adaptedLum), 1, 1, 1);
} // psAdaptLuminance

// An easy optimization consists in make a specific technique without branching.
float4 psPostProcess(uniform int toneMappingFunction, in float2 uv : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(sceneSampler, uv).rgba;	// HDR Linear space	
	
	color.rgb = ExposureColor(color.rgb, uv);

	// This function takes an input colour in linear space, and converts it to a tone mapped gamma corrected color output		
	[branch]
	if (toneMappingFunction == 0)
    {	
		color.rgb = ToneMapFilmicALU(color.rgb);
    }
	else if (toneMappingFunction == 1)
    {
		color.rgb = ToneMapFilmicUncharted2(color.rgb);
    }
	else if (toneMappingFunction == 2)
    {
		color.rgb = ToneMapDuiker(color.rgb);
    }
	else if (toneMappingFunction == 3)
    {
		color.rgb = ToneMapReinhard(color.rgb);
    }
	else if (toneMappingFunction == 4)
    {
		color.rgb = ToneMapReinhardModified(color.rgb);
    }
	else if (toneMappingFunction == 5)
    {
		color.rgb = ToneMapExponential(color.rgb);
    }
	else if (toneMappingFunction == 6)
    {
		color.rgb = ToneMapLogarithmic(color.rgb);
    }
	else if (toneMappingFunction == 7)
    {
		color.rgb = ToneMapDragoLogarithmic(color.rgb);
    }
	
	// Film grain has to be calculated before bloom to avoid artifacts and after film tone mapping.
	[branch]
	if (filmGrainEnabled)
		color.rgb = FilmGrain(color.rgb, uv);	

	[branch]
	if (bloomEnabled)
		color.rgb = color.rgb + Bloom(color.rgb, uv);
	
	[branch]
	if (adjustLevelsEnabled)
		color.rgb = AdjustLevels(color.rgb);

	[branch]
	if (adjustLevelsIndividualChannelsEnabled)
		color.rgb = AdjustLevelsIndividualChannels(color.rgb);

	[branch]
	if (colorCorrectOneLutEnabled)
		color.rgb = lerp(color, TransformColor(color.rgb, firstlookupTableSampler), lerpOriginalColorAmount);

	[branch]
	if (colorCorrectTwoLutEnabled)
		color.rgb = lerp(color.rgb, lerp(TransformColor(color.rgb, firstlookupTableSampler), TransformColor(color.rgb, secondlookupTableSampler), lerpLookupTablesAmount), lerpOriginalColorAmount);
	
	return color;
} // psPostProcess

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique LuminanceMap
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 psLuminanceMap();
	}
} // LuminanceMap

technique LuminanceAdaptation
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 psAdaptLuminance();
	}
} // AdaptLuminance

technique PostProcessingFilmicALU
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 psPostProcess(0);
	}
} // PostProcessingFilmicALU

technique PostProcessingFilmicUncharted2
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 psPostProcess(1);
	}
} // PostProcessingFilmicUncharted2

technique PostProcessingDuiker
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 psPostProcess(2);
	}
} // PostProcessingDuiker

technique PostProcessingReinhard
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 psPostProcess(3);
	}
} // PostProcessingReinhard

technique PostProcessingReinhardModified
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 psPostProcess(4);
	}
} // PostProcessingReinhardModified

technique PostProcessingExponential
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 psPostProcess(5);
	}
} // PostProcessingExponential

technique PostProcessingLogarithmic
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 psPostProcess(6);
	}
} // PostProcessingLogarithmic

technique PostProcessingDragoLogarithmic
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 psPostProcess(7);
	}
} // PostProcessingDragoLogarithmic