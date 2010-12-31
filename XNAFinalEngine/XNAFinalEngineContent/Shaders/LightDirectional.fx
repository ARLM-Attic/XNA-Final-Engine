
float3 ViewPosition				: VIEWPOSITION;

float2 ScreenRes			: SCREENRES;

float2 halfPixel()
{
	return (0.5f / ScreenRes);
}

float3 FrustumCorners[4]	: FRUSTUMCORNERS;

texture DepthTexture : RENDERCOLORTARGET
< 
    float2 ViewportRatio = { 1, 1 };
	string Format="R32F";
    int MIPLEVELS = 1;
>;

sampler2D DepthSampler = sampler_state
{
	Texture = <DepthTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
};

texture NormalMap;

sampler2D normalSampler = sampler_state
{
	Texture = <NormalMap>;
	AddressU = CLAMP;
    AddressV = CLAMP;
	MipFilter = POINT;
	MagFilter = POINT;
	MinFilter = POINT;
};

float3 LightColor;
float3 LightDir;
float SpecPower = 16;

struct VS_OUT
{
	float4 Position		: POSITION;
	float2 TexCoord		: TEXCOORD0;
	float3 FrustumRay	: TEXCOORD1;
};

float3 FSQ_GetFrustumRay(in float2 texCoord)
{
	float index = texCoord.x + (texCoord.y * 2);
	return FrustumCorners[index];
}

VS_OUT vs_main(in float4 pos : POSITION, in float2 tc : TEXCOORD)
{
	VS_OUT output = (VS_OUT)0;
	
	output.Position = pos;
	output.TexCoord = tc + halfPixel();

	output.FrustumRay = FSQ_GetFrustumRay(tc);
	
	return output;
}

float4 ps_main(in float2 tc : TEXCOORD0, in float3 ray : TEXCOORD1) : COLOR0
{
	// Reconstruct position from the depth value, making use of the ray pointing towards the far clip plane	
	float depth = tex2D(DepthSampler, tc).r;
	float3 positionWorld = ray * (1 - depth);
	float3 V = normalize(ViewPosition - positionWorld);
		
	float3 N = float3(0,0,0);
	N.z = dot(float3(tex2D(normalSampler, tc).xy, 1), -float3(tex2D(normalSampler, tc).xy, -1)) * 2 - 1;
	N.xy = normalize(tex2D(normalSampler, tc).xy) * sqrt(1 - N.z * N.z);
	N = normalize(N);

	float3 L = normalize(-LightDir); // TODO Sacar el normalize.
	// N dot L lighting term
	float NL = max(0, dot(L, N));
	
	// Calculate specular term
	float3 H = normalize(L + V);

	float4 litVec  = lit(NL, dot(H, N), 300);
    //diffContrib    = litVec.y * LightColor;
    float spec    = ((litVec.z * 2) * litVec.y);

	//float spec = pow(saturate(max(0, dot(H, N))), SpecPower);
	
	// Fill the light buffer:
	// R: Color.r * N.L
	// G: Color.g * N.L
	// B: Color.b * N.L
	// A: Specular Term
		
	//return float4(V, 1);
	return float4(LightColor * NL, spec);
}

technique DirectionalLight
{
	pass p0
	{
		VertexShader = compile vs_3_0 vs_main();
		ZEnable = false;
		ZWriteEnable = false; 
		AlphaBlendEnable = true;
		PixelShader = compile ps_3_0 ps_main();
	}
}
