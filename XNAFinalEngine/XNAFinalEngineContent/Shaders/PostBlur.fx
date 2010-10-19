/******************************************************************************

    Based in the shader from RacingGame
	License: Microsoft_Permissive_License
	Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

******************************************************************************/
string description = "Post screen shader for shadow blurring";

//////////////////////////////////////////////
///////////////// Script /////////////////////
//////////////////////////////////////////////
float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";

	string Script = "Technique=Blur;";
> = 1.0;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 windowSize : VIEWPORTPIXELSIZE;

float BlurWidth <
	string UIName = "Blur Width";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 5;
	float UIStep = 0.05;
> = 1.0f;

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

texture sceneMap : RENDERCOLORTARGET
<
    float2 ViewportRatio = { 1.0, 1.0 };
    int MIPLEVELS = 1;
>;
sampler sceneMapSampler = sampler_state 
{
    texture = <sceneMap>;
    AddressU  = Clamp;
    AddressV  = Clamp;
    AddressW  = Clamp;
    MIPFILTER = None;
    MINFILTER = Linear;
    MAGFILTER = Linear;
};

texture blurMap : RENDERCOLORTARGET
<
    float2 ViewportRatio = { 1.0, 1.0 };
    int MIPLEVELS = 1;
>;
sampler blurMapSampler = sampler_state 
{
    texture = <blurMap>;
    AddressU  = Clamp;
    AddressV  = Clamp;
    AddressW  = Clamp;
    MIPFILTER = None;
    MINFILTER = Linear;
    MAGFILTER = Linear;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_Output
{
   	float4 pos         : POSITION;
    float2 texCoord[8] : TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_Output VS_Blur(float4 pos      : POSITION, 
	              float2 texCoord : TEXCOORD0,
	              uniform float2 dir)
{
	VS_Output Out = (VS_Output)0;
	
	Out.pos = pos;
	float2 texelSize = 1.0 / windowSize;
	float2 s = texCoord - texelSize*(8-1) *0.5 * dir * BlurWidth + texelSize * 0.5;
	for(int i = 0; i < 8; i++)
	{
		Out.texCoord[i] = s + texelSize * i * dir * BlurWidth;
	}
	return Out;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 PS_Blur(VS_Output In, uniform sampler2D tex) : COLOR
{
	
	float4 ret = 0;
	// This loop will be unrolled by the compiler
	for (int i = 0; i < 8; i++)
	{
		float4 col  = tex2D(tex, In.texCoord[i]);
		       ret += col * Weights8[i];
	}
	return ret;
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

const float4 ClearColor : DIFFUSE = { 0.0f, 0.0f, 0.0f, 1.0f};
const float ClearDepth = 1.0f;

technique Blur
<
	// Script stuff is just for FX Composer
	string Script =
		"ClearSetDepth=ClearDepth;"
		"RenderColorTarget=sceneMap;"
		"ClearSetColor=ClearColor;"
		"ClearSetDepth=ClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptSignature=color;"
		"ScriptExternal=;"
		"Pass=BlurHorizontal;"
		"Pass=BlurVertical;";
>
{
	pass BlurHorizontal
	<
		string Script =
			"RenderColorTarget=blurMap;"
			"RenderDepthStencilTarget=;"
			"ClearSetColor=ClearColor;"
			"ClearSetDepth=ClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"Draw=Buffer;";
	>
	{
		VertexShader = compile vs_3_0 VS_Blur(float2(1, 0));
		CullMode = CCW; 
		PixelShader  = compile ps_3_0 PS_Blur(sceneMapSampler);
	}

	pass BlurVertical
	<
		string Script =
			"RenderColorTarget=;"
			"RenderDepthStencilTarget=;"
			"ClearSetColor=ClearColor;"
			"ClearSetDepth=ClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"Draw=Buffer;";
	>
	{
		VertexShader = compile vs_3_0 VS_Blur(float2(0, 1));
		CullMode = CCW; 
		PixelShader  = compile ps_3_0 PS_Blur(blurMapSampler);
	}
}
