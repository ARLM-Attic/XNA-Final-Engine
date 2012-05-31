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

#include <..\Helpers\SkinningCommon.fxh>

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct SkinnedWithoutTextureVS_INPUT 
{
   float4 position : POSITION;
   float3 normal   : NORMAL;   
   int4   indices  : BLENDINDICES0;
   float4 weights  : BLENDWEIGHT0;
};

struct SkinnedWithTextureVS_INPUT
{
   float4 position : POSITION;
   float3 normal   : NORMAL;
   float2 uv       : TEXCOORD0;
   int4   indices  : BLENDINDICES0;
   float4 weights  : BLENDWEIGHT0;
};

struct SkinnedWithTangentVS_INPUT
{
   float4 position : POSITION;
   float3 normal   : NORMAL;
   float3 tangent  : TANGENT;
   float3 binormal : BINORMAL;
   float2 uv       : TEXCOORD0;
   int4   indices  : BLENDINDICES0;
   float4 weights  : BLENDWEIGHT0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

WithoutTextureVS_OUTPUT SkinnedWithoutTextureVS(SkinnedWithoutTextureVS_INPUT input)
{
	WithoutTextureVS_OUTPUT output;
   
	SkinTransform(input.position, input.normal, input.indices, input.weights, 4);
	output.position         = mul(input.position, worldViewProj);
	output.normalDepth.w    = -mul(input.position, worldView).z / farPlane;	
	output.normalDepth.xyz  = mul(input.normal, worldViewIT); // The normals are in view space.	

	return output;
} // SkinnedWithoutTextureVS

WithTextureVS_OUTPUT SkinnedWithTextureVS(SkinnedWithTextureVS_INPUT input)
{
	WithTextureVS_OUTPUT output;
   
	SkinTransform(input.position, input.normal, input.indices, input.weights, 4);
	output.position         = mul(input.position, worldViewProj);
	output.normalDepth.w    = -mul(input.position, worldView).z / farPlane;	
	output.normalDepth.xyz  = mul(input.normal, worldViewIT); // The normals are in view space.
	output.uv = input.uv;

	return output;
} // SkinnedWithTextureVS

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

WithParallaxVS_OUTPUT SkinnedWithParallaxVS(SkinnedWithTangentVS_INPUT input)
{
	WithParallaxVS_OUTPUT output;
   
	SkinTransform(input.position, input.normal, input.indices, input.weights, 4);
	output.position = mul(input.position, worldViewProj);
	float3 positionVS = mul(input.position, worldView);
	output.uvDepth.z    = -mul(input.position, worldView).z / farPlane;	

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
} // SkinnedWithParallaxVS