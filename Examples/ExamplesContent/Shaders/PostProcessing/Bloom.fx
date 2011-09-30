/***********************************************************************************************************************************************

From Xen: Graphics API for XNA 
License: Microsoft_Permissive_License
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel;

// Lens exposure (fraction of light to display)
float	lensExposure = 0.1f;

// Bloom threshold.
float	bloomThreshold;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture sceneTexture;

sampler2D sceneSampler = sampler_state
{
	Texture = <sceneTexture>;
	MipFilter = POINT;
	MagFilter = POINT;
	MinFilter = POINT;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position		: POSITION;
	float2 uv			: TEXCOORD0;
};

//////////////////////////////////////////////
//////////////// Functions ///////////////////
//////////////////////////////////////////////

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_OUT vs_main(in float4 position : POSITION, in float2 uv : TEXCOORD)
{
	VS_OUT output = (VS_OUT)0;
	
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	output.uv = uv; 
	
	return output;
} // vs_main

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

// Generate the bloom source. Output the bright pixels that will bloom.
float4 ps_main(in float2 uv : TEXCOORD0) : COLOR0
{		
	float3 rgb = tex2D(sceneSampler, uv);
		
	// Apply lens exposure.
	float3 bloom = (rgb * lensExposure);
	
	//work out the intensity of the color
	float brightness = dot(bloom, float3(0.3, 0.6, 0.1));
		
	// Work out the gamma space color of the bloom (so it just stores the color of the bloom, not intensity)
	float3 bloomColor = sqrt(bloom / brightness);
	
	// Work out the intensity of the output bloom, based on the threshold.
	// Anything below the threshold will be zero.
	float bloomScale = max(0, (brightness - bloomThreshold) / bloomThreshold);
	
	// If you don't separate color from intensity, then you can get ulgy highly sautrated bloom.
	// This happens when a color such as orange (1.0, 0.5, 0.0) goes over a threshold in just
	// one color channel (eg, just the red channel). You get ulgy red bloom from an orange source.
	
	// Multiply this with the bloom color and output
	float3 bloomRgb = bloomColor * saturate(bloomScale);
	
	// Store the intensity 'booster' for bright bloom spots, as this will render to an 8bit RGBA texture and get heavily blurred
	// so it's very difficult to store spot bloom.
	float bloomIntensityBoost = saturate((bloomScale - 1) * 0.1);
	
	return float4(bloomRgb, bloomIntensityBoost);

} // ps_main

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique Bloom
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 ps_main();
	}
} // Bloom