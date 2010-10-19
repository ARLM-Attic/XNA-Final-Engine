/******************************************************************************

    Based in the shader from RacingGame
	License: Microsoft_Permissive_License
	Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

******************************************************************************/

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
    string Script = "Technique=Technique?ParallaxMapping;";
> = 0.8;

//////////////////////////////////////////////
//////////////// Surface /////////////////////
//////////////////////////////////////////////

float4 specularColor : Specular
<
	string UIName = "Specular Color";
	string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float shininess : SpecularPower
<
	string UIName = "Specular Power";
	string UIWidget = "slider";
	float UIMin = 1.0;
	float UIMax = 128.0;
	float UIStep = 1.0;
> = 16.0;

float parallaxAmount
<
	string UIName = "Parallax amount";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.0001;
> = 0.033f;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture normalTexture : Diffuse
<
	string UIName = "Normal Texture";
	string ResourceName = "NormalMap.bmp";
>;
sampler normalSampler = sampler_state
{
	Texture = <normalTexture>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
	AddressU = MIRROR;
	AddressV = MIRROR;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VertexInput
{
	float3 pos      : POSITION;
	float2 texCoord : TEXCOORD0;
	float3 normal   : NORMAL;
	float3 tangent	: TANGENT;
};

struct VertexOutput
{
	float4 pos            : POSITION;
	float2 texCoord       : TEXCOORD0;
	float3 lightVec       : TEXCOORD1;
	float3 viewVec        : TEXCOORD2;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

float3 CalcNormalVector(float3 nor)
{
	return normalize(mul(nor, (float3x3)World));
} // CalcNormalVector

float3x3 ComputeTangentMatrix(float3 tangent, float3 normal)
{
	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace;
	// For FX Composer use this format:
	worldToTangentSpace[0] = mul(tangent, World);
	worldToTangentSpace[1] = mul(cross(tangent, normal), World);
	worldToTangentSpace[2] = mul(normal, World);
	
	return worldToTangentSpace;
} // ComputeTangentMatrix

VertexOutput VS(VertexInput In)
{
	VertexOutput Out = (VertexOutput) 0;

	float4 PosWorld = mul(float4(In.pos.xyz, 1.0), World);
	Out.pos = mul(PosWorld, ViewProj);
	
	// Copy texture coordinates for diffuse and normal maps
	Out.texCoord = In.texCoord;

	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace = ComputeTangentMatrix(In.tangent, In.normal);

	// Transform light vector and pass it as a color
	Out.lightVec = normalize(mul(worldToTangentSpace, -DirectionalLightDir));
	Out.viewVec = mul(worldToTangentSpace, ViewI[3].xyz - PosWorld);

	return Out;
} // VS

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 PS(VertexOutput In) : COLOR
{
	// Get height from normal map alpha channel!
	float height = tex2D(normalSampler, In.texCoord).a;
	
	// Calculate parallax offset
	float3 viewVector = normalize(In.viewVec);
	
	float2 offsetTexCoord = In.texCoord +
		// Push stuff more in than pulling it out, this minimized the disortion effect.
		(height*parallaxAmount - parallaxAmount*0.6f)*viewVector;
		//(height-1)*parallaxAmount*viewVector;

	// Grab texture data
	float4 diffuseTexture = float4(tex2D(DiffuseSampler, offsetTexCoord).rgb, 1);
	
	// Mode 1
	
	float3 normalVector = (2.0 * tex2D(normalSampler, offsetTexCoord).rgb) - 1.0;
	normalVector = normalize(normalVector);
	/*
	// Mode 2
	float3 normalVector = float3(tex2D(normalSampler, offsetTexCoord).g*2-1, tex2D(normalSampler, offsetTexCoord).a*2-1,0);
    normalVector.z = sqrt( 1 - normalVector.x * normalVector.x + normalVector.y * normalVector.y);
    */
	// Additionally normalize the vectors
	float3 lightVector = normalize(In.lightVec);
	// Compute the angle to the light
	float bump = dot(normalVector, lightVector);
		
	// Specular factor
	float3 reflect = normalize(2 * bump * normalVector - lightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);

	float3 ambDiffColor = AmbientLightColor * diffuseTexture;
	
	float4 ret;
	ret.rgb = diffuseTexture * bump + ambDiffColor +
		// Also multiply by height, lower stuff should be more occluded and not have so much shininess
		(height.x + 0.5f) * bump * spec * specularColor;
	
	ret.a = AlphaBlending;
	
	return ret;
} // PS

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique ParallaxMapping
{
	pass P0
	{
		VertexShader = compile vs_3_0 VS();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
#else
        CullMode = CCW;    // For The Engine	
#endif
		AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;
		PixelShader  = compile ps_3_0 PS();
	}
}
