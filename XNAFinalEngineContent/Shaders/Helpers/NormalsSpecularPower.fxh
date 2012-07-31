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

//#define BEST_FIT_NORMALS

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture normalsFittingTexture   : register(t7);
sampler2D normalsFittingSampler : register(s7) = sampler_state
{
  Texture = <normalsFittingTexture>;
  /*MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = POINT;
  AddressU = Clamp;
  AddressV = Clamp;*/
};

//////////////////////////////////////////////
////////////////// Normals ///////////////////
//////////////////////////////////////////////

// Helper for best fit normals method.
float Quantize255(float c)
{
	float w = saturate(c * .5f + .5f);
	float r = round(w * 256.f);
	float v = r / 256.f * 2.f - 1.f;
	return v;
} // Quantize255

// Brute force method to compress normals using best fit normals method.
// The cube texture stores the result of this method.
float3 FindMin(in half3 normal)
{
	normal /= max(abs(normal.x), max(abs(normal.y), abs(normal.z)));

	float fMinError = 100000.f;
	float3 fOut = normal;

	for(float nStep = 1.5f; nStep <= 127.5f; ++nStep)
	{
		float t = nStep / 127.5f;

		// compute the probe
		float3 vP = normal * t;

		// quantize the probe
		float3 vQuantizedP = float3(Quantize255(vP.x), Quantize255(vP.y), Quantize255(vP.z));

		// error computation for the probe
		float3 vDiff = (vQuantizedP - vP) / t;
		float fError = max(abs(vDiff.x), max(abs(vDiff.y), abs(vDiff.z)));

		// find the minimum
		if(fError < fMinError)
		{
			fMinError = fError;
			fOut = vQuantizedP;
		}
	}
	return fOut;
} // FindMin

// A 24 bits buffer allows to store 16 millions of values.
// But when you store NORMALIZED normals just 2 percent of these values are used. 
// This method search for scale value that maximize the use of the 24 bits value maintaining a good precision. The best part is the decompression is just a normalize operation.
// Why bother, because I need the extra channel and the extra channel (specular power) will be read at the same time as the normals so the fetch is amortized.
half3 CompressNormalBestFit(half3 vNormal)
{
	// Renormalize (needed if any blending or interpolation happened before)
	vNormal.rgb = normalize(vNormal.rgb);

	// This bruteforce computation could be enabled for the highest spec in the future.
	//vNormal.rgb = FindMin(vNormal.rgb);
	
	// Get unsigned normal for cubemap lookup (note the full float presision is required)
	half3 vNormalUns = abs(vNormal.rgb);
	// Get the main axis for cubemap lookup
	half maxNAbs = max(vNormalUns.z, max(vNormalUns.x, vNormalUns.y));
	// Get texture coordinates in a collapsed cubemap
	float2 vTexCoord = vNormalUns.z < maxNAbs ? (vNormalUns.y < maxNAbs ? vNormalUns.yz : vNormalUns.xz) : vNormalUns.xy;
	vTexCoord = vTexCoord.x < vTexCoord.y ? vTexCoord.yx : vTexCoord.xy;
	vTexCoord.y /= vTexCoord.x;
	// Fit normal into the edge of unit cube
	vNormal.rgb /= maxNAbs;

	// Look-up fitting length and scale the normal to get the best fit
	float fFittingScale = tex2D(normalsFittingSampler, vTexCoord).a;
	
	// Scale the normal to get the best fit
	vNormal.rgb *= fFittingScale;

	// Squeeze to unsigned.
    vNormal.rgb = vNormal.rgb * .5h + .5h;

	return vNormal;
} // CompressNormalBestFit

float3 CompressNormal(float3 inputNormal)
{
	#if defined(BEST_FIT_NORMALS)
		// Compress the normal using best fit normals. This gives us good precision with 24 bits (that gives an extra free chaneel)
		return CompressNormalBestFit(inputNormal);
	#else
		// Compress the normal using spherical coordinates. This gives us more precision with an acceptable space (32 bits).
		float f = inputNormal.z * 2 + 1;
		float g = dot(inputNormal, inputNormal);
		float p = sqrt(g + f);
		return float3(inputNormal.xy / p * 0.5 + 0.5, 1);
	#endif	
} // CompressNormal

float3 DecompressNormal(float3 normalCompressed)
{
	#if defined(BEST_FIT_NORMALS)
		// Expand from unsigned
		normalCompressed.xyz = 2 * normalCompressed.xyz - 1;
		// renormalization is required because of fitted normal's length
		normalCompressed.xyz = normalize(normalCompressed.xyz);
		return normalCompressed;
	#else	
		float3 N;
		// Spherical Coordinates
		N.xy = -normalCompressed.xy * normalCompressed.xy + normalCompressed.xy;
		N.z = -1;
		float f = dot(N, float3(1, 1, 0.25));
		float m = sqrt(f);
		N.xy = (normalCompressed.xy * 8 - 4) * m;
		N.z = -(1 - 8 * f);
		return N; // Already normalized.
	#endif
} // DecompressNormal

//////////////////////////////////////////////
/////////////// Specular Power ///////////////
//////////////////////////////////////////////

float DecompressSpecularPower(float compressedSpecularPower)
{
	return pow(2, compressedSpecularPower * 10.5);
} // DecompressSpecularPower

/// Compress to the (0,1) range with high precision for low values. Guerilla method.
float CompressSpecularPower(float specularPower)
{
	return log2(specularPower) / 10.5;
} // CompressSpecularPower
