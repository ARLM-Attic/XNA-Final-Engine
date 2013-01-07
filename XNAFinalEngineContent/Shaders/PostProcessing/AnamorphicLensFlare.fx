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

#include <..\Helpers\Discard.fxh>
#include <..\GBuffer\GBufferReader.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float4x4 worldViewProj : WorldViewProjection;
float4x4 worldView     : WorldView;

float  farPlane;
float3 sunColor;
float2 halfPixel;
float  dispersal,
       haloWidth,
	   intensity;
float2 sunPosProj;
float3 distortion;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture lowBlurredSunTexture: register(t6);
sampler lowBlurredSunTextureSampler: register(s6) = sampler_state 
{
    texture = <lowBlurredSunTexture>;
	/*AddressU  = CLAMP;
    AddressV  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;*/
};
texture highBlurredSunTexture: register(t7);
sampler highBlurredSunTextureSampler: register(s7) = sampler_state 
{
    texture = <highBlurredSunTexture>;
	/*AddressU  = CLAMP;
    AddressV  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;*/
};
texture dirtTexture : register(t8);
sampler dirtTextureSampler : register(s8) = sampler_state 
{
    texture = <dirtTexture>;
	/*AddressU  = CLAMP;
    AddressV  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;*/
};
texture sceneTexture : register(t9);
sampler sceneTextureSamplerLinear : register(s9) = sampler_state 
{
    texture = <sceneTexture>;
	/*AddressU  = CLAMP;
    AddressV  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;*/
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position		: POSITION;
	float2 uv			: TEXCOORD0;
};

struct VS_OUTSun
{
	float4 position		  : POSITION;
	float  depth		  : TEXCOORD0;
	float4 screenPosition : TEXCOORD1;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

// Vertex shader for a mesh object.
VS_OUTSun vsSun(in float4 position : POSITION)
{	
    VS_OUTSun output = (VS_OUTSun)0;
    output.position = mul(position, worldViewProj);	
	output.screenPosition = output.position;
	output.depth    = -mul(position, worldView).z / farPlane; // Linear depth
    return output;
} // vsSun

// Vertex shader for a screen quad.
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

float4 psSun(VS_OUTSun input) : COLOR
{
    // Obtain screen position
    input.screenPosition.xy /= input.screenPosition.w;
    // Obtain textureCoordinates corresponding to the current pixel
	// The screen coordinates are in [-1,1]*[1,-1]
    // The texture coordinates need to be in [0,1]*[0,1]
	// http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	// http://diaryofagraphicsprogrammer.blogspot.com.ar/2008/09/calculating-screen-space-texture.html
    float2 uv = 0.5f * (float2(input.screenPosition.x, -input.screenPosition.y) + 1) + halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/

	float depth = tex2Dlod(depthSampler, float4(uv, 0, 0)).r;

	// If the sun is occluded
	if (depth < input.depth)
	{
		Discard();
		return 0;
	}

    return float4(sunColor, 1);
} // psSun

float4 PS_BlurV(VS_OUT input, uniform int blurWidth) : COLOR
{	
	float3 color = 0;
	int widthPlus1 = blurWidth + 1;
	float sum = 0.0;

	//[loop] 
	for(int y = -blurWidth; y <= blurWidth; y++)
	{
		float width = (widthPlus1 - abs(float(y)));
		color += tex2Dlod(sceneTextureSamplerLinear, float4(input.uv + float2(0, halfPixel.y * y), 0, 0)).rgb * width;
		sum += width;
	}
	
	return float4(color / sum, 1.0);
} // PS_BlurV

float4 PS_BlurH(VS_OUT input, uniform int blurWidth) : COLOR
{	
	float3 color = 0;
	int widthPlus1 = blurWidth + 1;
	float sum = 0.0;
	
	//[loop]
	for(int x = -blurWidth; x <= blurWidth; x++)
	{
		float width = (widthPlus1 - abs(float(x)));
		color += tex2Dlod(sceneTextureSamplerLinear, float4(input.uv + float2(-halfPixel.x * x, 0), 0, 0)).rgb * width;
		sum += width;
	}
	
	return float4(color / sum, 1.0);
} // PS_BlurH

float3 Tex2DDistorted(sampler textureSampler, float2 uv, float2 offset)
{
	return float3(tex2D(textureSampler, uv + offset * distortion.r).r,
		          tex2D(textureSampler, uv + offset * distortion.g).g,
		          tex2D(textureSampler, uv + offset * distortion.b).b);
} // Tex2DDistorted

float4 LensFlareCompositionPS(VS_OUT input) : COLOR
{
	float3 radialBlur = 0;	
	float2 texCoord = input.uv;
	int radialBlurSamples = 128;
	float2 radialBlurVector = (sunPosProj - texCoord) / (radialBlurSamples * 1.02f); // TODO: it could be a parameter.

	for(int i = 0; i < radialBlurSamples; i++)
	{
		radialBlur += tex2D(highBlurredSunTextureSampler, texCoord).rgb; // lowBlurredSunTextureSampler
		texCoord += radialBlurVector;
	}

	radialBlur /= radialBlurSamples;	

	float3 lensFlareHalo = 0;
	texCoord = 1.0 - input.uv;
	float2 lensFlareVector = (0.5 - texCoord) * dispersal;
	float2 lensFlareOffset = 0;

	for(int j = 0; j < 4; j++)
	{
		lensFlareHalo += Tex2DDistorted(highBlurredSunTextureSampler, texCoord, lensFlareOffset).rgb;
		lensFlareOffset += lensFlareVector;
	}

	lensFlareHalo += Tex2DDistorted(highBlurredSunTextureSampler, texCoord, normalize(lensFlareVector) * haloWidth);

	lensFlareHalo /= 5.0;
		
	float3 dirtColor = tex2D(dirtTextureSampler, input.uv).rgb;	
	return float4((tex2D(highBlurredSunTextureSampler, input.uv).rgb + (radialBlur + lensFlareHalo) * dirtColor) * intensity, 1.0);
} // LensFlareCompositionPS

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique LensFlare
{
    pass Sun
	{
		VertexShader = compile vs_3_0 vsSun();		
		PixelShader  = compile ps_3_0 psSun();
	}
	pass BlurVerticalLowBlurred
	{
		VertexShader = compile vs_3_0 vs_main();		
		PixelShader  = compile ps_3_0 PS_BlurV(1);
	}
	pass BlurHorizontalLowBlurred
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 PS_BlurH(1);
	}
	pass BlurVerticalHighBlurred
	{
		VertexShader = compile vs_3_0 vs_main();		
		PixelShader  = compile ps_3_0 PS_BlurV(5);
	}
	pass BlurHorizontalHighBlurred
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 PS_BlurH(5);
	}
	pass LensFlareComposition
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 LensFlareCompositionPS();
	}
} // LensFlare