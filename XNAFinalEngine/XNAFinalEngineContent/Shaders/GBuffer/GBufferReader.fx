/***********************************************************************************************************************************************
Copyright (c) 2008-2011, Laboratorio de Investigaci?n y Desarrollo en Visualizaci?n y Computaci?n Gr?fica - 
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

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture depthTexture : register(t0);

sampler2D depthSampler : register(s0) = sampler_state
{
	Texture = <depthTexture>;
    /*ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;*/
};

texture normalTexture : register(t1);

sampler2D normalSampler : register(s1) = sampler_state
{
	Texture = <normalTexture>;
    /*ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;*/
};

texture motionVectorSpecularPowerTexture : register(t2);

sampler2D motionVectorSpecularPowerSampler : register(s2) = sampler_state
{
	Texture = <motionVectorSpecularPowerTexture>;
    /*ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;*/
};

//////////////////////////////////////////////
/////////////// Sample Normal ////////////////
//////////////////////////////////////////////

float3 SampleNormal(float2 uv, sampler2D textureSampler)
{
	float2 normalInformation = tex2D(textureSampler, uv).xy;
	float3 N;
	
	// Spheremap Transform (not working)
	/*N.z = dot(normalInformation, normalInformation) * 2 - 1; // lenght2 = dot(v, v)
	N.xy = normalize(normalInformation) * sqrt(1 - N.z * N.z);	*/	

	// Spherical Coordinates
	N.xy = -normalInformation * normalInformation + normalInformation;
	N.z = -1;
	float f = dot(N, float3(1, 1, 0.25));
	float m = sqrt(f);
	N.xy = (normalInformation * 8 - 4) * m;
	N.z = -(1 - 8 * f);
	
	// Basic form
	//float3 N = tex2D(normalSampler2, uv).xyz * 2 - 1;
	
	return N; // Already normalized
} // SampleNormal

float3 SampleNormal(float2 uv)
{
	return SampleNormal(uv, normalSampler);
} // SampleNormal

//////////////////////////////////////////////
/////////////// Specular Power ///////////////
//////////////////////////////////////////////

float DecompressSpecularPower(float compressedSpecularPower)
{
	return pow(2, compressedSpecularPower * 10.5);
} // DecompressSpecularPower
