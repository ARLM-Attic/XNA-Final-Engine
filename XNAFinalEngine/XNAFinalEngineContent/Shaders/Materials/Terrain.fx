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

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 world;
float4x4 worldView;
float4x4 worldViewProj;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel;

float specularIntensity = 0.01;

float3 cameraPosition;

float2 uvRectangleMin, uvRectangleSide;

// Flattening //

bool farTerrain; // Is this grid a far terrain grid? A far terrain grid uses the big color texture.

float farTerrainBeginDistance, flatRange; // Related to flatting out far surfaces.

// Fog //

float3 fogSunColor;
float3 fogFarColor;
float3 fogFrontColor;
float3 fogSunDirPrj;
float  fogSunInitialAngle;
float  fogSunRangeLength;
float  fogInitialDistance;
float  fogFarInitialDistance;
float  fogFarLenght;
// The height divides the terrain between two fog systems.
// From initial height to initial height + height length the top and bottom fogs will be lineal interpolated.
float  fogInitialHeight;
float  fogHeightLength;
float  fogTopStrength;
float  fogBottomStrength;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture lightMap;

sampler2D lightSampler = sampler_state
{
	Texture = <lightMap>;
	MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;
};

texture diffuseTexture;

sampler2D diffuseSampler = sampler_state
{
	Texture = <diffuseTexture>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MinFilter = ANISOTROPIC;
	MagFilter = ANISOTROPIC;
	MipFilter = LINEAR;
};

texture displacementTexture;

sampler2D displacementSampler = sampler_state
{
	Texture = <displacementTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT; // Is a floting point texture :(
	MINFILTER = POINT;
	MIPFILTER = NONE;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position : POSITION;
	float2 uv		: TEXCOORD0;
	float4 postProj : TEXCOORD1;
	float4 color    : TEXCOORD2;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

float fastsin(float x)
{
	float3 xx;
	float3 coefs = float3(1, -0.1666666, 0.0083333);
	float x2 = x*x;

	xx.x = x;
	xx.y = xx.x * x2;
	xx.z = xx.y * x2;

	return dot(xx, coefs);
} // Fastsin

float ComputeYFogValue(float EyeDist, float Height)
{
	float fDist1Lerp = saturate((EyeDist - fogInitialDistance) * fogTopStrength);
	float fDist2Lerp = saturate((EyeDist - fogInitialDistance) * fogBottomStrength);
	float fAltitLerp = saturate((Height  - fogInitialHeight)  * fogHeightLength);
	
	float res = lerp(fDist2Lerp, fDist1Lerp, fAltitLerp);
			
	res = (fastsin(3.14159 *(res - 0.5f)) + 1) * 0.5f;
	return res;
} // ComputeYFogValue

float3 ComputeHorizFogColor(float3 PrjLook)
{
	float fAlpha = dot(-fogSunDirPrj, PrjLook);
	float d = saturate((fAlpha - fogSunInitialAngle) * fogSunRangeLength);
	return lerp(GammaToLinear(fogSunColor.rgb), GammaToLinear(fogFarColor.rgb), d);
} // ComputeHorizFogColor

float3 ComputeFogColor(float EyeDist, float3 WorldPos, float3 CamPos)
{
	float3 Out;

	float3 EyeDir = WorldPos.xyz - CamPos.xyz;

	float3 PrjEyeDir = -normalize(EyeDir);
	Out.rgb = ComputeHorizFogColor(normalize(float3(PrjEyeDir.x, 0, PrjEyeDir.z)));

	float prjDist = length(float2(EyeDir.x, EyeDir.z));
	float fogLerp = saturate((prjDist - fogFarInitialDistance) * fogFarLenght);
	fogLerp = (fastsin(3.14159 *(fogLerp - 0.5f)) + 1) * 0.5f;

	Out.rgb = lerp(GammaToLinear(fogFrontColor.rgb), Out.rgb, fogLerp);
	return Out;
}

VS_OUT vs_main(in float4 position : POSITION,
			   in float3 normal   : NORMAL,
			   in float2 uv       : TEXCOORD0)
{
	VS_OUT output;
	
	// Change the uv space from the grid to the whole terrain.
	float2 displacementUV = uv;
	displacementUV.y = 1 - displacementUV.y;
	displacementUV   = float2(displacementUV * uvRectangleSide + uvRectangleMin);
	displacementUV.y = 1 - displacementUV.y;
		
	position.y = tex2Dlod(displacementSampler, float4(displacementUV, 0, 0)) / 100;	
	float eyeDist = length(mul(position, worldView));
	[branch]
	if (farTerrain)
	{		
		float flatLerp = saturate((eyeDist - farTerrainBeginDistance) / flatRange);
		position.y = lerp(position.y, 0, flatLerp);
		output.uv = displacementUV;		
	}
	else
		output.uv = uv;

	output.position = mul(position, worldViewProj);
	output.postProj = output.position;
	// Fog //
	output.color.w   = ComputeYFogValue(eyeDist, position.y);
	output.color.rgb = ComputeFogColor(eyeDist, mul(position, world), cameraPosition);

    return output;
} // vs_main

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float2 PostProjectToScreen(float4 pos)
{
	float2 screenPosition = pos.xy / pos.w;
	// Screen position to uv coordinates.
	return (0.5f * (float2(screenPosition.x, -screenPosition.y) + 1));
} // PostProjectToScreen

float4 ps_main(VS_OUT input) : COLOR
{
	/*// Find the screen space texture coordinate & offset
	float2 lightMapUv = PostProjectToScreen(input.postProj) + halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	
	// Diffuse contribution + specular exponent.
	float4 light = tex2D(lightSampler, lightMapUv);*/
		
	// Final color (in linear space)		
	//float3 color = GammaToLinear(tex2D(diffuseSampler, input.uv).rgb * light.rgb + specularIntensity * light.a);
	
	float3 color = GammaToLinear(tex2D(diffuseSampler, input.uv).rgb);	
	color = lerp(color, input.color, input.color.w); // Apply fog
	return float4(color, 1);	
} // ps_main

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique Terrain
{
    pass P0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader  = compile ps_3_0 ps_main();
    }
} // Terrain