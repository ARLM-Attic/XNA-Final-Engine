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
float2 invTextureResolution;
float blurRadius;
float blurFalloff;
float sharpness;

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

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_Output
{
   	float4 position    : POSITION;
	float2 uv			: TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_Output VS_Blur(float4 position : POSITION, float2 uv : TEXCOORD0)
{
	VS_Output output = (VS_Output)0;
	
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/	
	output.uv = uv; 		
	
	return output;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float BlurFunction(float2 uv, float r, float center_c, float center_d, inout float w_total)
{
    float c = tex2Dlod(sceneTextureSamplerPoint, float4(uv, 0, 0));
    float d = tex2Dlod(sceneTextureSamplerPoint, float4(uv, 0, 0)).r;

    float ddiff = d - center_d;
    float w = exp(-r * r * blurFalloff - ddiff * ddiff * sharpness);
    w_total += w;

    return w*c;
} // BlurFunction

float4 PS_BlurX(VS_Output input) : COLOR
{	
	float b = 0;
    float w_total = 0;
    float center_c = tex2D(sceneTextureSamplerPoint, input.uv);
    float center_d = tex2D(depthSampler, input.uv).r;
    	
    for (float r = -blurRadius; r <= blurRadius; ++r)
    {
        float2 uv = input.uv.xy + float2(r * invTextureResolution.x , 0);
        b += BlurFunction(uv, r, center_c, center_d, w_total);	
    }

    return b / w_total;
} // PS_BlurX

float4 PS_BlurY(VS_Output input) : COLOR
{
    float b = 0;
    float w_total = 0;
    float center_c = tex2D(sceneTextureSamplerPoint, input.uv);
    float center_d = tex2D(depthSampler, input.uv).r;
    
    for (float r = -blurRadius; r <= blurRadius; ++r)
    {
        float2 uv = input.uv.xy + float2(0, r * invTextureResolution.y); 
        b += BlurFunction(uv, r, center_c, center_d, w_total);
    }
	    
    return b/w_total;	
} // PS_BlurY

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

// When a shadow is blured is important to avoid leaks in the edges.
// Moreover this shader allows to a more aggressive and configurable blurring with just a little more processing cost.
technique BilateralBlur
{
	pass BlurHorizontal
	{
		VertexShader = compile vs_3_0 VS_Blur();		
		PixelShader  = compile ps_3_0 PS_BlurX();
	}
	pass BlurVertical
	{
		VertexShader = compile vs_3_0 VS_Blur();		
		PixelShader  = compile ps_3_0 PS_BlurY();
	}
} // BilateralBlur