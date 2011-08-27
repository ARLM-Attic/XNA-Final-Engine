/***********************************************************************************************************************************************
Copyright (c) 2008-2011, Laboratorio de Investigaci?n y Desarrollo en Visualizaci?n y Computaci?n Gr?fica - 
                         Departamento de Ciencias e Ingenier?a de la Computaci?n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

?	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

?	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

?	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
************************************************************************************************************************************************/

#include <..\Helpers\ParallaxMapping.fxh>

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 worldView;
float3x3 worldViewIT;
float4x4 worldViewProj;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel;

float farPlane = 100.0f;

float specularPower;

bool specularTextured;

// Terrain //

float2 uvRectangleMin, uvRectangleSide; // UV Rectangle.

bool farTerrain; // Is this grid a far terrain grid? A far terrain grid uses the big color texture.

float farTerrainBeginDistance, flatRange; // Related to flatting out far surfaces.

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture objectNormalTexture : RENDERCOLORTARGET;

sampler2D objectNormalSampler = sampler_state
{
	Texture = <objectNormalTexture>;
    ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = ANISOTROPIC; //LINEAR;
	MINFILTER = ANISOTROPIC; //LINEAR;
	MIPFILTER = LINEAR;
};

texture specularTexture : RENDERCOLORTARGET;

sampler2D specularSampler = sampler_state
{
	Texture = <specularTexture>;
    ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

texture displacementTexture;

sampler2D displacementSampler = sampler_state
{
	Texture = <displacementTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
};

texture normalTexture : RENDERCOLORTARGET;

sampler2D normalSampler = sampler_state
{
	Texture = <normalTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
};

texture depthTexture : RENDERCOLORTARGET;

sampler2D depthSampler = sampler_state
{
	Texture = <depthTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
};

texture motionVectorSpecularPowerTexture : RENDERCOLORTARGET;

sampler2D motionVectorSpecularPowerSampler = sampler_state
{
	Texture = <motionVectorSpecularPowerTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct GBufferWithoutTextureVS_INPUT 
{
   float4 position : POSITION;
   float3 normal   : NORMAL;   
};

struct GBufferWithTextureVS_INPUT
{
   float4 position : POSITION;
   float3 normal   : NORMAL;
   float2 uv       : TEXCOORD0;
};

struct GBufferWithTangentVS_INPUT
{
   float4 position : POSITION;
   float3 normal   : NORMAL;
   float3 tangent  : TANGENT;
   float3 binormal : BINORMAL;
   float2 uv       : TEXCOORD0;
};

struct GBufferWithoutTextureVS_OUTPUT 
{
   float4 position : POSITION0;
   float3 normal   : TEXCOORD0;
   float  depth    : TEXCOORD1;   
};

struct GBufferWithTextureVS_OUTPUT 
{
   float4 position         : POSITION0;
   float3 normal           : TEXCOORD0;
   float  depth            : TEXCOORD1;   
   float2 uv			   : TEXCOORD2;
};

struct GBufferWithTangentVS_OUTPUT 
{
   float4 position         : POSITION0;
   float  depth            : TEXCOORD0;
   float2 uv			   : TEXCOORD1;
   float3x3 tangentToView  : TEXCOORD2;
};

struct GBufferWithParallaxVS_OUTPUT 
{
   float4 position         : POSITION0;
   float  depth            : TEXCOORD0;
   float2 uv			   : TEXCOORD1;
   float2 parallaxOffsetTS : TEXCOORD2;
   float3 viewVS           : TEXCOORD3;
   float3x3 tangentToView  : TEXCOORD4;
};

struct PixelShader_OUTPUT
{
    float4 depth  : COLOR0;
    float4 normal : COLOR1;
	float4 motionVectorSpecularPower : COLOR2;
};

//////////////////////////////////////////////
/////////////// Sample Normal ////////////////
//////////////////////////////////////////////

float3 SampleNormal(float2 uv, sampler2D textureSampler)
{
	float2 normalInformation = tex2D(textureSampler, uv).xy;
	float3 N;
	
	// Spheremap Transform (not working)
	/*N.z = dot(normalInformation, normalInformation) * 2 - 1; // lenght2 = dot(v, v)
	N.xy = normalize(normalInformation) * sqrt(1 - N.z * N.z);	*/	

	// Spherical Coordinates
	N.xy = -normalInformation * normalInformation + normalInformation;
	N.z = -1;
	float f = dot(N, float3(1, 1, 0.25));
	float m = sqrt(f);
	N.xy = (normalInformation * 8 - 4) * m;
	N.z = -(1 - 8 * f);
	
	// Basic form
	//float3 N = tex2D(normalSampler2, uv).xyz * 2 - 1;
	
	return N; // Already normalized
} // SampleNormal

float3 SampleNormal(float2 uv)
{
	return SampleNormal(uv, normalSampler);
} // SampleNormal

//////////////////////////////////////////////
////////////// Specular Power ////////////////
//////////////////////////////////////////////

/// Compress to the (0,1) range with high precision for low values. Guerilla method.
float CompressSpecularPower(float specularPower)
{
	return log2(specularPower) / 10.5;
}

float DecompressSpecularPower(float compressedSpecularPower)
{
	return pow(2, compressedSpecularPower * 10.5);
}

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

GBufferWithoutTextureVS_OUTPUT GBufferWithoutTextureVS(GBufferWithoutTextureVS_INPUT input)
{
	GBufferWithoutTextureVS_OUTPUT output;
   
	output.position = mul(input.position, worldViewProj);
	output.depth    = -mul(input.position, worldView).z / farPlane;
	// The normals are in view space.
	output.normal   = mul(input.normal, worldViewIT);

	return output;
} // GBufferWithoutTextureVS

GBufferWithTextureVS_OUTPUT GBufferWithSpecularTextureVS(GBufferWithTextureVS_INPUT input)
{
	GBufferWithTextureVS_OUTPUT output;

	output.position = mul(input.position, worldViewProj);
	output.depth    = -mul(input.position, worldView).z / farPlane;	
	// The normals are in view space.
	output.normal   = mul(input.normal, worldViewIT);
	output.uv = input.uv;

	return output;
} // GBufferWithTextureVS

GBufferWithTangentVS_OUTPUT GBufferWithNormalMapVS(GBufferWithTangentVS_INPUT input)
{
	GBufferWithTangentVS_OUTPUT output;
   
	output.position = mul(input.position, worldViewProj);
	output.depth    = -mul(input.position, worldView).z / farPlane;
   
	// Generate the tanget space to view space matrix
	output.tangentToView[0] = mul(input.tangent,  worldViewIT);
	output.tangentToView[1] = mul(input.binormal, worldViewIT); // binormal = cross(input.tangent, input.normal)
	output.tangentToView[2] = mul(input.normal,   worldViewIT);

	output.uv = input.uv;

	return output;
} // GBufferWithTangentVS

GBufferWithParallaxVS_OUTPUT GBufferWithParallaxVS(GBufferWithTangentVS_INPUT input)
{
	GBufferWithParallaxVS_OUTPUT output;
   
	output.position = mul(input.position, worldViewProj);
	float3 positionVS = mul(input.position, worldView);
	output.depth    = -positionVS.z / farPlane;
   
	// Generate the tanget space to view space matrix
	output.tangentToView[0] = mul(input.tangent,  worldViewIT);
	output.tangentToView[1] = mul(input.binormal, worldViewIT); // binormal = cross(input.tangent, input.normal)
	output.tangentToView[2] = mul(input.normal,   worldViewIT);

	output.viewVS = normalize(-positionVS);

	output.uv = input.uv;

	// Compute the ray direction for intersecting the height field profile with current view ray.

	float3 viewTS = mul(output.tangentToView, output.viewVS);
         
	// Compute initial parallax displacement direction:
	float2 parallaxDirection = normalize(viewTS.xy);
       
	// The length of this vector determines the furthest amount of displacement:
	float fLength        = length( viewTS );
	float parallaxLength = sqrt( fLength * fLength -viewTS.z * viewTS.z ) / viewTS.z; 
       
	// Compute the actual reverse parallax displacement vector:
	// Need to scale the amount of displacement to account for different height ranges in height maps.
	// This is controlled by an artist-editable parameter heightMapScale.
	output.parallaxOffsetTS = parallaxDirection * parallaxLength * heightMapScale;

	return output;
} // GBufferWithParallaxVS

GBufferWithTextureVS_OUTPUT GBufferTerrainVS(GBufferWithTangentVS_INPUT input)
{
	GBufferWithTextureVS_OUTPUT output = (GBufferWithTextureVS_OUTPUT)0;

	// Change the uv space from the grid to the whole terrain.
	float2 displacementUV = input.uv;
	displacementUV.y = 1 - displacementUV.y;
	displacementUV   = float2(displacementUV * uvRectangleSide + uvRectangleMin);
	displacementUV.y = 1 - displacementUV.y;
		
	input.position.y = tex2Dlod(displacementSampler, float4(displacementUV, 0, 0)) / 100;
	//position.y = LinearToGamma(tex2Dlod(displacementSampler, float4(displacementUV, 0, 0))) / 2;
	float3 distance = mul(input.position, worldView);	
	[branch]
	if (farTerrain)
	{		
		float flatLerp = saturate((distance - farTerrainBeginDistance) / flatRange);
		input.position.y = lerp(input.position.y, 0, flatLerp);
	}
	output.uv = displacementUV;

	output.position = mul(input.position, worldViewProj);	
	output.depth    = -distance.z / farPlane;
    
	return output;
} // GBufferTerrainVS

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

PixelShader_OUTPUT GBufferWithoutTexturePS(GBufferWithoutTextureVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	output.depth = float4(input.depth, 0, 1, 1);

	input.normal = normalize(input.normal);
		
	// Spherical Coordinates
	float f = input.normal.z * 2 + 1;
	float g = dot(input.normal, input.normal);
	float p = sqrt(g + f);
	output.normal  = float4(input.normal.xy / p * 0.5 + 0.5, 1, 1);
	//output.normal = float4(normalize(input.normal.xy) * sqrt(input.normal.z * 0.5 + 0.5), 1, 1); // Spheremap Transform: Crytek method. 
	//output.normal = 0.5f * (float4(input.normal.xyz, 1) + 1.0f); // Change to the [0, 1] range to avoid negative values.

	output.motionVectorSpecularPower.b = CompressSpecularPower(specularPower);

	return output;
} // GBufferWithoutTexturePS

PixelShader_OUTPUT GBufferWithSpecularTexturePS(GBufferWithTextureVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	output.depth = float4(input.depth, 0, 1, 1);
 
 	input.normal = normalize(input.normal);
		
	// Spherical Coordinates
	float f = input.normal.z * 2 + 1;
	float g = dot(input.normal, input.normal);
	float p = sqrt(g + f);
	output.normal  = float4(input.normal.xy / p * 0.5 + 0.5, 1, 1);
	//output.normal = float4(normalize(input.normal.xy) * sqrt(input.normal.z * 0.5 + 0.5), 1, 1); // Spheremap Transform: Crytek method. 
	//output.normal = 0.5f * (float4(input.normal.xyz, 1) + 1.0f); // Change to the [0, 1] range to avoid negative values.

	output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(specularSampler, input.uv).a);	

	return output;
} // GBufferWithSpecularTexturePS

PixelShader_OUTPUT GBufferWithNormalMapPS(GBufferWithTangentVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	output.depth = float4(input.depth, 0, 1, 1);
 
 	output.normal.xyz = 2.0 * tex2D(objectNormalSampler, input.uv).rgb - 1;
	output.normal.xyz =  normalize(mul(output.normal.xyz, input.tangentToView));

	// Spherical Coordinates
	float f = output.normal.z * 2 + 1;
	float g = dot(output.normal.xyz, output.normal.xyz);
	float p = sqrt(g + f);
	output.normal  = float4(output.normal.xy / p * 0.5 + 0.5, 1, 1);

	if (specularTextured)
		output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(specularSampler, input.uv).a);
	else
		output.motionVectorSpecularPower.b = CompressSpecularPower(specularPower);

	return output;
} // GBufferWithNormalMap

PixelShader_OUTPUT GBufferWithParallaxPS(GBufferWithParallaxVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	output.depth = float4(input.depth, 0, 1, 1);
 
 	output.normal.xyz = 2.0 * tex2D(objectNormalSampler, CalculateParallaxUV(input.uv, input.parallaxOffsetTS, normalize(input.viewVS), input.tangentToView, objectNormalSampler)).rgb - 1;
	output.normal.xyz =  normalize(mul(output.normal.xyz, input.tangentToView));

	// Spherical Coordinates
	float f = output.normal.z * 2 + 1;
	float g = dot(output.normal.xyz, output.normal.xyz);
	float p = sqrt(g + f);
	output.normal  = float4(output.normal.xy / p * 0.5 + 0.5, 1, 1);
	
	if (specularTextured)
		output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(specularSampler, input.uv).a);
	else
		output.motionVectorSpecularPower.b = CompressSpecularPower(specularPower);

	return output;
} // GBufferWithParallaxPS

PixelShader_OUTPUT GBufferTerrainPS(GBufferWithTextureVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	output.depth = float4(input.depth, 0, 1, 1);
 	
	float3 normalMap = 2.0 * tex2D(objectNormalSampler, input.uv).rgb - 1;
	input.normal =  normalize(mul(normalMap, worldViewIT));
		
	// Spherical Coordinates
	float f = input.normal.z * 2 + 1;
	float g = dot(input.normal, input.normal);
	float p = sqrt(g + f);
	output.normal  = float4(input.normal.xy / p * 0.5 + 0.5, 1, 1);

	output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(specularSampler, input.uv).a);	

	return output;
} // GBufferWithSpecularTexturePS

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique GBufferWithoutTexture
{
    pass p0
    {
        VertexShader = compile vs_3_0 GBufferWithoutTextureVS();
        PixelShader  = compile ps_3_0 GBufferWithoutTexturePS();
    }
} // GBufferWithoutSpecularTexture

technique GBufferWithSpecularTexture
{
    pass p0
    {
        VertexShader = compile vs_3_0 GBufferWithSpecularTextureVS();
        PixelShader  = compile ps_3_0 GBufferWithSpecularTexturePS();
    }
} // GBufferWithSpecularTexture

technique GBufferWithNormalMap
{
    pass p0
    {
        VertexShader = compile vs_3_0 GBufferWithNormalMapVS();
        PixelShader  = compile ps_3_0 GBufferWithNormalMapPS();
    }
} // GBufferWithNormalMap

technique GBufferWithParallax
{
    pass p0
    {
        VertexShader = compile vs_3_0 GBufferWithParallaxVS();
        PixelShader  = compile ps_3_0 GBufferWithParallaxPS();
    }
} // GBufferWithParallax

technique GBufferTerrain
{
    pass p0
    {
        VertexShader = compile vs_3_0 GBufferTerrainVS();
        PixelShader  = compile ps_3_0 GBufferTerrainPS();
    }
} // GBufferTerrain