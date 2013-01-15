/***********************************************************************************************************************************************

From AMD
Paper: Layered Car Paint Shader
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

#include <..\GBuffer\GBufferReader.fxh>
#include <..\Helpers\VertexAndFragmentDeclarations.fxh>
#include <..\Helpers\GammaLinearSpace.fxh>
#include <..\Helpers\RGBM.fxh>

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 worldViewProj : WorldViewProjection;
float4x4 worldIT       : WorldInverseTranspose;
float4x4 world         : World;
float3x3 viewI         : ViewInverse;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel;

// Value between -1 and 1.
float microflakePerturbation;

// Value between 0 and 1.
float microflakePerturbationA;

// Value between -1 and 1.
float normalPerturbation;

// Value between -1 and 10.
float specularIntensity = 6;

float3 cameraPosition;

float3 basePaintColor1;
float3 basePaintColor2;
float3 basePaintColor3;

// Flakes
float3 flakeLayerColor;
float flakesScale = 20;
int flakesExponent = 16;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture microflakeMap : register(t0);;
sampler2D microflakeSampler : register(s0) = sampler_state
{
	Texture = <microflakeMap>;
	/*MipFilter = LINEAR;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;*/
};

texture reflectionTexture : register(t3);
samplerCUBE reflectionSampler : register(s3) = sampler_state
{
	Texture = <reflectionTexture>;
	/*MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;*/
};

texture diffuseAccumulationTexture : register(t4);
sampler2D diffuseAccumulationSampler : register(s4) = sampler_state
{
	Texture = <diffuseAccumulationTexture>;
	/*MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;*/
};

texture specularAccumulationTexture : register(t5);
sampler2D specularAccumulationSampler : register(s5) = sampler_state
{
	Texture = <specularAccumulationTexture>;
	/*MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;*/
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position         : POSITION;
	float4 postProj         : TEXCOORD0;	
	float2 sparkleUv        : TEXCOORD1;
	float3 view             : TEXCOORD2;
	float3x3 tangentToWorld : TEXCOORD3;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_OUT vs_main(WithTangentVS_INPUT input)
{
	VS_OUT output = (VS_OUT)0;

	output.position = mul(input.position, worldViewProj);
	output.postProj = output.position;
	
    // Generate the tanget space to world space matrix. 
	// View space doesn't work in this case because the reflection vector is in world space.
    output.tangentToWorld[0] = mul(input.tangent,  worldIT);
    output.tangentToWorld[1] = mul(input.binormal, worldIT); // binormal = cross(input.tangent, input.normal)
    output.tangentToWorld[2] = mul(input.normal,   worldIT);

	// Compute microflake tiling factor:
    output.sparkleUv = input.uv * flakesScale;

	float3 positionWorld = mul(input.position, world).xyz;	
	output.view = normalize(cameraPosition - positionWorld);
	
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

float4 ps_main(in float4 positionProj : TEXCOORD0, float2 sparkleUv : TEXCOORD1, float3 view : TEXCOORD2, float3x3 tangentToWorld : TEXCOORD3) : COLOR
{
	// Find the screen space texture coordinate & offset
	float2 lightMapUv = PostProjectToScreen(positionProj) + halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	
	// Diffuse contribution + specular contribution.
	float3 diffuseAccumulation  = tex2D(diffuseAccumulationSampler, lightMapUv);
	float3 specularAccumulation = tex2D(specularAccumulationSampler, lightMapUv);
			
	float3 normalCompressed = tex2Dlod(normalSampler, float4(lightMapUv, 0, 0)).xyz;
	float3 normalWS = mul(DecompressNormal(normalCompressed.xyz), viewI);
		
	// Microflakes normal map is a high frequency normalized
    // vector noise map which is repeated across all surface. 
    // Fetching the value from it for each pixel allows us to 
    // compute perturbed normal for the surface to simulate
    // appearance of microflakes suspected in the coat of paint.
	// Don't forget to bias and scale to shift color into [-1.0, 1.0] range:
	float3 flakesNormal = tex2D(microflakeSampler, sparkleUv) * 2.0f - 1.0f;

	// This shader simulates two layers of microflakes suspended in 
    // the coat of paint. To compute the surface normal for the first layer,
    // the following formula is used:
    //   Np1 = ( a * Np + b * N ) /  || a * Np + b * N || where a << b        
	float3 Np1World = normalize(mul(microflakePerturbationA * flakesNormal, tangentToWorld) + normalPerturbation * normalWS);

    // To compute the surface normal for the second layer of microflakes, which
    // is shifted with respect to the first layer of microflakes, we use this formula:
    //    Np2 = ( c * Np + d * N ) / || c * Np + d * N || where c == d    
	float3 Np2World = normalize(microflakePerturbation * (mul(flakesNormal, tangentToWorld) + normalWS));
	
	// The view vector (which is currently in world space) needs to be normalized.
    // This vector is normalized in the pixel shader to ensure higher precision of
    // the resultinv view vector. For this highly detailed visual effect normalizing
    // the view vector in the vertex shader and simply interpolating it is insufficient
    // and produces artifacts.
    view =  normalize(view);
	
	// Compute reflection vector resulted from the clear coat of paint on the metallic surface:    
    float3 reflectionUv = reflect(view, normalWS);

	float3 reflection;
	[branch]
	if (isRGBM)
		reflection = RgbmLinearToFloatLinear(GammaToLinear(texCUBE(reflectionSampler, reflectionUv).rgba));
	else
		reflection = GammaToLinear(texCUBE(reflectionSampler, reflectionUv).rgb);
	    
    // Brighten the environment map sampling result:
    reflection.rgb *= specularIntensity;

    // Compute modified Fresnel term for reflections from the first layer of
    // microflakes. First transform perturbed surface normal for that layer into 
    // world space and then compute dot product of that normal with the view vector:    
    float  fresnel1 = saturate(dot(Np1World, view));

    // Compute modified Fresnel term for reflections from the second layer of 
    // microflakes. Again, transform perturbed surface normal for that layer into 
    // world space and then compute dot product of that normal with the view vector:    
    float  fresnel2 = saturate(dot(Np2World, view));

    // Compute final paint color: combines all layers of paint as well as two layersof microflakes
	float  fresnel1Sq = fresnel1 * fresnel1;
	float3 flakes =  pow(fresnel2, flakesExponent) * flakeLayerColor;	
    float3 paintColor = fresnel1 * GammaToLinear(basePaintColor1) + fresnel1Sq * GammaToLinear(basePaintColor2) +
	                    fresnel1Sq * fresnel1Sq * GammaToLinear(basePaintColor3) +
						flakes;

    // View depend reflections are more realistic.
	float  NdotV = saturate(dot(normalWS, view));
    float  envContribution = 1.0 - 0.5 * NdotV;
				          
    return float4(reflection * envContribution * specularAccumulation.rgb + paintColor * diffuseAccumulation.rgb, 1);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique CarPaint
{
    pass P0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader  = compile ps_3_0 ps_main();
    }
} // CarPaint
