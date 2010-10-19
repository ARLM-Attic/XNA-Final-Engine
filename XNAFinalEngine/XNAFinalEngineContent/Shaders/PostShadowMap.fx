/******************************************************************************

    Based in the shader from RacingGame
	License: Microsoft_Permissive_License
	Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

******************************************************************************/

//#define FX_COMPOSER

//////////////////////////////////////////////
///////////////// Script /////////////////////
//////////////////////////////////////////////

float Script : STANDARDSGLOBAL
<
	string description = "Generate and use a shadow map with a directional light";

	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";

	string Script = "Technique=Technique?ShadowMapFX;";
> = 0.8; // SAS version

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 worldViewProj         : WorldViewProjection <string UIWidget="None";>;

//////////////////////////////////////////////
///////////////// Lights /////////////////////
//////////////////////////////////////////////

// Extra values for this shader
// Transformation matrix for converting world pos to texture coordinates of the shadow map.
float4x4 shadowTexTransform;

// worldViewProj of the light projection
float4x4 worldViewProjLight;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float farPlane
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 500;
    float UIStep = 1;
    string UIName = "Far Plane";
> = 100.0;

float2 shadowMapTexelSize = float2(1.0f/1024.0f, 1.0f/1024);

// Depth bias, controls how much we remove from the depth
// to fix depth checking artifacts.
float depthBias
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.1;
    float UIStep = 0.0005;
    string UIName = "Depth Bias";
> = 0.0025f;

// Substract a very low value from shadow map depth to
// move everything a little closer to the camera.
// This is done when the shadow map is rendered before any
// of the depth checking happens, should be a very small value.
float shadowMapDepthBias 
<
    string UIWidget = "slider";
    float UIMin = -0.1;
    float UIMax = 0.1;
    float UIStep = 0.0005;
    string UIName = "Shadow Map Depth Bias";
> = 0.0017f;
	
float4 shadowColor : SPECULAR <
	string UIName = "Shadow Color";
	string UIWidget = "Color";
> = {0.45f, 0.45f, 0.45f, 1};

// Poison filter pseudo random filter positions for PCF with 10 samples
float2 FilterTaps[10] =
{
	// First test, still the best.
	{-0.84052f, -0.073954f},
	{-0.326235f, -0.40583f},
	{-0.698464f, 0.457259f},
	{-0.203356f, 0.6205847f},
	{0.96345f, -0.194353f},
	{0.473434f, -0.480026f},
	{0.519454f, 0.767034f},
	{0.185461f, -0.8945231f},
	{0.507351f, 0.064963f},
	{-0.321932f, 0.5954349f}
};

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture shadowDistanceFadeoutTexture : DIFFUSE
<
	string UIName = "Shadow distance fadeout texture";
	string ResourceName = "ShadowDistanceFadeoutMap.dds";
>;
sampler shadowDistanceFadeoutTextureSampler = sampler_state
{
	Texture = <shadowDistanceFadeoutTexture>;
	AddressU  = Wrap;
	AddressV  = Wrap;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture shadowMap : RENDERCOLORTARGET
< 
    float2 ViewportRatio = { 1, 1 };
	string Format="R32F";
    int MIPLEVELS = 1;
>;

sampler ShadowMapSampler = sampler_state
{
	Texture = <shadowMap>;
	AddressU  = CLAMP;
	AddressV  = CLAMP;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = None;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
	// We just use the position here, nothing else is required.
	float4 pos      : POSITION;
};

// Struct used for passing data from VS_GenerateShadowMap to ps
struct VB_GenerateShadowMap
{
	float4 pos       : POSITION;
	float2 depth : TEXCOORD0;
};

// Vertex shader output structure for using the shadow map
struct VB_UseShadowMap
{
	float4 pos            : POSITION;
	float4 shadowTexCoord : TEXCOORD0;
	float2 depth          : TEXCOORD1;
};

//////////////////////////////////////////////
/////////// Generate ShadowMap ///////////////
//////////////////////////////////////////////

// Vertex shader function
VB_GenerateShadowMap VS_GenerateShadowMap(VertexInput In)
{
	VB_GenerateShadowMap Out = (VB_GenerateShadowMap) 0;
	Out.pos = mul(In.pos, worldViewProj);
	
	// Linear depth calculation instead of normal depth calculation.
	Out.depth = float2(Out.pos.z, farPlane);

	return Out;
} // VS_GenerateShadowMap

// Pixel shader function
float4 PS_GenerateShadowMap(VB_GenerateShadowMap In) : COLOR
{
	// Just set the interpolated depth value.
	return float4((In.depth.x / In.depth.y) + shadowMapDepthBias, 1, 1, 1);
} // PS_GenerateShadowMap

technique GenerateShadowMap
{
	pass P0
	{
		VertexShader = compile vs_3_0 VS_GenerateShadowMap();
		// Disable culling to throw shadow even if virtual
		// shadow light is inside big buildings!
		CullMode = None;
		//CullMode = CCW;
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		PixelShader  = compile ps_3_0 PS_GenerateShadowMap();
	} // pass P0
} // GenerateShadowMap

//////////////////////////////////////////////
/////////////// Use ShadowMap ////////////////
//////////////////////////////////////////////

VB_UseShadowMap VS_UseShadowMap(VertexInput In)
{
	VB_UseShadowMap Out = (VB_UseShadowMap)0;
	
	// Convert to float4 pos, used several times here.
	Out.pos = mul(In.pos, worldViewProj);

	// Transform model-space vertex position to light-space:
	float4 shadowTexPos = mul(In.pos, shadowTexTransform);
	// Set first texture coordinates
	Out.shadowTexCoord = float4(shadowTexPos.x, shadowTexPos.y, 0.0f, shadowTexPos.w);

	// Get depth of this point relative to the light position
	float4 depthPos = mul(In.pos, worldViewProjLight);
	
	// Same linear depth calculation as above.
	// Also substract depthBias to fix shadow mapping artifacts.
	Out.depth = float2((depthPos.z), (farPlane));

	return Out;
} // VS_UseShadowMap

// Pixel shader for shadow depth calculations.
// However this shader looks blocky like PCF3x3 and should be smoothend
// out by a good post screen blur filter. This shader does a good
// job faking the penumbra and can look very good when adjusted carefully.
float4 PS_UseShadowMap(VB_UseShadowMap In) : COLOR
{
	float depth = (In.depth.x / In.depth.y) - depthBias;

	float2 shadowTex = (In.shadowTexCoord.xy / In.shadowTexCoord.w) - shadowMapTexelSize / 2.0f;

	float resultDepth = 0;
	for (int i = 0; i < 10; i++)
		resultDepth += depth > tex2D(ShadowMapSampler, shadowTex + FilterTaps[i]*shadowMapTexelSize).r ? 1.0f / 10.0f : 0.0f;
			
	// Multiply the result by the shadowDistanceFadeoutTexture, which fades shadows in and out at the max. shadow distances
	resultDepth *= tex2D(shadowDistanceFadeoutTextureSampler, shadowTex).r;

	// We can skip this if its too far away anway
	if (depth > 1)
		return float4(1,1,1,1);
	else
		// And apply
		return lerp(1, shadowColor, resultDepth);
} // PS_UseShadowMap

technique UseShadowMap
{  
	pass P0
	{
		VertexShader = compile vs_3_0 VS_UseShadowMap();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = CCW;
		AlphaBlendEnable = false;
		PixelShader  = compile ps_3_0 PS_UseShadowMap();
	} // P0
} // UseShadowMap

//////////////////////////////////////////////
/////////////// FX Technique /////////////////
//////////////////////////////////////////////

const float4 ClearColorDepth : DIFFUSE = { 1.0f, 0.0f, 0.0f, 1.0f};
const float4 ClearColor : DIFFUSE = { 1.0f, 0.0f, 0.0f, 1.0f};
const float ClearDepth = 1.0f;

technique ShadowMapFX
<
	string Script =
		"Pass=GenerateShadowMap;"
		"Pass=UseShadowMap;";
>
{
	pass GenerateShadowMap
	<
		string Script = "RenderColorTarget0=shadowMap;"
						"ClearSetColor=ClearDepth;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
					    "Draw=geometry;";
	>
    {          
        VertexShader = compile vs_3_0 VS_GenerateShadowMap();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
#else
        CullMode = CCW;    // For The Engine	
#endif
        PixelShader  = compile ps_3_0 PS_GenerateShadowMap();
    }
    pass UseShadowMap
	<
		string Script = "RenderColorTarget0=;"
						"ClearSetColor=ClearColor;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
			            "Draw=Buffer;"; 
	>
    {          
        VertexShader = compile vs_3_0 VS_UseShadowMap();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = CCW; 
		AlphaBlendEnable = false;
        PixelShader  = compile ps_3_0 PS_UseShadowMap();
    }
}
