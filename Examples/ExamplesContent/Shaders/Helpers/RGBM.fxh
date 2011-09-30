/***********************************************************************************************************************************************

From Xen: Graphics API for XNA 
License: Microsoft_Permissive_License
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

bool isRGBM;

float	maxRange = 50;

float3 RgbmLinearToFloatLinear(float4 rgbm)
{
	return rgbm.rgb * (maxRange * rgbm.a);
}

float4 LinearToRgbmLinear(float3 rgb)
{
	float maxRGB	= max(rgb.x, max(rgb.g, rgb.b));
	float m			= maxRGB / maxRange;

	m				= ceil(m * 255.0f) / 255.0;		// make sure to round up

	return saturate(float4(rgb / (m * maxRange), m));
}