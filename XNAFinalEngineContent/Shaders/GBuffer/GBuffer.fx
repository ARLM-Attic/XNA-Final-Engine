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

//////////////////////////////////////////////
///////// Vertex and Pixel Shaders //////////
/////////////////////////////////////////////

#include <..\Helpers\VertexAndFragmentDeclarations.fxh>
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
/*
technique GBufferTerrain
{
    pass p0
    {
        VertexShader = compile vs_3_0 TerrainVS();
        PixelShader  = compile ps_3_0 TerrainPS();
    }
} // GBufferTerrain*/

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