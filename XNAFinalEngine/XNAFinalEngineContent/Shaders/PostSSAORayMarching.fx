/***********************************************************************************************************************************************

Based in the DirectX 10 shader of NVIDIA.
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

#include <CommonDepthNormals.fxh>
#define FX_COMPOSER;

//////////////////////////////////////////////
///////////////// Script /////////////////////
//////////////////////////////////////////////

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";

	string Script = "Technique=Technique?SSAOFX;";
> = 0.8; // SAS version

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float AspectRatio <
    string UIWidget = "slider";
    float UIMin = 1;
    float UIMax = 3;
    float UIStep = 0.1f;
    string UIName = "Aspect Ratio";
> = 1.6;

int g_NumSteps <
    string UIWidget = "slider";
    float UIMin = 1;
    float UIMax = 32;
    float UIStep = 1;
    string UIName = "Number of steps";
> = 4.0;

float g_R <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 2;
    float UIStep = 0.05;
    string UIName = "Radius";
> = 0.1;

int g_NumDirs <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 15;
    float UIStep = 1;
    string UIName = "Number of Directions";
> = 10;

int g_NumRays <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 25;
    float UIStep = 1;
    string UIName = "Number of Rays";
> = 4;

float g_LinAtt <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 2;
    float UIStep = 0.05;
    string UIName = "Line Attenuation";
> = 1;

float g_Contrast <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 2;
    float UIStep = 0.05;
    string UIName = "Contrast";
> = 1;

float2 g_FocalLen <
	string UIWidget = "slider";
> = float2(2.77, 1.73);

float2 g_Dirs[16] <
	string UIWidget = "none";
>
= {
	normalize(float2(0.355512, 	-0.709318)),
	normalize(float2(0.534186, 	 0.71511)),
	normalize(float2(-0.87866, 	 0.157139)),
	normalize(float2(0.140679, 	-0.475516)),
	normalize(float2(-0.0796121, 0.158842)),
	normalize(float2(-0.0759516,-0.101676)),
	normalize(float2(0.12493, 	-0.0223423)),
	normalize(float2(-0.0720074, 0.243395)),
	normalize(float2(-0.207641,  0.414286)),
	normalize(float2(-0.277332, -0.371262)),
	normalize(float2(0.63864, 	-0.114214)),
	normalize(float2(-0.184051,  0.622119)),
	normalize(float2(0.110007, 	-0.219486)),
	normalize(float2(0.235085, 	 0.314707)),
	normalize(float2(-0.290012,  0.0518654)),
	normalize(float2(0.0975089, -0.329594))
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

float3 fetch_eye_pos(float3 pos)
{
    float2 r =  pos.xy / pos.z;
    float2 tx_n = float2(0.5, -0.5)*(g_FocalLen * r + float2(1.0, -1.0));
    
	// Esta operacion (tex2dlod) se utiliza para evitar usar una operacion grandiente.
	//float z = tex2Dlod(depthNormalSampler, float4(tx_n.x, tx_n.y, 0, 1)).a; 
	float z = tex2Dlod(highPresicionDepthSampler, float4(tx_n.x, tx_n.y, 0, 1)).r; 
	
    return float3(z * r.x, z * r.y, z);
}

float4 PixelShaderFunction(VS_OUTPUT IN) : COLOR0
{	
	//float z = tex2D(depthNormalSampler, IN.texUV).a; 
    float z = tex2D(highPresicionDepthSampler, IN.texUV).r;
    float3 P = float3(z*IN.tex.xy, z);
	
	// Account for far plane
    //if(P.z < 0.05)
//      return 1;

    // Get the basis per pixel
	float3 N = normalize(tex2D(depthNormalSampler, IN.texUV).rgb);
    float3 Tan   = float3(1,0,0);
    float3 BiTan = normalize(cross(N, Tan));
    Tan          = cross(BiTan, N);

    const float step_size = g_R / g_NumSteps;
    float3 rand = tex2D( RandNormal, IN.texUV * 200).rgb;// = tRandom.Load(int3((int)IN.pos.x&63, (int)IN.pos.y&63, 0)).xyz;
    
    float3 dir_t;
    float color = 0.0;
    for (int d = 0; d < g_NumDirs; d++)
	{
        float3 dir = float3(g_Dirs[d].x*rand.x - g_Dirs[d].y*rand.y, 
                            g_Dirs[d].x*rand.y + g_Dirs[d].y*rand.x, 
                            0);

        dir = dir.x*Tan + dir.y*BiTan;

        float n_weight = 1.0/(g_NumRays*g_NumDirs);
        for (float n = 1; n <= g_NumRays; n++)
		{
            float frac = n/(g_NumRays + 1.5e-2);
//#ifdef COSINE_WEIGHTED
            //float3 ndir = dir*sqrt(frac) + N*sqrt(1-frac);		
//#else
            float3 ndir = dir*frac + N*sqrt(1.0-frac*frac);
//#endif

            for (float i = 1.0; i <= g_NumSteps; i++)
			{
                float3 cur_ray = (i + rand.z) * step_size * ndir;
                float3 cur_pos = cur_ray + P;
                float3 tex_pos = fetch_eye_pos(cur_pos);
        
                if (tex_pos.z - cur_pos.z > 0.0) {
                    float l = length(P - tex_pos);
                    if (l < g_R) {
                        color -= n_weight * (g_R - g_LinAtt * l) / g_R;					
                        i = g_NumSteps + 1; // break;
                    }
                }
            }
        }
    }
    return 1.0 + color * g_Contrast;
}

float4 PixelShaderFunctionFixedSteps(VS_OUTPUT IN) : COLOR0
{	
    float z = tex2D(highPresicionDepthSampler, IN.texUV).r;
    float3 P = float3(z*IN.tex.xy, z);
	
	// Account for far plane
    if(P.z < 0.05)
      return 1;

    // Get the basis per pixel
	float3 N = normalize(tex2D(depthNormalSampler, IN.texUV).rgb);
    float3 Tan   = float3(1,0,0);
    float3 BiTan = normalize(cross(N, Tan));
    Tan          = cross(BiTan, N);

    const float step_size = g_R / g_NumSteps;
    float3 rand = tex2D( RandNormal, IN.texUV * 200).rgb;// = tRandom.Load(int3((int)IN.pos.x&63, (int)IN.pos.y&63, 0)).xyz;
    
    float3 dir_t;
    float color = 0.0;
    for (int d = 0; d < 6/*g_NumDirs*/; d++)
	{
        float3 dir = float3(g_Dirs[d].x * rand.x - g_Dirs[d].y * rand.y, 
                            g_Dirs[d].x * rand.y + g_Dirs[d].y * rand.x, 
                            0);

        dir = dir.x*Tan + dir.y*BiTan;

        float n_weight = 1.0/(g_NumRays*g_NumDirs);
        for (float n = 1; n <= 4/*g_NumRays*/; n++)
		{
            float frac = n / (g_NumRays + 1.5e-2);
//#ifdef COSINE_WEIGHTED
            //float3 ndir = dir * sqrt(frac) + N * sqrt(1 - frac);		
//#else
            float3 ndir = dir * frac + N*sqrt(1.0 - frac * frac);
//#endif

            for (float i = 1.0; i <= 4/*g_NumSteps*/; i++)
			{
                float3 cur_ray = (i  +rand.z) * step_size * ndir;
                float3 cur_pos = cur_ray + P;
                float3 tex_pos = fetch_eye_pos(cur_pos);
        
                if (tex_pos.z - cur_pos.z > 0.0) {
                    float l = length(P - tex_pos);
                    if (l < g_R) {
                        color -= n_weight * (g_R - g_LinAtt * l) / g_R;					
                        i = g_NumSteps + 1; // break;
                    }
                }
            }
        }
    }
    return 1.0 + color * g_Contrast;
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
        PixelShader  = compile ps_3_0 PixelShaderFunctionFixedSteps();
    }
}