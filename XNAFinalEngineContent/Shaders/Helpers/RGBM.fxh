/***********************************************************************************************************************************************

From Xen: Graphics API for XNA 
License: Microsoft_Permissive_License
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/
// RGBD could be useful in some scenarios. For instance, Capcom’s engine uses RGBD for some assets and RGBM for others,
// because RGBD could be easily interchangeable with sRGB textures and RGBM has a better distribution of values.
// LogLuv is slower than this formats but it is a great format because it has great range with good presicion, and a better gamut.

// All this formats don’t work well with hardware’s alpha composition, texture filtering and multisampling.
// If you create an RGBM texture you can use the DXT5 format, but you can lost too much precision in the M channel.
// There are alternatives, but they are too complex for a medium size project.

bool isRGBM;

// If maxRange is too big then the precision could suffers. Be careful.
float maxRange = 50;

float3 RgbmLinearToFloatLinear(float4 rgbm)
{
	return rgbm.rgb * (maxRange * rgbm.a);
} // RgbmLinearToFloatLinear

float4 LinearToRgbmLinear(float3 rgb)
{
	float maxRGB	= max(rgb.x, max(rgb.g, rgb.b));
	float m			= maxRGB / maxRange;

	m				= ceil(m * 255.0f) / 255.0;		// make sure to round up

	return saturate(float4(rgb / (m * maxRange), m));
} // LinearToRgbmLinear

