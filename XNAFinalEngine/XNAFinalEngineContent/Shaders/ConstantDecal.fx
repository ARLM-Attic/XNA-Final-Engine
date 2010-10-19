/***********************************************************************************************************************************************
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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

#include <Common.fxh>
//#define FX_COMPOSER

//////////////////////////////////////////////
///////////////// Script /////////////////////
//////////////////////////////////////////////

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?ConstantWithTexture:ConstantWithoutTexture;";
> = 0.8;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture AlphaTexture : DIFFUSE
<
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

sampler2D AlphaSampler = sampler_state
{
	Texture = <AlphaTexture>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct vertexOutput {
    float4 HPosition	: POSITION;
};

struct vertexOutputWT {
    float4 HPosition	: POSITION;
	float2 UV		    : TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

vertexOutputWT VSConstantDecal(appdata IN) {
	
    vertexOutputWT OUT;
    OUT.HPosition = mul(IN.Position, WorldViewProj);		// screen clipspace coords
	
	OUT.UV = float2(UScale, VScale) * IN.UV.xy;				// Mosaic effect
    return OUT;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 PSConstantDecalWithoutTexture(vertexOutputWT IN) : COLOR {
	float  alpha = tex2D(AlphaSampler, IN.UV).r * AlphaBlending;
    return float4(SurfaceColor, alpha);
}

float4 PSConstantDecalWithTexture(vertexOutputWT IN) : COLOR {
    float3 colorTex = tex2D(DiffuseSampler, IN.UV).rgb;
	float  alpha = tex2D(AlphaSampler, IN.UV).r;
    return float4(colorTex.rgb, alpha * AlphaBlending);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique ConstantDecalWithoutTexture
{
	pass P0
	{
		VertexShader = compile vs_3_0 VSConstantDecal();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
        AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;
#else
        CullMode = CCW;    // For The Engine	
#endif	
		PixelShader = compile ps_3_0 PSConstantDecalWithoutTexture();
	}
}

technique ConstantDecalWithTexture
{
	pass P0
	{
		VertexShader = compile vs_3_0 VSConstantDecal();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
        AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;
#else
        CullMode = CCW;    // For The Engine	
#endif	
		PixelShader = compile ps_3_0 PSConstantDecalWithTexture();
	}
}