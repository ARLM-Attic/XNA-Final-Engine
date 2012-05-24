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
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct WithoutTextureVS_INPUT 
{
   float4 position : POSITION;
   float3 normal   : NORMAL;   
};

struct WithTextureVS_INPUT
{
   float4 position : POSITION;
   float3 normal   : NORMAL;
   float2 uv       : TEXCOORD0;
};

struct WithTangentVS_INPUT
{
   float4 position : POSITION;
   float3 normal   : NORMAL;
   float3 tangent  : TANGENT;
   float3 binormal : BINORMAL;
   float2 uv       : TEXCOORD0;
};

struct WithoutTextureVS_OUTPUT 
{
   float4 position : POSITION0;
   float3 normal   : TEXCOORD0;
   float  depth    : TEXCOORD1;   
};

struct WithTextureVS_OUTPUT 
{
   float4 position         : POSITION0;
   float3 normal           : TEXCOORD0;
   float  depth            : TEXCOORD1;   
   float2 uv			   : TEXCOORD2;
};

struct WithTangentVS_OUTPUT 
{
   float4 position         : POSITION0;
   float  depth            : TEXCOORD0;
   float2 uv			   : TEXCOORD1;
   float3x3 tangentToView  : TEXCOORD2;
};

struct WithParallaxVS_OUTPUT 
{
   float4 position         : POSITION0;
   float  depth            : TEXCOORD0;
   float2 uv			   : TEXCOORD1;
   float2 parallaxOffsetTS : TEXCOORD2;
   float3 viewVS           : TEXCOORD3;
   float3x3 tangentToView  : TEXCOORD4;
};

//////////////////////////////////////////////
///////// Vertex and Pixel Shaders //////////
/////////////////////////////////////////////

#include <..\Helpers\ParallaxMapping.fxh>
#include <GBufferVertexShaders.fxh>
#include <GBufferPixelShaders.fxh>
#include <GBufferSkinning.fxh>

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique GBufferWithoutTexture
{
    pass p0
    {
        VertexShader = compile vs_3_0 WithoutTextureVS();
        PixelShader  = compile ps_3_0 WithoutTexturePS();
    }
} // GBufferWithoutTexture

technique GBufferWithSpecularTexture
{
    pass p0
    {
        VertexShader = compile vs_3_0 WithSpecularTextureVS();
        PixelShader  = compile ps_3_0 WithSpecularTexturePS();
    }
} // GBufferWithSpecularTexture

technique GBufferWithNormalMap
{
    pass p0
    {
        VertexShader = compile vs_3_0 WithNormalMapVS();
        PixelShader  = compile ps_3_0 WithNormalMapPS();
    }
} // GBufferWithNormalMap

technique GBufferWithParallax
{
    pass p0
    {
        VertexShader = compile vs_3_0 WithParallaxVS();
        PixelShader  = compile ps_3_0 WithParallaxPS();
    }
} // GBufferWithParallax

technique GBufferTerrain
{
    pass p0
    {
        VertexShader = compile vs_3_0 TerrainVS();
        PixelShader  = compile ps_3_0 TerrainPS();
    }
} // GBufferTerrain

technique GBufferSkinnedWithoutTexture
{
    pass p0
    {
        VertexShader = compile vs_3_0 SkinnedWithoutTextureVS();
        PixelShader  = compile ps_3_0 WithoutTexturePS();
    }
} // GBufferSkinnedWithTexture

technique GBufferSkinnedWithSpecularTexture
{
    pass p0
    {
        VertexShader = compile vs_3_0 SkinnedWithTextureVS();
        PixelShader  = compile ps_3_0 WithSpecularTexturePS();
    }
} // GBufferSkinnedWithSpecularTexture

technique GBufferSkinnedWithNormalMap
{
    pass p0
    {
        VertexShader = compile vs_3_0 SkinnedWithNormalMapVS();
        PixelShader  = compile ps_3_0 WithNormalMapPS();
    }
} // GBufferSkinnedWithNormalMap

technique GBufferSkinnedWithParallax
{
    pass p0
    {
        VertexShader = compile vs_3_0 SkinnedWithParallaxVS();
        PixelShader  = compile ps_3_0 WithParallaxPS();
    }
} // GBufferSkinnedWithParallax