/***********************************************************************************************************************************************

From MJP http://mynameismjp.wordpress.com/
License: Microsoft_Permissive_License
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

This is a potpourri of tone mapping functions that MJP gather.
They are some of the most popular right now.

References:
	http://mynameismjp.wordpress.com/2010/04/30/a-closer-look-at-tone-mapping/
	http://content.gpwiki.org/index.php/D3DBook:High-Dynamic_Range_Rendering
	http://filmicgames.com/archives/75

************************************************************************************************************************************************/

#include <..\Helpers\GammaLinearSpace.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

// Logarithmic
float whiteLevel;
float luminanceSaturation;
// Drago
float bias;
// Uncharted 2
float shoulderStrength;
float linearStrength;
float linearAngle;
float toeStrength;
float toeNumerator;
float toeDenominator;
float linearWhite;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture2D filmLutTexture : register(t11);;
sampler2D filmLutSampler : register(s11) = sampler_state
{
	Texture = <filmLutTexture>;	
	/*MipFilter = NONE;
	MagFilter = LINEAR;
	MinFilter = LINEAR;*/
};

//////////////////////////////////////////////
//////////////// Functions ///////////////////
//////////////////////////////////////////////

// Approximates luminance from an RGB value
float CalcLuminance(float3 color)
{
    return max(dot(color, float3(0.299f, 0.587f, 0.114f)), 0.0001f);
}

// Logarithmic mapping
float3 ToneMapLogarithmic(float3 color)
{
	float pixelLuminance = CalcLuminance(color);    
    float toneMappedLuminance = log10(1 + pixelLuminance) / log10(1 + whiteLevel);
	return LinearToGamma(toneMappedLuminance * pow(color / pixelLuminance, luminanceSaturation));
}

// Drago's Logarithmic mapping
float3 ToneMapDragoLogarithmic(float3 color)
{
	float pixelLuminance = CalcLuminance(color);    
    float toneMappedLuminance = log10(1 + pixelLuminance);
	toneMappedLuminance /= log10(1 + whiteLevel);
	toneMappedLuminance /= log10(2 + 8 * (pow((pixelLuminance / whiteLevel), log10(bias) / log10(0.5f))));
	return LinearToGamma(toneMappedLuminance * pow(color / pixelLuminance, luminanceSaturation)); 
}

// Exponential mapping
float3 ToneMapExponential(float3 color)
{
	float pixelLuminance = CalcLuminance(color);    
    float toneMappedLuminance = 1 - exp(-pixelLuminance / whiteLevel); // The original is calculated with average luminance.
	return LinearToGamma(toneMappedLuminance * pow(color / pixelLuminance, luminanceSaturation));
}

// Applies Reinhard's basic tone mapping operator
float3 ToneMapReinhard(float3 color) 
{
	float pixelLuminance = CalcLuminance(color);    
    float toneMappedLuminance = pixelLuminance / (pixelLuminance + 1);
	return LinearToGamma(toneMappedLuminance * pow(color / pixelLuminance, luminanceSaturation));
}

// Applies Reinhard's modified tone mapping operator
float3 ToneMapReinhardModified(float3 color) 
{    
    float pixelLuminance = CalcLuminance(color);
	float toneMappedLuminance = pixelLuminance * (1.0f + pixelLuminance / (whiteLevel * whiteLevel)) / (1.0f + pixelLuminance);
	return LinearToGamma(toneMappedLuminance * pow(color / pixelLuminance, luminanceSaturation));
}

// Applies the filmic curve from John Hable's presentation.
// It only uses ALU operations, and replaces the entire Lin/Log and Texture LUT.
// Moreover, this approximation performs gamma correction.
// In other words is very fast and is one of the best tone mapping function available. 
float3 ToneMapFilmicALU(float3 color)
{
    color = max(0, color - 0.004f);
    color = (color * (6.2f * color + 0.5f)) / (color * (6.2f * color + 1.7f)+ 0.06f);
	// Is already in gamma space.
    return color;
}

// Function used by the Uncharte2D tone mapping curve
float3 Uncharted2Function(float3 x)
{
    float A = shoulderStrength;
    float B = linearStrength;
    float C = linearAngle;
    float D = toeStrength;
    float E = toeNumerator;
    float F = toeDenominator;
    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

// Applies the Uncharted 2 filmic tone mapping curve
float3 ToneMapFilmicUncharted2(float3 color)
{
    float3 numerator   = Uncharted2Function(color);        
    float3 denominator = Uncharted2Function(linearWhite);

    return LinearToGamma(numerator / denominator);
}

// Haarm-Pieter Duiker
float3 ToneMapDuiker(float3 color) : COLOR
{
	float3 ld = 0.002;
	float linReference = 0.18;
	float logReference = 444;
	float logGamma = 0.45;
 
	float3 logColor;
	logColor = (log10(0.4 * color / linReference) / ld * logGamma + logReference) / 1023.f;
	logColor = saturate(logColor);
 
	float filmLutWidth = 256;
	float padding = 0.5f / filmLutWidth;
 
	// Apply response lookup and color grading for target display.
	float3 retColor;
	retColor.r = tex2D(filmLutSampler, float2(lerp(padding, 1 - padding, logColor.r), 0.5)).r;
	retColor.g = tex2D(filmLutSampler, float2(lerp(padding, 1 - padding, logColor.g), 0.5)).r;
	retColor.b = tex2D(filmLutSampler, float2(lerp(padding, 1 - padding, logColor.b), 0.5)).r;
	// Is already in gamma space.
	return retColor;
}