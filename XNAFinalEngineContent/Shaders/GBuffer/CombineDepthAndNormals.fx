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

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture depthOpaqueTexture : register(t0);
sampler depthOpaqueSampler : register(s0) = sampler_state 
{
    texture = <depthOpaqueTexture>;
    /*AddressU  = Clamp;
    AddressV  = Clamp;
    AddressW  = Clamp;    
    MINFILTER = POINT;
    MAGFILTER = POINT;
	MIPFILTER = NONE;*/
};

texture normalsOpaqueTexture : register(t1);
sampler normalsOpaqueSampler : register(s1) = sampler_state 
{
    texture = <normalsOpaqueTexture>;
    /*AddressU  = Clamp;
    AddressV  = Clamp;
    AddressW  = Clamp;    
    MINFILTER = POINT;
    MAGFILTER = POINT;
	MIPFILTER = NONE;*/
};

texture depthFoliageTexture : register(t2);
sampler deptFoliageSampler : register(s2) = sampler_state
{
    texture = <depthFoliageTexture>;
    /*AddressU  = Clamp;
    AddressV  = Clamp;
    AddressW  = Clamp;    
    MINFILTER = POINT;
    MAGFILTER = POINT;
	MIPFILTER = NONE;*/
};

texture normalsFoliageTexture : register(t3);
sampler normalsFoliageSampler : register(s3) = sampler_state
{
    texture = <normalsFoliageTexture>;
    /*AddressU  = Clamp;
    AddressV  = Clamp;
    AddressW  = Clamp;    
    MINFILTER = POINT;
    MAGFILTER = POINT;
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

struct PS_Output
{
   	float4 depth       : COLOR0;
    float4 normals     : COLOR1;
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

PS_Output PS(VS_Output input)
{		
	PS_Output output = (PS_Output)0;

	float foliageDepth = tex2D(deptFoliageSampler, input.texCoord).x;	
	[branch]
	if (foliageDepth < 1)
	{
		output.depth.x = foliageDepth;
		output.normals.xy = tex2D(normalsFoliageSampler, input.texCoord).xy;
		return output;
	}
	output.depth.x = tex2D(depthOpaqueSampler, input.texCoord).x;
	output.normals.xy = tex2D(normalsOpaqueSampler, input.texCoord).xy;
	return output;
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique CombineDepthNormals
{
	pass p0	
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader  = compile ps_3_0 PS();
	}
}
