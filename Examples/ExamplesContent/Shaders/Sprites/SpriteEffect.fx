/***********************************************************************************************************************************************
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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

#include <..\Helpers\GammaLinearSpace.fxh>
#include <..\Helpers\Discard.fxh>

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 worldViewProj : WorldViewProjection;
float4x4 projectionInverse;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float farPlane;
float2 halfPixel;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture diffuseTexture : register(t0);
sampler2D diffuseSampler : register(s0) = sampler_state
{
	Texture = <diffuseTexture>;
	/*MinFilter = ANISOTROPIC;
	MagFilter = ANISOTROPIC;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;*/
};

texture depthTexture   : register(t1);
sampler2D depthSampler : register(s1) = sampler_state
{
	Texture = <depthTexture>;
    /*ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;*/
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

void SpriteVertexShaderLinearSpace(inout float4 color    : COLOR0,
                                   inout float2 texCoord : TEXCOORD0,
                                   inout float4 position : POSITION0)
{
    position = mul(position, worldViewProj);
} // SpriteVertexShaderLinearSpace

void SpriteVertexShaderGammaSpace(inout float4 color    : COLOR0,
                                  inout float2 texCoord : TEXCOORD0,
                                  inout float4 position : Position0,
								  out   float  spriteDepth : TEXCOORD1,
								  out   float4 screenPosition : TEXCOORD2)
{
    position = mul(position, worldViewProj);
	screenPosition = position;
	spriteDepth = -mul(position, projectionInverse).z / farPlane;
} // SpriteVertexShaderGammaSpace

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 SpritePixelShaderLinearSpace(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : SV_Target0
{	
	float4 textureSampled = tex2D(diffuseSampler, texCoord).rgba;
	return float4(GammaToLinear(textureSampled.rgb * color), textureSampled.a * color.a);
} // SpritePixelShaderLinearSpace

float4 SpritePixelShaderGammaSpace(float4 color : COLOR0, float2 texCoord : TEXCOORD0, float spriteDepth : TEXCOORD1, float4 screenPosition : TEXCOORD2) : SV_Target0
{		
	// Obtain screen position. You have to do this in here so that the clip-space position interpolates correctly.
    screenPosition.xy /= screenPosition.w;

	// Obtain textureCoordinates corresponding to the current pixel
	// The screen coordinates are in [-1,1]*[1,-1]
    // The texture coordinates need to be in [0,1]*[0,1]
    float2 uv = 0.5f * (float2(screenPosition.x, -screenPosition.y) + 1) + halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/

	// Reconstruct position from the depth value, making use of the ray pointing towards the far clip plane	
	float depth = tex2D(depthSampler, uv).r;
		
    if(depth < spriteDepth)
	{
        Discard();
	}	
	return float4(tex2D(diffuseSampler, texCoord) * color);
} // SpritePixelShaderGammaSpace

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique SpriteBatchLinearSpace
{
    pass
    {
        VertexShader = compile vs_3_0 SpriteVertexShaderLinearSpace();
        PixelShader  = compile ps_3_0 SpritePixelShaderLinearSpace();
    }
}

technique SpriteBatchGammaSpace
{
    pass
    {
        VertexShader = compile vs_3_0 SpriteVertexShaderGammaSpace();
        PixelShader  = compile ps_3_0 SpritePixelShaderGammaSpace();
    }
}