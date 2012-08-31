#include <..\Helpers\SkinningCommon.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float4x4 worldViewProj;
float4x4 world;
float  lightRadius;
float3 lightPosition;

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VertexInput
{	
	float4 position : POSITION;
};

struct VertexInputSkinned
{	
	float4 position : POSITION;
    int4   indices  : BLENDINDICES0;
    float4 weights  : BLENDWEIGHT0;
};

struct GenerateShadowMapVS_OUTPUT 
{
	float4 position : POSITION;
	float  depth    : TEXCOORD0;
};

struct GenerateCubeShadowMapVS_OUTPUT 
{
	float4 position      : POSITION;
	float3 positionWorld : TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

GenerateShadowMapVS_OUTPUT  VS_GenerateShadowMap(VertexInput input)
{
	GenerateShadowMapVS_OUTPUT  output = (GenerateShadowMapVS_OUTPUT ) 0;
	output.position = mul(input.position, worldViewProj);
	
	// Linear depth calculation instead of normal depth calculation.
	output.depth = (output.position.z / output.position.w);

	return output;
} // VS_GenerateShadowMap

GenerateShadowMapVS_OUTPUT  VS_SkinnedGenerateShadowMap(VertexInputSkinned input)
{
	GenerateShadowMapVS_OUTPUT  output = (GenerateShadowMapVS_OUTPUT ) 0;

	SkinTransform(input.position, input.indices, input.weights, 4);
	output.position = mul(input.position, worldViewProj);
	
	// Linear depth calculation instead of normal depth calculation.
	output.depth = (output.position.z / output.position.w);

	return output;
} // VS_GenerateShadowMap

// Cube shadows maps compare the distance of the geometry to the light sources.
GenerateCubeShadowMapVS_OUTPUT  VS_GenerateCubeShadowMap(VertexInput input)
{
	GenerateCubeShadowMapVS_OUTPUT  output = (GenerateCubeShadowMapVS_OUTPUT ) 0;
	output.position = mul(input.position, worldViewProj);
			
	output.positionWorld = mul(input.position, world);

	return output;
} // VS_GenerateCubeShadowMap

// Cube shadows maps compare the distance of the geometry to the light sources.
GenerateCubeShadowMapVS_OUTPUT  VS_SkinnedGenerateCubeShadowMap(VertexInputSkinned input)
{
	GenerateCubeShadowMapVS_OUTPUT  output = (GenerateCubeShadowMapVS_OUTPUT ) 0;

	SkinTransform(input.position, input.indices, input.weights, 4);
	output.position = mul(input.position, worldViewProj);
	
	// Linear depth calculation instead of normal depth calculation.
	output.positionWorld = mul(input.position, world);

	return output;
} // VS_SkinnedGenerateCubeShadowMap

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 PS_GenerateShadowMap(GenerateShadowMapVS_OUTPUT input) : COLOR
{		
	return float4(input.depth, 1, 1, 1);
} // PS_GenerateShadowMap

float4 PS_GenerateCubeShadowMap(GenerateCubeShadowMapVS_OUTPUT input) : COLOR
{
	return float4(length(input.positionWorld.xyz - lightPosition) / lightRadius, 1, 1, 1); 
	// Probably dot(input.positionWorld.xyz - lightPosition, input.positionWorld.xyz - lightPosition) / (lightRadius * lightRadius) could be faster but we need to change also the other part of the shadow algorithm.
} // PS_GenerateCubeShadowMap

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique GenerateLightDepthBuffer
{
	pass P0
	{
		VertexShader = compile vs_3_0 VS_GenerateShadowMap();
		PixelShader  = compile ps_3_0 PS_GenerateShadowMap();
	}
} // GenerateLightDepthBuffer

technique GenerateLightDepthBufferSkinned
{
	pass P0
	{
		VertexShader = compile vs_3_0 VS_SkinnedGenerateShadowMap();
		PixelShader  = compile ps_3_0 PS_GenerateShadowMap();
	}
} // GenerateLightDepthBufferSkinned

technique GenerateCubeLightDepthBuffer
{
	pass P0
	{
		VertexShader = compile vs_3_0 VS_GenerateCubeShadowMap();
		PixelShader  = compile ps_3_0 PS_GenerateCubeShadowMap();
	}
} // GenerateCubeLightDepthBuffer

technique GenerateCubeLightDepthBufferSkinned
{
	pass P0
	{
		VertexShader = compile vs_3_0 VS_SkinnedGenerateCubeShadowMap();
		PixelShader  = compile ps_3_0 PS_GenerateCubeShadowMap();
	}
} // GenerateCubeLightDepthBufferSkinned