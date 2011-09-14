/******************************************************************************

    From MJP example (mpettineo@gmail.com)	
	Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

******************************************************************************/

#include <..\GBuffer\GBufferReader.fx>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float4x4 worldViewProj;

float2 halfPixel;

float3 frustumCorners[4];

float2 shadowMapSize;

// Texel size
float2 invShadowMapSize;

// Depth bias, controls how much we remove from the depth to fix depth checking artifacts.
float depthBias;

// Poison filter pseudo random filter positions for PCF with 10 samples
float2 FilterTaps[10] =
{
	// First test, still the best.
	{-0.84052f, -0.073954f},
	{-0.326235f,-0.40583f},
	{-0.698464f, 0.457259f},
	{-0.203356f, 0.6205847f},
	{0.96345f,  -0.194353f},
	{0.473434f, -0.480026f},
	{0.519454f,  0.767034f},
	{0.185461f, -0.8945231f},
	{0.507351f,  0.064963f},
	{-0.321932f, 0.5954349f}
};

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture shadowMap : RENDERCOLORTARGET;

sampler shadowMapSampler = sampler_state
{
	Texture = <shadowMap>;
	AddressU  = CLAMP;
	AddressV  = CLAMP;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = None;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VertexInput
{	
	float4 position : POSITION;
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

float4 PS_GenerateShadowMap(GenerateShadowMapVS_OUTPUT input) : COLOR
{	
	return float4(input.depth, 1, 1, 1);
} // PS_GenerateShadowMap

//////////////////////////////////////////////
///////////// Render Shadow Map //////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position		: POSITION;
	float2 uv			: TEXCOORD0;
	float3 frustumRay	: TEXCOORD1;
};

float3 FrustumRay(in float2 uv)
{
	float  index = uv.x + (uv.y * 2);
	return frustumCorners[index];
}

VS_OUT vs_main(in float4 position : POSITION, in float2 uv : TEXCOORD)
{
	VS_OUT output = (VS_OUT)0;
	
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	output.uv = uv; 

	output.frustumRay = FrustumRay(uv);
	
	return output;
} // vs_main

//////////////////////////////////////////////
///// Calculate Shadow Term Bilinear PCF /////
//////////////////////////////////////////////

// Calculates the shadow occlusion using bilinear PCF
float CalculateShadowTermBilinearPCF(float positionLightSpace, float2 shadowTexCoord)
{
	// Transform to texel space
	float2 shadowMapCoord = shadowMapSize * shadowTexCoord;
    
	// Determine the lerp amounts
	float2 lerps = frac(shadowMapCoord);

	// Read in the 4 samples, doing a depth check for each
	float fSamples[4];	
	fSamples[0] = (tex2D(shadowMapSampler, shadowTexCoord).x                                                  + depthBias < positionLightSpace) ? 0.0f: 1.0f;  
	fSamples[1] = (tex2D(shadowMapSampler, shadowTexCoord + float2(invShadowMapSize.x, 0)).x                  + depthBias < positionLightSpace) ? 0.0f: 1.0f;  
	fSamples[2] = (tex2D(shadowMapSampler, shadowTexCoord + float2(0, invShadowMapSize.y)).x                  + depthBias < positionLightSpace) ? 0.0f: 1.0f;  
	fSamples[3] = (tex2D(shadowMapSampler, shadowTexCoord + float2(invShadowMapSize.x, invShadowMapSize.y)).x + depthBias < positionLightSpace) ? 0.0f: 1.0f;  
    
	// Lerp between the shadow values to calculate our light amount
	return lerp(lerp(fSamples[0], fSamples[1], lerps.x), lerp( fSamples[2], fSamples[3], lerps.x), lerps.y);							  
} // CalculateShadowTermBilinearPCF

//////////////////////////////////////////////
/////// Calculate Shadow Term Soft PCF ///////
//////////////////////////////////////////////

// Calculates the shadow term using PCF soft-shadowing
// sqrtSample is the filter size. I.e. 2 for 2x2 PCF, 3 for 3x3 PCF, etc.
float CalculateShadowTermSoftPCF(float positionLightSpace, float2 shadowTexCoord, int sqrtSamples)
{
	float shadowTerm = 0.0f;  
		
	float radius = (sqrtSamples - 1.0f) / 2;
	//float weightAccum = 0.0f;
	
	for (float y = -radius; y <= radius; y++)
	{
		for (float x = -radius; x <= radius; x++)
		{
			float2 offset = float2(x, y) / shadowMapSize;
			float sampleDepth = tex2D(shadowMapSampler, shadowTexCoord + offset).x;
			float sample = (positionLightSpace <= sampleDepth + depthBias);
			
			// Edge tap smoothing
			float xWeight = 1;
			float yWeight = 1;
			
			if (x == -radius)
				xWeight = 1 - frac(shadowTexCoord.x * shadowMapSize.x);
			else if (x == radius)
				xWeight = frac(shadowTexCoord.x * shadowMapSize.x);
				
			if (y == -radius)
				yWeight = 1 - frac(shadowTexCoord.y * shadowMapSize.y);
			else if (y == radius)
				yWeight = frac(shadowTexCoord.y * shadowMapSize.y);
				
			shadowTerm += sample * xWeight * yWeight;
			//weightAccum = xWeight * yWeight;
		}
	}
	
	shadowTerm /= (sqrtSamples * sqrtSamples);
	shadowTerm *= 1.55f;
	
	return shadowTerm;
} // CalculateShadowTermSoftPCF

// Calculates the shadow term using Poison destribution
float CalculateShadowTermPoisonPCF(float positionLightSpace, float2 shadowTexCoord)
{
	float shadowTerm = 0.0f;  

	for (int i = 0; i < 10; i++)
		shadowTerm += positionLightSpace > tex2D(shadowMapSampler, shadowTexCoord + FilterTaps[i] * invShadowMapSize).r + depthBias ? 0.0f : 1.0f / 10.0f;

	return shadowTerm;

} // CalculateShadowTermPoisonPCF