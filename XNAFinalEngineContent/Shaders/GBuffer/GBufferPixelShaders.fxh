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
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

// Specular Power
float specularPower;    // Specular power value.
bool  specularTextured; // Indicates if a texture or the value will be used to store the object's specular power.

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
////////////////// Helpers ///////////////////
//////////////////////////////////////////////

/// Compress to the (0,1) range with high precision for low values. Guerilla method.
float CompressSpecularPower(float specularPower)
{
	return log2(specularPower) / 10.5;
} // CompressSpecularPower

// Compress the normal using spherical coordinates. This gives us more precision with an acceptable space.
float4 CompressNormal(float3 inputNormal)
{
	float f = inputNormal.z * 2 + 1;
	float g = dot(inputNormal, inputNormal);
	float p = sqrt(g + f);
	return float4(inputNormal.xy / p * 0.5 + 0.5, 1, 1);
	// return float4(normalize(inputNormal.xy) * sqrt(inputNormal.z * 0.5 + 0.5), 1, 1); // Spheremap Transform: Crytek method. 
	// return 0.5f * (float4(inputNormal.xyz, 1) + 1.0f); // Change to the [0, 1] range to avoid negative values.
} // CompressNormal

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

PixelShader_OUTPUT WithoutTexturePS(WithoutTextureVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	// Depth
	output.depth = float4(input.depth, 0, 1, 1);

	// Normals
	input.normal = normalize(input.normal);	
	output.normal  = CompressNormal(input.normal);	

	// Specular Power
	output.motionVectorSpecularPower.b = CompressSpecularPower(specularPower);

	return output;
} // WithoutTexturePS

PixelShader_OUTPUT WithSpecularTexturePS(WithTextureVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	// Depth
	output.depth = float4(input.depth, 0, 1, 1);
 
	// Normals
 	input.normal = normalize(input.normal);			
	output.normal  = CompressNormal(input.normal);

	// Specular Power
	output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(objectSpecularSampler, input.uv).a);	

	return output;
} // WithSpecularTexturePS

PixelShader_OUTPUT WithNormalMapPS(WithTangentVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	// Depth
	output.depth = float4(input.depth, 0, 1, 1);
 
	// Normals
 	output.normal.xyz = 2.0 * tex2D(objectNormalSampler, input.uv).rgb - 1;
	output.normal.xyz =  normalize(mul(output.normal.xyz, input.tangentToView));
	output.normal  = CompressNormal(output.normal.xyz);

	// Specular Power
	if (specularTextured)
		output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(objectSpecularSampler, input.uv).a);
	else
		output.motionVectorSpecularPower.b = CompressSpecularPower(specularPower);

	return output;
} // WithNormalMap

PixelShader_OUTPUT WithParallaxPS(WithParallaxVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	// Depth
	output.depth = float4(input.depth, 0, 1, 1);
 
	// Normals
 	output.normal.xyz = 2.0 * tex2D(objectNormalSampler, CalculateParallaxUV(input.uv, input.parallaxOffsetTS, normalize(input.viewVS), input.tangentToView, objectNormalSampler)).rgb - 1;
	output.normal.xyz =  normalize(mul(output.normal.xyz, input.tangentToView));
	output.normal  = CompressNormal(output.normal.xyz);
	
	// Specular Power
	if (specularTextured)
		output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(objectSpecularSampler, input.uv).a);
	else
		output.motionVectorSpecularPower.b = CompressSpecularPower(specularPower);

	return output;
} // WithParallaxPS

PixelShader_OUTPUT TerrainPS(WithTextureVS_OUTPUT input)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	// Depth
	output.depth = float4(input.depth, 0, 1, 1);
 	
	// Normals
	float3 normalMap = 2.0 * tex2D(objectNormalSampler, input.uv).rgb - 1;
	input.normal =  normalize(mul(normalMap, worldViewIT));		
	output.normal  = CompressNormal(input.normal);

	// Specular Power
	output.motionVectorSpecularPower.b = CompressSpecularPower(tex2D(objectSpecularSampler, input.uv).a);	

	return output;
} // TerrainPS