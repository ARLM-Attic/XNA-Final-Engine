/******************************************************************************

    Based in the shader of Ben Cloward. 
    No license.
	Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

It includes some features to imitate the look of tricked-out car paint. Some car
paint appears to change color based on your viewing angle. The paint looks like
one color when you're looking straight at the surface and another color when
the surface is parallel to your view. This shader imitates that effect.

To achieve the effect, the first thing we need is a color gradient. The shader
includes two colors. Surfaces on the model that are facing you will receive
the "Middle Color" while surfaces at glancing angles will receive the "Edge
Color." 

The amount of each color in the gradient is determined by the "Fresnel Power"
and "Fresnel Bias" values. These will allow you to tweek how much of each color
gets applied to the effect.

Finally, the two colors are subtracted from the per-pixel lighting result.
Because the colors are subtracted, you'll end up with the opposite colors from
the ones you selected and they'll darken the final result.

The fresnel term in this shader is a very useful shader component. Most objects
that are reflective are more reflective at glancing angles than straight on and
the fresnel term can be used to achieve that effect. It can also be used to
blend between reflection and refraction in water.
	
******************************************************************************/

#include <Common.fxh> // Common illumination code.

//#define FX_COMPOSER  // When we are working in FX Composer


//////////////////////////////////////////////
///////////////// Script /////////////////////
//////////////////////////////////////////////

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?CarPaint2PointLight:CarPaint1PointLight;";
> = 0.8;

//////////////////////////////////////////////
//////////////// Surface /////////////////////
//////////////////////////////////////////////

float3 specularColor : Specular
<
    string UIName = "Specular Color";
> = { 1.0f, 1.0f, 1.0f };

float shininess <
	string UIWidget = "slider";
    float UIMin = 1;
    float UIMax = 128;
    string UIName = "Shininess";
> = 0;

float3 paintedge : Diffuse
<
    string UIName = "Edge Color";
> = { 0.0f, 0.0, 0.0f};

float3 paintmiddle : Diffuse
<
    string UIName = "Middle Color";
> = { 0.0f, 0.0f, 0.0f };

float fresnelbias <
	string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 2.0f;
    string UIName = "Fresnel Bias";
> = 0.02f;

float fresnelpower <
	string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 10.0f;
    string UIName = "Fresnel Power";
> = 10.0f;

float Kd <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Diffuse (from dirt)";
> = 0.0;

float reflection <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Reflection";
> = 0.7;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture CubeEnvMap : ENVIRONMENT <
	string ResourceName = "default_reflection.dds";
	string ResourceType = "Cube";
>;

samplerCUBE EnvSampler = sampler_state {
	Texture = <CubeEnvMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

// input from application
struct a2v {
	float4 position		: POSITION;
	float2 texCoord		: TEXCOORD0;
	float3 normal		: NORMAL;
};

// output to fragment program
struct v2f {
        float4 position    	: POSITION;
		float2 texCoord    	: TEXCOORD0;
		float3 eyeVec		: TEXCOORD1;
		float3 lightVec   	: TEXCOORD2;
		float3 lightVec2   	: TEXCOORD3;
		float3 lightVec3   	: TEXCOORD4;
		float3 worldNormal	: TEXCOORD5;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

v2f VertexShaderFunctionThreeLights(a2v In)
{
	v2f Out;
	Out.position = mul(In.position, WorldViewProj);
    Out.worldNormal = mul(In.normal, WorldIT).xyz;
    
	float3 Pw = mul(In.position, World);
	
	Out.lightVec  = normalize(PointLightPos  - Pw);
	Out.lightVec2 = normalize(PointLightPos2 - Pw);
	Out.lightVec3 = normalize(PointLightPos3 - Pw);
	
	Out.eyeVec = ViewI[3].xyz - Pw;
	
	Out.texCoord.xy = In.texCoord;
	
	return Out;
} // VertexShaderFunction

v2f VertexShaderFunctionOneLight(a2v In)
{
	v2f Out;
	Out.position = mul(In.position, WorldViewProj);
    Out.worldNormal = mul(In.normal, WorldIT).xyz;
    
	float3 worldSpacePos = mul(In.position, World);
    Out.lightVec = normalize(PointLightPos - worldSpacePos);
	Out.lightVec2 = 0; // Es para evitar el warning
	Out.lightVec3 = 0; // Es para evitar el warning
	Out.eyeVec = ViewI[3].xyz - worldSpacePos;
	
	Out.texCoord.xy = In.texCoord;
        
    return Out;
} // VertexShaderFunction

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

// Blinn lighting with lit function
float3 Blinn(float3 N,
		    float3 L,
		    float3 V,
		    uniform float3 diffuseColor,
		    uniform float3 specularColor,
		    uniform float shininess)
{	
	float3  H        = normalize(V + L);
	float3  lighting = lit(dot(L, N), dot(H, N), shininess);
	return diffuseColor * lighting.y + specularColor * lighting.z;
} // Blinn

void metal_refl_shared(float3 N, float3 V,
		               out float3 ReflectionContrib)
{
 	float3 reflVect    = -reflect(V, N);
    ReflectionContrib = reflection * texCUBE(EnvSampler, reflVect).xyz;
} // metal refl shared

float4 PixelShaderFunctionThreeLights(v2f In) : COLOR
{
	float3 N  = normalize(In.worldNormal);
  	float3 V  = normalize(In.eyeVec);
  	float3 L  = normalize(In.lightVec.xyz);
  	float3 L2 = normalize(In.lightVec2.xyz);
	float3 L3 = normalize(In.lightVec3.xyz);
  
    ///// lighting  ////
  
    // Ambient
    float3 Color = AmbientLightColor * SurfaceColor;
   
    // Reflections
	
    float3 reflColor;
    metal_refl_shared(N, V, reflColor);
  
    // Diffuse and specular
    Color += Blinn(N, L,  V, PointLightColor  * SurfaceColor, reflColor * specularColor, shininess);
    Color += Blinn(N, L2, V, PointLightColor2 * SurfaceColor, reflColor * specularColor, shininess);
	Color += Blinn(N, L3, V, PointLightColor3 * SurfaceColor, reflColor * specularColor, shininess);

    // Calculate car paint color
    float NdV     = dot(N, V);
    float fresnel = fresnelbias + (1.0-fresnelbias) * pow(1.0 - max(NdV, 0), fresnelpower);
    float3 paint  = lerp(paintmiddle, paintedge, fresnel);
  
    // Subtract car paint color from total
    Color -= paint;

    return float4(Color, AlphaBlending);
}

float4 PixelShaderFunctionOneLight(v2f In) : COLOR
{
	float3 N  = normalize(In.worldNormal);
  	float3 V  = normalize(In.eyeVec);
  	float3 L  = normalize(In.lightVec.xyz);
  	float3 L2 = normalize(In.lightVec2.xyz);
	float3 L3 = normalize(In.lightVec3.xyz);
  
    ///// lighting  ////
  
    // Ambient
    float3 Color = AmbientLightColor * SurfaceColor;
   
	// Reflections
	
    float3 reflColor;
    metal_refl_shared(N, V, reflColor);
  
    // Diffuse and specular
    Color += Blinn(N, L,  V, PointLightColor  * SurfaceColor, reflColor * specularColor, shininess);

    // Calculate car paint color
    float NdV     = dot(N, V);
    float fresnel = fresnelbias + (1.0-fresnelbias) * pow(1.0 - max(NdV, 0), fresnelpower);
    float3 paint  = lerp(paintmiddle, paintedge, fresnel);
  
    // Subtract car paint color from total
    Color -= paint;

    return float4(Color, AlphaBlending);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique CarPaint3PointLight
{ 
    pass envPass 
    {		
		VertexShader = compile vs_3_0 VertexShaderFunctionThreeLights();
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
#else
        CullMode = CCW;    // For The Engine	
#endif	
		PixelShader = compile ps_3_0 PixelShaderFunctionThreeLights();
    }
}

technique CarPaint1PointLight
{ 
    pass envPass 
    {		
		VertexShader = compile vs_3_0 VertexShaderFunctionOneLight();
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
#else
        CullMode = CCW;    // For The Engine	
#endif
		PixelShader = compile ps_3_0 PixelShaderFunctionOneLight();
    }
}