/***********************************************************************************************************************************************

From the NVIDIA DirectX 10 SDK
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

#define M_PI 3.14159265f
#include <..\GBuffer\GBufferReader.fxh>
#include <..\Helpers\Discard.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

// Between 0 to 32
int  numberSteps = 32;

// Between 0 to 25
int  numberDirections = 25;

float  radius = 0.03;

// Between 0.1 to 3.1416
float  angleBias = 0.2;

float  attenuation = 1;

float  contrast = 2;

float2 focalLength;

float2 invFocalLength;

float2 resolution;

float2 invResolution;

float sqrRadius;

float invRadius;

float tanAngleBias;

float bounceStrength = 1000;

float bounceSingularity = 2700;

float2 halfPixel;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture randomTexture  : register(t3);
sampler2D randomNormalSampler : register(s3) = sampler_state
{
	Texture = <randomTexture>;
    /*ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = POINT;
	MINFILTER = POINT;*/
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUTPUT
{	
	float4 position   : POSITION;
    float2 uv         : TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_OUTPUT VertexShaderFunction(in float4 position : POSITION, in float2 uv : TEXCOORD)
{	
	VS_OUTPUT output = (VS_OUTPUT)0;
		
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	output.uv = uv;
		
	return output;
} // VertexShaderFunction

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float tangent(float3 P, float3 S)
{
    return (P.z - S.z) / length(S.xy - P.xy);
}

float3 uv_to_eye(float2 uv, float eye_z)
{
    uv = (uv * float2(2.0, -2.0) - float2(1.0, -1.0)); // uv (0, 1) to (-1, 1)
    return float3(uv * invFocalLength * eye_z, eye_z); // Position in view space	
}

float3 fetch_eye_pos(float2 uv)
{
	float z = tex2Dlod(depthSampler, float4(uv, 0, 0)).r; // Single channel zbuffer texture
    return uv_to_eye(uv, z);
}

// I don't use face normals, but it works!!
float3 tangent_eye_pos(float2 uv, float4 tangentPlane)
{
    // view vector going through the surface point at uv
    float3 V = fetch_eye_pos(uv);
    float NdotV = dot(tangentPlane.xyz, V);
    // intersect with tangent plane except for silhouette edges    
	if (NdotV < 0.0)
	{
		V *= (tangentPlane.w / NdotV);
	}
    return V;
}

float length2(float3 v)
{
	return dot(v, v);
} 

float3 min_diff(float3 P, float3 Pr, float3 Pl)
{
    float3 V1 = Pr - P;
    float3 V2 = P - Pl;
    return (length2(V1) < length2(V2)) ? V1 : V2;
}

float Falloff(float r)
{
	return 1.0f - attenuation * r * r;
}

float2 snap_uv_offset(float2 uv)
{
    return round(uv * resolution) * invResolution;
}

float2 snap_uv_coord(float2 uv)
{
    return uv - (frac(uv * resolution) - 0.5f) * invResolution;
}

float tan_to_sin(float x)
{
    return x / sqrt(1.0f + x*x);
}

float3 tangent_vector(float2 deltaUV, float3 dPdu, float3 dPdv)
{
    return deltaUV.x * dPdu + deltaUV.y * dPdv;
}

float tangent(float3 T)
{
    return -T.z / length(T.xy);
}

float biased_tangent(float3 T)
{
    float phi = atan(tangent(T)) + angleBias;
    return tan(min(phi, M_PI*0.5));
}

float2 rotate_direction(float2 Dir, float2 CosSin)
{
    return float2(Dir.x * CosSin.x - Dir.y * CosSin.y, 
                  Dir.x * CosSin.y + Dir.y * CosSin.x);
}