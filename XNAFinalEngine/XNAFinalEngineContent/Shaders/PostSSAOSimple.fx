/******************************************************************************

    Based in the shader of Alex Urbano Alvarez
	License: Microsoft_Permissive_License
	Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

******************************************************************************/

#include <CommonDepthNormals.fxh>

//////////////////////////////////////////////
///////////////// Script /////////////////////
//////////////////////////////////////////////

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";

	string Script = "Technique=Technique?SSAOFX:SSAO;";
> = 0.8; // SAS version

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 Projection : PROJECTION <string UIWidget="None";>;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float sampleRadius <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 50;
    float UIStep = 0.1;
    string UIName = "Sample Radius";
> = 1.0;

float distanceScale <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 10000;
    float UIStep = 10;
    string UIName = "Distance Scale";
> = 150.0;

float AspectRatio
<
    string UIWidget = "slider";
    float UIMin = 1;
    float UIMax = 2;
    float UIStep = 0.1f;
    string UIName = "Aspect Ratio";
> = 1.3;

float3 cornerFustrum
<
    string UIWidget = "none";
>;

float2 g_InvResolution = {0.00097, 0.0013};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUTPUT
{
    float4 pos				: POSITION;
    float2 TexCoord			: TEXCOORD0;
    float3 viewDirection	: TEXCOORD1;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

float3 CornetFrustrum()
{

    float farY = tan(3.1416 / 3 / 2) * FarPlane; // cameraFOV = 3.1416 / 3
	
    float farX = farY * AspectRatio;

    return float3(farX, farY, FarPlane);
}

VS_OUTPUT VertexShaderFunction(float4 Position : POSITION,
                               float2 TexCoord : TEXCOORD0)
{
    VS_OUTPUT Out = (VS_OUTPUT)0;

    Out.pos = Position;
    Position.xy = sign(Position.xy);
    Out.TexCoord = (float2(Position.x, -Position.y) + float2( 1.0f, 1.0f ) ) * 0.5f;
    
	float3 corner = float3(-cornerFustrum.x * Position.x,
			                cornerFustrum.y * Position.y,
							cornerFustrum.z);
							
	Out.viewDirection =  corner;
    
    return Out;
}

VS_OUTPUT VertexShaderFunctionFX(float4 Position : POSITION,
                                 float2 TexCoord : TEXCOORD0)
{
    VS_OUTPUT Out = (VS_OUTPUT)0;

    Out.pos = Position;
    Position.xy = sign(Position.xy);
    Out.TexCoord = (float2(Position.x, -Position.y) + float2( 1.0f, 1.0f ) ) * 0.5f;
    
	float3 cornerFustrum = CornetFrustrum();
	
	float3 corner = float3(-cornerFustrum.x * Position.x,
			                cornerFustrum.y * Position.y,
							cornerFustrum.z);
							
	Out.viewDirection =  corner;
    
    return Out;
}

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture randomTexture : Diffuse
<
	string UIName = "Random Texture";
	string ResourceName = "RANDOMNORMAL.png";
>;

sampler2D RandNormal = sampler_state
{
	Texture = <randomTexture>;
    ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
};

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 PixelShaderFunction(VS_OUTPUT IN) : COLOR0
{
	float4 samples[16] =
	{
		float4(0.355512, 	-0.709318, 	-0.102371,	0.0 ),
		float4(0.534186, 	0.71511, 	-0.115167,	0.0 ),
		float4(-0.87866, 	0.157139, 	-0.115167,	0.0 ),
		float4(0.140679, 	-0.475516, 	-0.0639818,	0.0 ),
		float4(-0.0796121, 	0.158842, 	-0.677075,	0.0 ),
		float4(-0.0759516, 	-0.101676, 	-0.483625,	0.0 ),
		float4(0.12493, 	-0.0223423,	-0.483625,	0.0 ),
		float4(-0.0720074, 	0.243395, 	-0.967251,	0.0 ),
		float4(-0.207641, 	0.414286, 	0.187755,	0.0 ),
		float4(-0.277332, 	-0.371262, 	0.187755,	0.0 ),
		float4(0.63864, 	-0.114214, 	0.262857,	0.0 ),
		float4(-0.184051, 	0.622119, 	0.262857,	0.0 ),
		float4(0.110007, 	-0.219486, 	0.435574,	0.0 ),
		float4(0.235085, 	0.314707, 	0.696918,	0.0 ),
		float4(-0.290012, 	0.0518654, 	0.522688,	0.0 ),
		float4(0.0975089, 	-0.329594, 	0.609803,	0.0 )
	};
	
	IN.TexCoord.x += 0.5/1024.0; 
	IN.TexCoord.y += 0.5/768.0;

	normalize (IN.viewDirection);
	float depth = tex2D(highPresicionDepthSampler, IN.TexCoord).r;
	float3 se = depth * IN.viewDirection;
	
	float3 randNormal = tex2D(RandNormal, IN.TexCoord * 200.0).rgb;

	float3 normal = tex2D(depthNormalSampler, IN.TexCoord).rgb;
	float finalColor = 0.0f;
	
	for (int i = 0; i < 12; i++)
	{
		float3 ray = reflect(samples[i].xyz,randNormal) * sampleRadius;
		
		//if (dot(ray, normal) < 0)
		//	ray += normal * sampleRadius;
			
		float4 sample = float4(se + ray, 1.0f);
		float4 ss = mul(sample, Projection);

		float2 sampleTexCoord = 0.5f * ss.xy/ss.w + float2(0.5f, 0.5f);
		
		sampleTexCoord.x += g_InvResolution.x;
		sampleTexCoord.y += g_InvResolution.y;
		float sampleDepth = tex2D(highPresicionDepthSampler, sampleTexCoord).r;
		
		if (sampleDepth == 1.0)
		{
			finalColor ++;
		}
		else
		{		
			float occlusion = distanceScale * max(sampleDepth - depth, 0.0f);
			finalColor += 1.0f / (1.0f + occlusion * occlusion * 0.1);
		}
	}

	return float4(finalColor / 16, finalColor / 16, finalColor / 16, 1.0f);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

const float4 ClearColorDepth : DIFFUSE = { 1.0f, 0.0f, 0.0f, 1.0f};
const float4 ClearColor : DIFFUSE = { 1.0f, 0.0f, 0.0f, 1.0f};
const float ClearDepth = 1.0f;

technique SSAOFX
<
	string Script =
		"Pass=Normals;"
		"Pass=Depth;"
		"Pass=SSAO;";
>
{
pass Normals
	<
		string Script = "RenderColorTarget0=depthNormalTexture;"
						"ClearSetColor=ClearColorDepth;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
					    "Draw=geometry;";
	>
    {          
        VertexShader = compile vs_3_0 DepthNormalsVertexShaderFunction();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
#else
        CullMode = CCW;    // For The Engine	
#endif	
        PixelShader  = compile ps_3_0 DepthNormalsPixelShaderFunction();
    }
	pass Depth
	<
		string Script = "RenderColorTarget0=highPresicionDepthTexture;"
						"ClearSetColor=ClearDepth;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
					    "Draw=geometry;";
	>
    {          
        VertexShader = compile vs_3_0 DepthNormalsVertexShaderFunction();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
#else
        CullMode = CCW;    // For The Engine	
#endif
        PixelShader  = compile ps_3_0 HighPresicionDepthPixelShaderFunction();
    }
    pass SSAO
	<
		string Script = "RenderColorTarget0=;"
						"ClearSetColor=ClearColor;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
			            "Draw=Buffer;"; 
	>
    {          
        VertexShader = compile vs_3_0 VertexShaderFunctionFX();
        PixelShader  = compile ps_3_0 PixelShaderFunction();
    }
}

technique SSAO
{
    pass P0
    {          
        VertexShader = compile vs_3_0 VertexShaderFunction();
        ZEnable = true;
		ZWriteEnable = true;
		CullMode = CCW; 
		AlphaBlendEnable = false;
        PixelShader  = compile ps_3_0 PixelShaderFunction();
    }
}