/***********************************************************************************************************************************************
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
                         Departamento de Ciencias e Ingeniería de la Computación - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
************************************************************************************************************************************************/

#include <..\GBuffer\GBufferReader.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel;
float2 quarterTexel;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

sampler2D normalLinearSampler : register(s2) = sampler_state
{
	Texture = <normalTexture>;
    /*ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;*/
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_Output
{
   	float4 position    : POSITION;
    float2 texCoord    : TEXCOORD0;
};

struct PixelShader_OUTPUT
{
    float4 depth                     : COLOR0;
    float4 normal                    : COLOR1;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_Output VS(float4 position : POSITION, 
	         float2 texCoord : TEXCOORD0)
{
	VS_Output output = (VS_Output)0;
	
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	output.texCoord = texCoord;
	
	return output;
} // VS

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

PixelShader_OUTPUT PS(VS_Output input)
{		
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
    // Downsample depth.
	// Read in the 4 samples, doing a depth check for each
	float fSamples[4];
	fSamples[0] = tex2D(depthSampler, input.texCoord + float2(-quarterTexel.x,  quarterTexel.y)).x;
	fSamples[1] = tex2D(depthSampler, input.texCoord + float2(-quarterTexel.x, -quarterTexel.y)).x;
	fSamples[2] = tex2D(depthSampler, input.texCoord + float2( quarterTexel.x,  quarterTexel.y)).x;  
	fSamples[3] = tex2D(depthSampler, input.texCoord + float2( quarterTexel.x, -quarterTexel.y)).x;  
	output.depth = float4(max(max(fSamples[0], fSamples[1]), max( fSamples[2], fSamples[3])), 1, 1, 1);
	// Donwsample normals. 
	// Applying typical color filters in normal maps is not the best way course of action, but is simple, fast and the resulting error is subtle.
	// Is important that the filter type is linear, not point.
    // If some error occurs them probably the surfaceformat does not support linear filter.
	output.normal = tex2D(normalLinearSampler, input.texCoord).rgba;
	return output;
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique DownsampleGBuffer
{
	pass p0	
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader  = compile ps_3_0 PS();
	}
}
