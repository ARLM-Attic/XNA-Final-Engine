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

//#define FX_COMPOSER

const float4 ClearColor : DIFFUSE = { 0.0f, 0.0f, 0.0f, 1.0f};
const float ClearDepth = 1.0f;

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "preprocess";
	string ScriptOutput = "color";
	
	// We just call a script in the main technique.
	string Script = 
			"RenderColorTarget0=;"
			"ClearSetColor=ClearColor;"
		    "ClearSetDepth=ClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"Technique=ModelShader30;"
			"ScriptExternal=color;"
			;
> = 0.8;

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 ViewProj : VIEWPROJECTION <string UIWidget="None";>;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float SkyboxIntensity <
	string UIName = "Skybox Intensity";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 1.0f;

float AlphaBlending
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName = "Alpha Blending";
> = 1.0f;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture CubeMapTexture
<
    string ResourceName = "default_reflection.dds";
    string ResourceType = "CUBE";
>;

samplerCUBE CubeMapSampler = sampler_state
{
    Texture = <CubeMapTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    MipFilter = Linear;
	AddressU = CLAMP;
	AddressV = CLAMP;
	AddressW = CLAMP;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct CubeVertexOutput
{
   	float4 Position	: POSITION;
    float3 UV		: TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

CubeVertexOutput CubeVS(float3 Position : POSITION)
{
	CubeVertexOutput OUT;
	OUT.Position = float4(Position, 0);	
	OUT.Position = mul(OUT.Position , ViewProj).xyww;
    OUT.UV = Position.xyz;
	return OUT;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

half4 CubePS(CubeVertexOutput IN) : COLOR
{   
	half3 texCol = SkyboxIntensity * texCUBE(CubeMapSampler, IN.UV);
	return half4(texCol, AlphaBlending);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique ModelShader30 <
		string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=Buffer;";
	> {
		VertexShader = compile vs_3_0 CubeVS();
		CullMode = CW;
		ZWriteEnable = false;
#ifdef FX_COMPOSER
        AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;
#endif	
		PixelShader  = compile ps_3_0 CubePS();
	}
}