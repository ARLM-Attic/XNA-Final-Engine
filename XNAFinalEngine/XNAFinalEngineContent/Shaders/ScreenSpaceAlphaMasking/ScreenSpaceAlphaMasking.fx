
//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 worldViewProj : WorldViewProjection;
float4x4 worldView     : WorldView;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel;

float farPlane;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture depthTexture;

sampler2D depthSampler = sampler_state
{
	Texture = <depthTexture>;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
};

texture lightMap;

sampler2D lightSampler = sampler_state
{
	Texture = <lightMap>;
	MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;
};

texture maskTexture;

sampler2D maskSampler = sampler_state
{
	Texture = <maskTexture>;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
};

texture foliageColorTexture;

sampler2D foliageColorSampler = sampler_state
{
	Texture = <foliageColorTexture>;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
};

texture diffuseTexture;

sampler2D diffuseSampler = sampler_state
{
	Texture = <diffuseTexture>;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MinFilter = ANISOTROPIC;
	MagFilter = ANISOTROPIC;
	MipFilter = LINEAR;	
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position		: POSITION;
	float4 positionProj : TEXCOORD0;
	float4 viewPosition : TEXCOORD1;
	float2 uv			: TEXCOORD2;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_OUT vs_main(in float4 position : POSITION, in float2 uv : TEXCOORD0)
{	
    VS_OUT output;
	
    output.position = mul(position, worldViewProj);
	output.positionProj = output.position;
	output.viewPosition = mul(position, worldView);
    
	output.uv = uv;
		
    return output;
} // vs_main

VS_OUT FinalColorVS(in float4 position : POSITION, in float2 uv : TEXCOORD)
{
	VS_OUT output = (VS_OUT)0;
	
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	output.uv = uv; 
	
	return output;
} // FinalColorVS

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float2 PostProjectToScreen(float4 pos)
{
	float2 screenPosition = pos.xy / pos.w;
	// Screen position to uv coordinates.
	return (0.5f * (float2(screenPosition.x, -screenPosition.y) + 1));
} // PostProjectToScreen

float4 ps_main(in float4 positionProj : TEXCOORD0, in float4 viewPosition : TEXCOORD1, in float2 uv : TEXCOORD2) : COLOR
{
	// Find the screen space texture coordinate & offset
	float2 depthMapUv = PostProjectToScreen(positionProj) + halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	
	// Diffuse contribution + specular exponent.
	float depth = tex2D(depthSampler, depthMapUv);
		
	if (depth < -viewPosition.z / farPlane)
	{
		// discard' doesn't actually exit the shader (the shader will continue to execute). 
		// It merely instructs the output merger stage not to output the result of the pixel (which must still be returned by the shader).
		// The pair discard return rocks!!! And you should be use it. However, the xbox 360 doesn't support it, I think.
		discard;
		return float4(1, 0, 0, 1);
	}	
	return tex2D(diffuseSampler, uv).aaaa;
} // ps_main

float4 FinalColorPS(in float2 uv : TEXCOORD2) : COLOR0
{
	float4 maskPixel = tex2D(maskSampler, uv);
	float mask = lerp(maskPixel.x, maskPixel.w, 0.6);

	float4 light = tex2D(lightSampler, uv).rgba;

	return float4(tex2D(foliageColorSampler, uv).rgb * light.rgb, mask * mask);
} // FinalColorPS

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique SSAM
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main();
	}
} // SSAM

technique FinalColor
{
	pass p0
	{
		VertexShader = compile vs_3_0 FinalColorVS();
		PixelShader  = compile ps_3_0 FinalColorPS();
	}
} // FinalColor