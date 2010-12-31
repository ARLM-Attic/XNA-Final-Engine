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
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 WorldIT       : WorldInverseTranspose <string UIWidget="None";>;
float4x4 WorldViewProj : WorldViewProjection   <string UIWidget="None";>;
float4x4 World         : World                 <string UIWidget="None";>;
float4x4 ViewI         : ViewInverse           <string UIWidget="None";>;
float4x4 ViewProj	   : ViewProjection        <string UIWidget="None";>;

//////////////////////////////////////////////
///////////////// Lights /////////////////////
//////////////////////////////////////////////

// Ambient light //

float3 AmbientLightColor : SPECULAR <
	string UIName = "Ambient Light Color";
	string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f};

// Point light //

float3 PointLightPos : POSITION <
	string UIName = "Point Position 1";
	string Object = "PointLight";
	string Space = "World";
	int refID = 0;
> = {500.0f, 500.0f, 0.0f};

float3 PointLightColor : SPECULAR <
	string UIName = "Point Light Color 1";
	string UIWidget = "Color";
	int refID = 0;
> = {0.0f, 0.0f, 0.0f};

// Point light 2 //

float3 PointLightPos2 : POSITION <
	string UIName = "Point Position 2";
	string Object = "PointLight";
	string Space = "World";
	int refID = 4;
> = {500.0f, 500.0f, 0.0f};

float3 PointLightColor2 : SPECULAR <
	string UIName = "Point Light Color 2";
	string UIWidget = "Color";
	int refID = 4;
> = {0.0f, 0.0f, 0.0f};

// Point light 3 //

float3 PointLightPos3 : POSITION <
	string UIName = "Point Position 3";
	string Object = "PointLight";
	string Space = "World";
	int refID = 5;
> = {500.0f, 500.0f, 0.0f};

float3 PointLightColor3 : SPECULAR <
	string UIName = "Point Light Color 3";
	string UIWidget = "Color";
	int refID = 5;
> = {0.0f, 0.0f, 0.0f};

// Directional light //

float3 DirectionalLightDir : DIRECTION <
	string UIName = "Directional Direction";
	string Object = "DirectionalLight";
	string Space = "World";
	int refID = 1;
> = {-0.65f, 0.65f, 0.39f};

float3 DirectionalLightColor : SPECULAR <
	string UIName = "Directional Light Color";
	string UIWidget = "Color";
	int refID = 1;
> = {0.0f, 0.0f, 0.0f};

// Directional light 2 //

float3 DirectionalLightDir2 : DIRECTION <
	string UIName = "Directional Direction 2";
	string Object = "DirectionalLight";
	string Space = "World";
> = {-0.65f, 0.65f, 0.39f};

float3 DirectionalLightColor2 : SPECULAR <
	string UIName = "Directional Light Color 2";
	string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f};

// Spot light //

float3 SpotLightPos : POSITION <
	string UIName = "Spot Position";
	string Object = "SpotLight";
	string Space = "World";
	int refID = 3;
> = {500.18f, -510.10f, -510.12f};

float3 SpotLightDir : DIRECTION <
	string UIName = "Spot Direction";
	string Object = "SpotLight";
	string Space = "World";
	int refID = 3;
> = {0.0f, -0.91f, -0.42f};

float SpotLightCone <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 90.5;
    float UIStep = 0.1;
    string UIName = "Spot Cone Angle";
> = 60.0;

float3 SpotLightColor : Specular <
	string UIName = "Spot Light Color";
	string UIWidget = "Color";
	int refID = 3;
> = {0.0f, 0.0f, 0.0f};

float SpotLightIntensity <
	string UIName = "Spot Intensity";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 2;
	float UIStep = 0.1;
> = 2;

//////////////////////////////////////////////
//////////////// Surface /////////////////////
//////////////////////////////////////////////

float AlphaBlending
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName = "Alpha Blending";
> = 1.0f;

float3 SurfaceColor : SPECULAR <
	string UIName = "Surface Color";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

float3 SurfaceSpecular : SPECULAR <
	string UIName = "Surface Specular Color";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

float3 SurfaceAmbient : SPECULAR <
	string UIName = "Surface Ambient Color";
	string UIWidget = "Color";
> = {0.1f, 0.1f, 0.1f};

float UScale
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 16.0;
    float UIStep = 1.0;
    string UIName = "U Texture Repeat";
> = 1.0;

float VScale
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 10.0;
    float UIStep = 1.0;
    string UIName = "V Texture Repeat";
> = 1.0;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture DiffuseTexture : DIFFUSE
<
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

sampler2D DiffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
	AddressU = MIRROR;
	AddressV = MIRROR;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct appdata
{
    float4 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};