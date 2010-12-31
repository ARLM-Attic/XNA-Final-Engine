
float4x4 World					: WORLD;
float4x4 ViewProjection			: VIEWPROJECTION;

float2 postProjToScreen(float4 pos)
{
	float2 scrPos = pos.xy / pos.w;
	return (0.5f * (float2(scrPos.x, -scrPos.y) + 1));
}

float2 ScreenRes			: SCREENRES;

float2 halfPixel()
{
	return (0.5f / ScreenRes);
}

texture lightMap;

sampler2D lightSampler = sampler_state
{
	Texture = <lightMap>;
	MipFilter = POINT;
	MagFilter = POINT;
	MinFilter = POINT;
};

struct VS_OUT
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float4 PostProj : TEXCOORD1;
};

VS_OUT vs_main(in float4 pos : POSITION)
{
	VS_OUT output = (VS_OUT)0;
	
	float4x4 wvp = mul(World, ViewProjection);
	output.Position = mul(pos, wvp);
	output.PostProj = output.Position;
	
	return output;
}

float4 ps_main(in float4 postProj : TEXCOORD1) : COLOR
{
	// Find the screen space texture coordinate & offset
	float2 scrPos = postProjToScreen(postProj) + halfPixel();
	
	float4 light = tex2D(lightSampler, scrPos);
	
	// This is similar to the standard Blinn Phong lighting model.
	// You would also add in a texture term here if so desired.
	float3 combined = saturate(light.rgb + light.aaa);
	
	return float4(combined, 1);
}

technique BlinnPhong
{
    pass p0
    {
        VertexShader = compile vs_2_0 vs_main();
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
        PixelShader = compile ps_2_0 ps_main();
    }
}
