/***********************************************************************************************************************************************
Copyright (c) 2008-2012, Laboratorio de Investigaci?n y Desarrollo en Visualizaci?n y Computaci?n Gr?fica - 
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

//////////////////////////////////////////////
///////// Vertex and Pixel Shaders //////////
/////////////////////////////////////////////

#include <..\Helpers\NormalsSpecularPower.fxh>
#include <..\Helpers\VertexAndFragmentDeclarations.fxh>
#include <..\Helpers\ParallaxMapping.fxh>
#include <..\Helpers\SkinningCommon.fxh>

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 worldViewProj : register(c0); // Projection uses all 4x4 matrix values.
float4x3 worldView     : register(c4); // We could use a float4x3 matrix because the last column is trivial.
float3x3 worldViewIT   : register(c7); // No translation information so float3x3 is enough.

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float  farPlane;

// Specular Power
float specularPower;    // Specular power value.
bool  specularTextured; // Indicates if a texture or the value will be used to store the object's specular power.

// Terrain //
float2 uvRectangleMin, uvRectangleSide;    // UV Rectangle.
bool   farTerrain;                         // Is this grid a far terrain grid? A far terrain grid uses the big color texture.
float  farTerrainBeginDistance, flatRange; // Related to flatting out far surfaces.

// Optimization: if the game is in its final steps of development and performance is still an issue then you can pack the shaders attributes.
// Unfortunately the packoffset keyword is only available in shader model 4/5, so this has to be done manually and that will hurt readability.

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture objectNormalTexture : register(t0);
sampler2D objectNormalSampler : register(s0) = sampler_state
{
	Texture = <objectNormalTexture>;
    /*ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = ANISOTROPIC; //LINEAR;
	MINFILTER = ANISOTROPIC; //LINEAR;
	MIPFILTER = LINEAR;*/
};

texture objectSpecularTexture : register(t1);
sampler2D objectSpecularSampler : register(s1) = sampler_state
{
	Texture = <objectSpecularTexture>;
    /*ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;*/
};

texture displacementTexture : register(t2);
sampler2D displacementSampler : register(s2) = sampler_state
{
	Texture = <displacementTexture>;
    /*ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;*/
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct PixelShader_OUTPUT
{
    float4 depth                     : COLOR0;
    float4 normal                    : COLOR1;
	float4 motionVectorSpecularPower : COLOR2;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

SimpleVS_OUTPUT SimpleVS(SimpleVS_INPUT input)
{
	SimpleVS_OUTPUT output;

	output.position     = mul(input.position, worldViewProj);
	output.uvDepth.z    = -mul(input.position, worldView).z / farPlane;
	// The normals are in view space.
	output.normal.xyz   = mul(input.normal, worldViewIT);
	output.uvDepth.xy = input.uv;

	return output;
} // SimpleVS

WithTangentVS_OUTPUT WithNormalMapVS(WithTangentVS_INPUT input)
{
	WithTangentVS_OUTPUT output;
   
	output.position  = mul(input.position, worldViewProj);
	output.uvDepth.z = -mul(input.position, worldView).z / farPlane;
   
	// Generate the tanget space to view space matrix
	output.tangentToView[0] = mul(input.tangent,  worldViewIT);
	output.tangentToView[1] = mul(cross(input.tangent, input.normal), worldViewIT);
	output.tangentToView[2] = mul(input.normal,   worldViewIT);

	output.uvDepth.xy = input.uv;

	return output;
} // WithNormalMapVS

WithParallaxVS_OUTPUT WithParallaxVS(WithTangentVS_INPUT input)
{
	WithParallaxVS_OUTPUT output;
   
	output.position   = mul(input.position, worldViewProj);
	float3 positionVS = mul(input.position, worldView);
	output.uvDepth.z  = -positionVS.z / farPlane;
   
	// Generate the tanget space to view space matrix
	output.tangentToView[0] = mul(input.tangent,  worldViewIT);
	output.tangentToView[1] = mul(input.binormal, worldViewIT); // binormal = cross(input.tangent, input.normal)
	output.tangentToView[2] = mul(input.normal,   worldViewIT);

	output.viewVS = normalize(-positionVS);

	output.uvDepth.xy = input.uv;

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
} // WithParallaxVS

/*WithTextureVS_OUTPUT TerrainVS(WithTangentVS_INPUT input)
{
	WithTextureVS_OUTPUT output = (WithTextureVS_OUTPUT)0;

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
} // TerrainVS*/

SimpleVS_OUTPUT SkinnedSimpleVS(SkinnedSimpleVS_INPUT input)
{
	SimpleVS_OUTPUT output;
   
	SkinTransform(input.position, input.normal, input.indices, input.weights, 4);
	output.position         = mul(input.position, worldViewProj);
	output.uvDepth.z    = -mul(input.position, worldView).z / farPlane;	
	output.normal.xyz  = mul(input.normal, worldViewIT); // The normals are in view space.
	output.uvDepth.xy = input.uv;

	return output;
} // SkinnedSimpleVS

WithTangentVS_OUTPUT SkinnedWithNormalMapVS(SkinnedWithTangentVS_INPUT input)
{
	WithTangentVS_OUTPUT output;
   
	SkinTransform(input.position, input.normal, input.indices, input.weights, 4);
	output.position = mul(input.position, worldViewProj);
	output.uvDepth.z    = -mul(input.position, worldView).z / farPlane;	

		// Generate the tanget space to view space matrix
	output.tangentToView[0] = mul(input.tangent,  worldViewIT);
	output.tangentToView[1] = mul(input.binormal, worldViewIT); // binormal = cross(input.tangent, input.normal)
	output.tangentToView[2] = mul(input.normal,   worldViewIT);

	output.uvDepth.xy = input.uv;

	return output;
} // SkinnedWithNormalMapVS

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

PixelShader_OUTPUT SimplePS(SimpleVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	// Depth
	output.depth = float4(input.uvDepth.z, 0, 1, 1);
 
	// Normals
 	input.normal.xyz = normalize(input.normal.xyz);
	output.normal  = float4(CompressNormal(input.normal.xyz), 1);

	// Specular Power
	if (specularTextured)
		output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(objectSpecularSampler, input.uvDepth.xy).a);
	else
		output.motionVectorSpecularPower.b = CompressSpecularPower(specularPower);	

	return output;
} // WithSpecularTexturePS

PixelShader_OUTPUT WithNormalMapPS(WithTangentVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	// Depth
	output.depth = float4(input.uvDepth.z, 0, 1, 1);
 
	// Normals
 	output.normal.xyz = 2.0 * tex2D(objectNormalSampler, input.uvDepth.xy).rgb - 1;
	output.normal.xyz =  normalize(mul(output.normal.xyz, input.tangentToView));
	output.normal  = float4(CompressNormal(output.normal.xyz), 1);

	// Specular Power
	if (specularTextured)
		output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(objectSpecularSampler, input.uvDepth.xy).a);
	else
		output.motionVectorSpecularPower.b = CompressSpecularPower(specularPower);

	return output;
} // WithNormalMap

PixelShader_OUTPUT WithParallaxPS(WithParallaxVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	// Depth
	output.depth = float4(input.uvDepth.z, 0, 1, 1);
 
	// Normals
 	output.normal.xyz = 2.0 * tex2D(objectNormalSampler, 
	                                CalculateParallaxUV(input.uvDepth.xy, input.parallaxOffsetTS, normalize(input.viewVS), input.tangentToView, objectNormalSampler)).rgb - 1;
	output.normal.xyz =  normalize(mul(output.normal.xyz, input.tangentToView));
	output.normal  = float4(CompressNormal(output.normal.xyz), 1);
	
	// Specular Power
	if (specularTextured)
		output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(objectSpecularSampler, input.uvDepth.xy).a);
	else
		output.motionVectorSpecularPower.b = CompressSpecularPower(specularPower);

	return output;
} // WithParallaxPS

/*PixelShader_OUTPUT TerrainPS(WithTextureVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	// Depth
	output.depth = float4(input.depth, 0, 1, 1);
 	
	// Normals
	float3 normalMap = 2.0 * tex2D(objectNormalSampler, input.uv).rgb - 1;
	input.normal =  normalize(mul(normalMap, worldViewIT));		
	output.normal  = float4(CompressNormal(input.normal.xyz), 1);

	// Specular Power
	output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(objectSpecularSampler, input.uv).a);	

	return output;
} // TerrainPS*/

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique GBufferSimple
{
    pass p0
    {
        VertexShader = compile vs_3_0 SimpleVS();
        PixelShader  = compile ps_3_0 SimplePS();
    }
} // GBufferSimple

technique GBufferWithNormalMap
{
    pass p0
    {
        VertexShader = compile vs_3_0 WithNormalMapVS();
        PixelShader  = compile ps_3_0 WithNormalMapPS();
    }
} // GBufferWithNormalMap

// The parallax will be calculated entirely on the scene pass. Therefore, is it necessary to calculate the parallax in the G-Buffer?
// It depends, you intermediate pass will use the information? Even if it uses, is it worth? 
// For example, SSAO can read normals and of course depth, but if the parallax shader calculates occlusion then maybe you can survive with it, 
// of course the results can not consider the other geometry but if the performance is critical (you also avoid a shader switch) maybe this could be not so important.
// Another possibility is to be platform dependent, Xbox 360 could ignore parallax on the G-Buffer and PC could use it.
technique GBufferWithParallax
{
    pass p0
    {
        VertexShader = compile vs_3_0 WithParallaxVS();
        PixelShader  = compile ps_3_0 WithParallaxPS();
    }
} // GBufferWithParallax

/*
// This shader tries to simulate a terrain system from a popular fly game.
// I deprecated because is very specific because of my available time (I need to use wisely) and because I need to use the game’s textures (incredible texture by the way).
// I wanted to left the source because is simple and the result are very good (with the original textures). 
// They use a better displacement technique in DX10+ GPUs and the results are even better.
technique GBufferTerrain
{
    pass p0
    {
        VertexShader = compile vs_3_0 TerrainVS();
        PixelShader  = compile ps_3_0 TerrainPS();
    }
} // GBufferTerrain*/

technique GBufferSkinnedSimple
{
    pass p0
    {
        VertexShader = compile vs_3_0 SkinnedSimpleVS();
        PixelShader  = compile ps_3_0 SimplePS();
    }
} // GBufferSkinnedSimple

technique GBufferSkinnedWithNormalMap
{
    pass p0
    {
        VertexShader = compile vs_3_0 SkinnedWithNormalMapVS();
        PixelShader  = compile ps_3_0 WithNormalMapPS();
    }
} // GBufferSkinnedWithNormalMap