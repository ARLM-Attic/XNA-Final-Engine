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

float2 textureResolution;

float blurWidth = 1.0f;

const float Weights8[8] =
{
	// more strength to middle to reduce effect of lighten up shadowed areas due mixing and bluring!
	0.035,
	0.09,
	0.125,
	0.25,
	0.25,
	0.125,
	0.09,
	0.035,
};

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture sceneTexture : register(t5);

sampler sceneTextureSamplerPoint : register(s5) = sampler_state 
{
    texture = <sceneTexture>;
	/*AddressU  = CLAMP;
    AddressV  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = POINT;
    MAGFILTER = POINT;*/
};

sampler sceneTextureSamplerLinear : register(s6) = sampler_state 
{
    texture = <sceneTexture>;
	/*AddressU  = CLAMP;
    AddressV  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;*/
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_Output
{
   	float4 position    : POSITION;
    float2 texCoord[8] : TEXCOORD0;
};

struct VS_OutputPoint
{
   	float4 position    : POSITION;
    float2 texCoord[7] : TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_Output VS_Blur(float4 position : POSITION, 
	              float2 texCoord : TEXCOORD0,
	              uniform float2 dir)
{
	VS_Output output = (VS_Output)0;
	
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
			
	float2 texelSize = 1.0 / textureResolution;
	float2 s = texCoord - texelSize*(8-1) *0.5 * dir * blurWidth;
	[unroll]
	for(int i = 0; i < 8; i++)
	{
		output.texCoord[i] = s + texelSize * i * dir * blurWidth;
	}
	return output;
}

VS_OutputPoint VS_BlurPoint(float4 position : POSITION, 
	                        float2 texCoord : TEXCOORD0,
	                        uniform float2 dir)
{
	VS_OutputPoint output = (VS_OutputPoint)0;
	
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
			
	float2 texelSize = 1.0 / textureResolution;
	output.texCoord[0] = texCoord - 3 * texelSize * dir;
	output.texCoord[1] = texCoord - 2 * texelSize * dir;
	output.texCoord[2] = texCoord -     texelSize * dir;
	output.texCoord[3] = texCoord;
	output.texCoord[4] = texCoord +     texelSize * dir;
	output.texCoord[5] = texCoord + 2 * texelSize * dir;	
	output.texCoord[6] = texCoord + 3 * texelSize * dir;	
	return output;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

// 8 linear samples (few cache misses)
float4 PS_Blur(VS_Output In) : COLOR
{	
	float4 ret = 0;
	[unroll]
	for (int i = 0; i < 8; i++)
	{
		ret += tex2D(sceneTextureSamplerLinear, In.texCoord[i]) * Weights8[i];
	}
	return ret;
}

// 7 point samples (few cache misses)
float4 PS_BlurPoint(VS_OutputPoint In) : COLOR
{	
	float4 ret = 0;
	ret += tex2D(sceneTextureSamplerPoint, In.texCoord[0]) * 0.05;
	ret += tex2D(sceneTextureSamplerPoint, In.texCoord[1]) * 0.1;
	ret += tex2D(sceneTextureSamplerPoint, In.texCoord[2]) * 0.2;
	ret += tex2D(sceneTextureSamplerPoint, In.texCoord[3]) * 0.3;
	ret += tex2D(sceneTextureSamplerPoint, In.texCoord[4]) * 0.2;
    ret += tex2D(sceneTextureSamplerPoint, In.texCoord[5]) * 0.1;
	ret += tex2D(sceneTextureSamplerPoint, In.texCoord[6]) * 0.05;
	return ret;
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique BlurLinear
{
	pass BlurHorizontal
	{
		VertexShader = compile vs_3_0 VS_Blur(float2(1, 0));		
		PixelShader  = compile ps_3_0 PS_Blur();
	}
	pass BlurVertical
	{
		VertexShader = compile vs_3_0 VS_Blur(float2(0, 1));		
		PixelShader  = compile ps_3_0 PS_Blur();
	}
}

technique BlurPoint
{
	pass BlurHorizontal
	{
		VertexShader = compile vs_3_0 VS_BlurPoint(float2(1, 0));		
		PixelShader  = compile ps_3_0 PS_BlurPoint();
	}
	pass BlurVertical
	{
		VertexShader = compile vs_3_0 VS_BlurPoint(float2(0, 1));
		PixelShader  = compile ps_3_0 PS_BlurPoint();
	}
}