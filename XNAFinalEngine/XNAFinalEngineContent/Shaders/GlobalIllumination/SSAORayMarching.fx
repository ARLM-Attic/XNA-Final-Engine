/***********************************************************************************************************************************************

From the NVIDIA DirectX 10 SDK
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

#include <..\GBuffer\GBuffer.fx>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float4x4 viewIT;

float2 focalLength;

int numberSteps <
    string UIWidget = "slider";
    float UIMin = 1;
    float UIMax = 32;
    float UIStep = 1;
    string UIName = "Number of steps";
> = 4.0;

float radius <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 2;
    float UIStep = 0.05;
    string UIName = "Radius";
> = 0.1;

int numberDirections <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 15;
    float UIStep = 1;
    string UIName = "Number of Directions";
> = 6;

int numberRays <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 25;
    float UIStep = 1;
    string UIName = "Number of Rays";
> = 4;

float linearAttenuation <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 2;
    float UIStep = 0.05;
    string UIName = "Line Attenuation";
> = 1;

float contrast <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 2;
    float UIStep = 0.05;
    string UIName = "Contrast";
> = 1;

float2 directions[16] 
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
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture randomTexture : Diffuse
<
	string UIName = "Random Texture";
	string ResourceName = "RANDOMNORMAL.png";
>;

sampler2D randomNormalSampler = sampler_state
{
	Texture = <randomTexture>;
    ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
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

VS_OUTPUT VertexShaderFunction(float4 position : POSITION, in float2 uv : TEXCOORD )
{	
	VS_OUTPUT Out = (VS_OUTPUT)0;
		
	Out.pos = position;
	Out.pos.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/

	Out.tex = position.xy / focalLength;

    Out.texUV = uv;
		
	return Out;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float3 fetch_eye_pos(float3 pos)
{
    float2 r =  pos.xy / pos.z;
    float2 tx_n = float2(0.5, -0.5) * (focalLength * r + float2(1.0, -1.0));
    
	// The tex2dlod operation avoids a gradient operation.
	float z = tex2Dlod(depthSampler, float4(tx_n, 0, 1)).r; 
	//float z = tex2D(depthSampler, tx_n).r;
	
    return float3(z * r, z);
} // fetch_eye_pos

float4 PixelShaderFunction(VS_OUTPUT IN) : COLOR0
{	
    float  z = tex2D(depthSampler, IN.texUV).r;
    float3 P = float3(z * IN.tex, z);
		
	// Account for far plane
    if(P.z > 0.95)
		return float4(1, 1, 1, 1);

    // Get the basis per pixel
	/*float3 N;
	float3 Tan;
	/*
	[branch]
	//if (useNormals)
	{
		N = SampleNormal(IN.texUV);
		Tan = SampleTangent(IN.texUV);
	}
	/*else
		N = mul(float3(0, 1, 0), viewIT);
		Tan = mul(float3(1, 0, 0), viewIT);
	}*/
	/*N.z = -N.z;
	Tan.z = -Tan.z;		
    float3 BiTan = normalize(cross(N, Tan));    */

	// Get the basis per pixel
	float3 N;
	[branch]
	//if (useNormals)
	{
		N = SampleNormal(IN.texUV);
	}
	//else
    //	N = mul(float3(0, 1, 0), viewIT);
	
	N.z = -N.z; // I'm not sure about this, but it works. I suppose that this happen because DirectX is left handed and XNA is not.
    float3 Tan   = float3(1, 0, 0);
    float3 BiTan = normalize(cross(N, Tan));
    Tan          = cross(BiTan, N);

    const float step_size = radius / numberSteps;

    float3 rand = tex2D(randomNormalSampler, IN.texUV * 200).rgb;// = tRandom.Load(int3((int)IN.pos.x&63, (int)IN.pos.y&63, 0)).xyz;
    
    float3 dir_t;
    float color = 0.0;
	float n_weight = 1.0 / (numberRays * numberDirections);
    for (int d = 0; d < numberDirections; d++)
	{
        float3 dir = float3(directions[d].x * rand.x - directions[d].y * rand.y, 
                            directions[d].x * rand.y + directions[d].y * rand.x, 
                            0);

        dir = dir.x * Tan + dir.y * BiTan;
		        
        for (float n = 1; n <= numberRays; n++)
		{
            float frac = n / (numberRays + 1.5e-2);
			// Cosine Weighted
            //float3 ndir = dir * sqrt(frac) + N * sqrt(1 - frac);
            float3 ndir = dir * frac + N * sqrt(1.0 - frac * frac);

            for (float i = 1.0; i <= numberSteps; i++)
			{
                float3 cur_ray = (i  + rand.z) * step_size * ndir;
                float3 cur_pos = cur_ray + P;
                float3 tex_pos = fetch_eye_pos(cur_pos);				

                if (tex_pos.z - cur_pos.z < 0.0)
				{					
                    float l = length(P - tex_pos);
                    if (l < radius)
					{
                        color -= n_weight * (radius - linearAttenuation * l) / radius;					
                        break; // i = numberSteps + 1;
                    }
                }
            }
        }
    }
    return float4(1.0 + color * contrast, 1, 1, 1);
} // PixelShaderFunction

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique SSAO
{
    pass P0
    {          
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader  = compile ps_3_0 PixelShaderFunction();
    }
}