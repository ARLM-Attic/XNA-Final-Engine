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

//////////////////////////////////////////////
///////////////// Script /////////////////////
//////////////////////////////////////////////

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=ModelShader30;";
	
> = 0.8; // version de SAS

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 windowSize : VIEWPORTPIXELSIZE;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture shadowMap
<
    string name = "default_bump_normal.dds";
	string UIName = "Shadow Map";
    string TextureType = "2D";
>;

sampler2D shadowMapSampler = sampler_state
{
    Texture = <shadowMap>;
    MinFilter = Point;
    MagFilter = Point;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VertexOutput
{
   	float4 Position	: POSITION;
    float2 UV		: TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VertexOutput ConvineShadowsVertexShaderFunction(float3 Position : POSITION,
                                                float2 TexCoord : TEXCOORD0)
{
    VertexOutput OUT;
	OUT.Position = float4(Position.xy, 0, 1);

	// En DirectX hay que considerar un offset de las texturas de 0.5f
	// Para esto necesitamos la resolucion usada.
	TexCoord += 0.5f / windowSize;
    OUT.UV = TexCoord;
	
	return OUT;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 ConvineShadowsPixelShaderFunction(float2 TexCoord :TEXCOORD0) : COLOR0
{
	return float4(0, 0, 0, 1 - tex2D(shadowMapSampler, TexCoord).r);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique ModelShader30 < 
	string Script =
			"ScriptExternal=color;"
        	"Pass=p0;";
                        >
{
	pass p0 < string Script = 
							  "RenderColorTarget0=;"
			                  "RenderDepthStencilTarget=;"
							  "Draw=Buffer;"
	        ;>
	{
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;
		CullMode = CCW;
		VertexShader = compile vs_3_0 ConvineShadowsVertexShaderFunction();
		PixelShader = compile ps_3_0 ConvineShadowsPixelShaderFunction();
	}
}