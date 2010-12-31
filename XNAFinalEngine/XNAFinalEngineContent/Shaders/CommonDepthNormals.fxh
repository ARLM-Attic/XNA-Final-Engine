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

// Faltaria considerar el near plane

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 worldView      : WorldView			        <string UIWidget="None";>;
float4x4 worldIT		: WorldInverseTranspose		<string UIWidget="None";>;
float4x4 worldViewProj  : WorldViewProjection       <string UIWidget="None";>;

//////////////////////////////////////////////
/////////////// Parametros ///////////////////
//////////////////////////////////////////////

float FarPlane <
	string UIWidget = "slider";
    float UIMin = 1;
    float UIMax = 1000;
    string UIName = "Far Plane";
> = 100.0f;

float NearPlane <
	string UIWidget = "slider";
    float UIMin = 1;
    float UIMax = 1000;
    string UIName = "Near Plane";
> = 1.0f;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture depthNormalTexture : RENDERCOLORTARGET
< 
    float2 ViewportRatio = { 1, 1 };
    int MIPLEVELS = 1;
	string Format="R10G10B10A2";
>;

sampler2D depthNormalSampler = sampler_state
{
	Texture = <depthNormalTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
};

texture highPresicionDepthTexture : RENDERCOLORTARGET
< 
    float2 ViewportRatio = { 1, 1 };
	string Format="R32F";
    int MIPLEVELS = 1;
>;

sampler2D highPresicionDepthSampler = sampler_state
{
	Texture = <highPresicionDepthTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct DepthNormalVS_INPUT 
{
   float4 Position: POSITION;
   float4 Normal : NORMAL;
};

struct DepthNormalVS_OUTPUT 
{
   float4 Position : POSITION0;
   float3 Normal   : TEXCOORD0;
   float  Depth    : TEXCOORD1;
};

struct PixelShader_OUTPUT
{
    float4 Depth   : COLOR0;
    float4 Normals : COLOR1;
	float4 Normals2 : COLOR2;
	float4 Normals3 : COLOR3;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

DepthNormalVS_OUTPUT DepthNormalsVertexShaderFunction(DepthNormalVS_INPUT IN)
{
   DepthNormalVS_OUTPUT Output;
   
   Output.Position = mul(IN.Position, worldViewProj);
   Output.Depth = mul(IN.Position, worldView).z;
   Output.Normal = mul(IN.Normal, worldIT);
   
   return Output;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

PixelShader_OUTPUT DepthNormalsPixelShaderFunction(DepthNormalVS_OUTPUT IN)
{
	PixelShader_OUTPUT output = (PixelShader_OUTPUT)0;
 
	output.Depth = float4(1 - (-IN.Depth / FarPlane), 1, 1, 1);
 
//	output.Normals = 0.5f * (float4(IN.Normal.x, IN.Normal.y, IN.Normal.z, 1) + 1.0f); // Change to the [0, 1] range to avoid negative values.		
 
	output.Normals = float4(normalize(IN.Normal.xy) * sqrt(-IN.Normal.z * 0.5 + 0.5), 1, 1); // Spheremap Transform: Crytek method

	return output;
}
// For FX Composer //
float4 OnlyNormalsPixelShaderFunction(DepthNormalVS_OUTPUT IN) : COLOR0
{
	IN.Normal = normalize(IN.Normal);
	return float4(IN.Normal.x, IN.Normal.y, IN.Normal.z, 1);
}

float4 HighPresicionDepthPixelShaderFunction(DepthNormalVS_OUTPUT IN) : COLOR0
{
	return float4(1 - (-IN.Depth / FarPlane), 1, 1, 1);
}