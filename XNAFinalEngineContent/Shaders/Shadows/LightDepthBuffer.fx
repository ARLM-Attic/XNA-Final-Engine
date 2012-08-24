#include <..\Helpers\SkinningCommon.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float4x4 worldViewProj;

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

//////////////////////////////////////////////
/////////// Generate Shadow Map //////////////
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

float4 PS_GenerateShadowMap(GenerateShadowMapVS_OUTPUT input) : COLOR
{		
	return float4(input.depth, 1, 1, 1);
} // PS_GenerateShadowMap

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