/***********************************************************************************************************************************************

Based in the DirectX 10 shader of NVIDIA.
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

#define M_PI 3.14159265f
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
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float AspectRatio <
    string UIWidget = "slider";
    float UIMin = 1;
    float UIMax = 2;
    float UIStep = 0.1f;
    string UIName = "Aspect Ratio";
> = 1.3;

float  g_NumSteps <
    string UIWidget = "slider";
    float UIMin = 1;
    float UIMax = 32;
    float UIStep = 1;
    string UIName = "Numbers of Steps";
> = 8;

float  g_NumDir <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 25;
    float UIStep = 1;
    string UIName = "Numbers of Directions";
> = 10;

float  g_R <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 2;
    float UIStep = 0.05;
    string UIName = "Radius";
> = 0.2;

float  g_AngleBias <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 3.1416;
    float UIStep = 0.05;
    string UIName = "Angle Bias";
> = 0.5;

float  g_Attenuation <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 2;
    float UIStep = 0.05;
    string UIName = "Line Attenuation";
> = 1;

float  g_Contrast <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 3;
    float UIStep = 0.1;
    string UIName = "Contrast";
> = 1.25;

float2 g_FocalLen <
	string UIWidget = "slider";
> = float2(2.77, 1.73);

float2 g_Resolution = {1024, 768};

float2 g_InvResolution = {0.00097, 0.0013};

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
	MAGFILTER = POINT;
	MINFILTER = POINT;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUTPUT
{	
	float4 pos   : POSITION;
    float2 tex   : TEXCOORD0;
    float2 texUV : TEXCOORD1;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_OUTPUT VertexShaderFunction( float4 Position : POSITION )
{
	VS_OUTPUT Out = (VS_OUTPUT)0;

	Out.tex = Position.xy / g_FocalLen;
	Out.pos = Position;
	
    Out.texUV = (float2(Position.x, -Position.y) + float2( 1.0f, 1.0f ) ) * 0.5f;
		
	return Out;
} // VertexShaderFunction

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

//----------------------------------------------------------------------------------
float tangent(float3 P, float3 S)
{
    return (P.z - S.z) / length(S.xy - P.xy);
}

//----------------------------------------------------------------------------------
float3 uv_to_eye(float2 uv, float eye_z)
{
    uv = (uv * float2(2.0, -2.0) - float2(1.0, -1.0));
    return float3(uv * (1 / g_FocalLen) * eye_z, eye_z);
}

//----------------------------------------------------------------------------------
float3 fetch_eye_pos(float2 uv)
{
	float z = 1 - tex2Dlod(highPresicionDepthSampler, float4(uv.x, uv.y, 0, 1)).r; // Single channel zbuffer texture
    //float z = 1 - tex2D(depthNormalSampler, float4(uv.x, uv.y, 0, 1)).a; // rgb zbuffer texture
    return uv_to_eye(uv, z);
}

//----------------------------------------------------------------------------------
float3 tangent_eye_pos(float2 uv, float4 tangentPlane)
{
    // view vector going through the surface point at uv
    float3 V = fetch_eye_pos(uv);
    float NdotV = dot(tangentPlane.xyz, V);
    // intersect with tangent plane except for silhouette edges
    if (NdotV < 0.0) V *= (tangentPlane.w / NdotV);
    return V;
}

float length2(float3 v) { return dot(v, v); } 

//----------------------------------------------------------------------------------
float3 min_diff(float3 P, float3 Pr, float3 Pl)
{
    float3 V1 = Pr - P;
    float3 V2 = P - Pl;
    return (length2(V1) < length2(V2)) ? V1 : V2;
}

//----------------------------------------------------------------------------------
float falloff(float r)
{
    return 1.0f - g_Attenuation*r*r;
}

//----------------------------------------------------------------------------------
float2 snap_uv_offset(float2 uv)
{
    return round(uv * g_Resolution) * (1/ g_Resolution);
}

//----------------------------------------------------------------------------------
float tan_to_sin(float x)
{
    return x / sqrt(1.0f + x*x);
}

//----------------------------------------------------------------------------------
float tangent(float3 T)
{
    return -T.z / length(T.xy);
}

//----------------------------------------------------------------------------------
void integrate_direction(inout float ao, float3 P, float2 uv, float2 deltaUV,
                         float numSteps, float tanH, float sinH)
{
    for (float j = 1; j <= 10/*numSteps*/; ++j) {
        uv += deltaUV;
        float3 S = fetch_eye_pos(uv);
        
        // Ignore any samples outside the radius of influence
        float d2  = length2(S - P);
        if (d2 < g_R * g_R) {
            float tanS = tangent(P, S);

            //[branch]
            if(tanS > tanH) {
                // Accumulate AO between the horizon and the sample
                float sinS = tanS / sqrt(1.0f + tanS*tanS);
                float r = sqrt(d2) * (1 / g_R);
                ao += falloff(r) * (sinS - sinH);
                
                // Update the current horizon angle
                tanH = tanS;
                sinH = sinS;
            }
        }
    }
}

//----------------------------------------------------------------------------------
float AccumulatedHorizonOcclusion_LowQuality(float2 deltaUV, 
                                             float2 uv0, 
                                             float3 P, 
                                             float numSteps, 
                                             float randstep)
{
    // Randomize starting point within the first sample distance
    float2 uv = uv0 + snap_uv_offset( randstep * deltaUV );
    
    // Snap increments to pixels to avoid disparities between xy 
    // and z sample locations and sample along a line
    deltaUV = snap_uv_offset( deltaUV );

    float tanT = tan(-M_PI*0.5 + g_AngleBias);
    //float sinT = (AngleBias != 0.0) ? tan_to_sin(tanT) : -1.0;
	float sinT = tan_to_sin(tanT);

    float ao = 0;
    integrate_direction(ao, P, uv, deltaUV, numSteps, tanT, sinT);

    // Integrate opposite directions together
    deltaUV = -deltaUV;
    uv = uv0 + snap_uv_offset( randstep * deltaUV );
    integrate_direction(ao, P, uv, deltaUV, numSteps, tanT, sinT);

    // Divide by 2 because we have integrated 2 directions together
    // Subtract 1 and clamp to remove the part below the surface
    return max(ao * 0.5 - 1.0, 0.0);
}


//----------------------------------------------------------------------------------
float2 rotate_direction(float2 Dir, float2 CosSin)
{
    return float2(Dir.x*CosSin.x - Dir.y*CosSin.y, 
                  Dir.x*CosSin.y + Dir.y*CosSin.x);
}

//----------------------------------------------------------------------------------
float4 PixelShaderFunction(uniform bool useNormal, VS_OUTPUT IN) : COLOR0
{
    float3 P = fetch_eye_pos(IN.texUV);

    // Project the radius of influence g_R from eye space to texture space.
    // The scaling by 0.5 is to go from [-1,1] to [0,1].
    float2 step_size = 0.5 * g_R  * g_FocalLen / P.z;

    // Early out if the projected radius is smaller than 1 pixel.
    float numSteps = min ( g_NumSteps, min(step_size.x * g_Resolution.x, step_size.y * g_Resolution.y));
   
	step_size = step_size / ( numSteps + 1 );

    // Nearest neighbor pixels on the tangent plane
    float3 Pr, Pl, Pt, Pb;
    float4 tangentPlane;
    if (useNormal) {
        float3 N = normalize(tex2D(depthNormalSampler, IN.texUV).rgb);
        tangentPlane = float4(N, dot(P, N));
        Pr = tangent_eye_pos(IN.texUV + float2(g_InvResolution.x, 0), tangentPlane);
        Pl = tangent_eye_pos(IN.texUV + float2(-g_InvResolution.x, 0), tangentPlane);
        Pt = tangent_eye_pos(IN.texUV + float2(0, g_InvResolution.y), tangentPlane);
        Pb = tangent_eye_pos(IN.texUV + float2(0, -g_InvResolution.y), tangentPlane);
    } else {
        Pr = fetch_eye_pos(IN.texUV + float2(g_InvResolution.x, 0));
        Pl = fetch_eye_pos(IN.texUV + float2(-g_InvResolution.x, 0));
        Pt = fetch_eye_pos(IN.texUV + float2(0, g_InvResolution.y));
        Pb = fetch_eye_pos(IN.texUV + float2(0, -g_InvResolution.y));
        float3 N = normalize(cross(Pr - Pl, Pt - Pb));
        tangentPlane = float4(N, dot(P, N));
    }
    
    // Screen-aligned basis for the tangent plane
    float3 dPdu = min_diff(P, Pr, Pl);
    float3 dPdv = min_diff(P, Pt, Pb) * (g_Resolution.y * g_InvResolution.x);

    // (cos(alpha),sin(alpha),jitter)
    float3 rand = tex2D( RandNormal, IN.texUV * 200).rgb;

    float ao = 0;
    float d;
    float alpha = 2.0f * M_PI / g_NumDir;

    for (d = 0; d < 7/*g_NumDir*/; ++d) {
		float angle = alpha * d;
		float2 dir = float2(cos(angle), sin(angle));
		float2 deltaUV = rotate_direction(dir, rand.xy) * step_size.xy;
		ao += AccumulatedHorizonOcclusion_LowQuality(deltaUV, IN.texUV, P, numSteps, rand.z);
	}
	ao *= 2.0;
	
	float contrast = g_Contrast / (1.0f - sin(g_AngleBias));
    return 1.0 - ao / g_NumDir * g_Contrast;
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

const float4 ClearColorDepth : DIFFUSE = { 0.0f, 0.0f, 0.0f, 1.0f};
const float4 ClearColor : DIFFUSE = { 0.0f, 0.0f, 0.0f, 1.0f};
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
        CullMode = None;   // For FX Composer
        PixelShader  = compile ps_3_0 DepthNormalsPixelShaderFunction();
    }
	pass Depth
	<
		string Script = "RenderColorTarget0=highPresicionDepthTexture;"
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
        CullMode = None;   // For FX Composer
        PixelShader  = compile ps_3_0 HighPresicionDepthPixelShaderFunction();
    }
    pass SSAO
	<
		string Script = "RenderColorTarget0=;"
						"ClearSetColor=ClearDepth;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
			            "Draw=Buffer;"; 
	>
    {          
        VertexShader = compile vs_3_0 VertexShaderFunction();
		CullMode = CCW; 
        PixelShader = compile ps_3_0 PixelShaderFunction(true);
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
        PixelShader = compile ps_3_0 PixelShaderFunction(true);
    }
}