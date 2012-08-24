//-----------------------------------------------------------------------------
// File: TetrahedronShadowMap.fx
//
// Desc: Effect file for Tetrahedron shadow map.
//
// Copyright (c) Hung-Chien Liao. All rights reserved.
//-----------------------------------------------------------------------------

float4x4 g_mWorldViewProj;		// Tranform from object to camera view projection space
float4x4 g_mWorldLightView;		// Transform from object to light view space
float4x4 g_mTexTransform[6];	// Transform point into final perspective shadow map sapace
float4x4 g_mTSMFaceCenter = {0.0f,			0.0f,			-0.81649655f,	0.81649655f,
							-0.57735026f,	-0.57735026f,	0.57735026f,	0.57735026f,
							0.81649661f,	-0.81649661f,	0.0f,			0.0f,
							0.0f,			0.0f,			0.0f,			0.0f};

texture  g_txShadowFront;		// front side shadow map
sampler2D g_samShadowFront = sampler_state
{
    Texture = <g_txShadowFront>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
    AddressU = WRAP;	// Need wrap for look up shadow map
    AddressV = WRAP;	// Need wrap for look up shadow map
};

//-----------------------------------------------------------------------------
// Vertex Shader: VSPointTSM
// Desc: Process vertex for scene with Tetrahedron shadow map
//-----------------------------------------------------------------------------
void VSPointTSM(float4 iPos : POSITION,	// input vertex position in object space
	float3 iNormal   : NORMAL,			// input vertex normal
	float2 iTexCoord : TEXCOORD0,		// input texture coordinate
	out float4 oPos : POSITION,			// output vertex position in clip space
	out float2 oTexCoord0 : TEXCOORD0,	// output texture coordinate for diffuse map
	out float3 oNormal : TEXCOORD1,		// output the vertex normal in light view space
	out float3 oVertexPos :TEXCOORD2,	// output the vertex position in light view space
	out float4 oSMTexCoord0 : TEXCOORD3,
	out float4 oSMTexCoord1 : TEXCOORD4,
	out float4 oSMTexCoord2 : TEXCOORD5,
	out float4 oSMTexCoord3 : TEXCOORD6)	// Shadow map texture coordinate
{
	// copy texture coordinates
	oTexCoord0 = iTexCoord;

	// transform position to clip space
	oPos = mul(iPos, g_mWorldViewProj);
			
	// Transform unit vertex position from object space to light view space
	oVertexPos.xyz = mul(iPos, g_mWorldLightView);
	float4 vVertexPos = float4(oVertexPos.xyz, 1.0f);
	oSMTexCoord0 = mul(vVertexPos, g_mTexTransform[0]);
	oSMTexCoord1 = mul(vVertexPos, g_mTexTransform[1]);
	oSMTexCoord2 = mul(vVertexPos, g_mTexTransform[2]);
	oSMTexCoord3 = mul(vVertexPos, g_mTexTransform[3]);
	
	// Transform the normal to light view space
	oNormal = mul(iNormal, g_mWorldLightView);
}


//-----------------------------------------------------------------------------
// Pixel Shader: PSPointTSM
// Desc: Process pixel for scene with Tetrahedron shadow map
//-----------------------------------------------------------------------------
float4 PSPointTSM(float2 iTexCoord0 : TEXCOORD0,	// input texture coordinate for diffuse map
	float3 iNormal : TEXCOORD1,		// input the vertex normal in light view space
	float3 iVertexPos : TEXCOORD2,	// input the vertex position in light view space
	float4 iSMTexCoord0 : TEXCOORD3,// Shadow map texture coordinate
	float4 iSMTexCoord1 : TEXCOORD4,
	float4 iSMTexCoord2 : TEXCOORD5,
	float4 iSMTexCoord3 : TEXCOORD6) : COLOR 
{
	/*// Determine the distance from the light to the vertex and the direction
	float3 vLightDir = -iVertexPos;
	float fDistance = length(vLightDir);
	vLightDir = vLightDir / fDistance;
	float fNdotL = dot(vLightDir, iNormal);
	// Compute the per-pixel distance based attenuation
	float fAttenuation = clamp( 0, 1, 1 / (g_vLightAttenuation.x +
		g_vLightAttenuation.y * fDistance + g_vLightAttenuation.z * fDistance * fDistance) );
	*/
	float4 vFaceDeter = mul(float4(iVertexPos.xyz, 0.0f), g_mTSMFaceCenter);
	float fMax = max(max(vFaceDeter.x, vFaceDeter.y), max(vFaceDeter.z, vFaceDeter.w));
	float4 shadowTexCoordDepth;
	if (vFaceDeter.x == fMax)
		shadowTexCoordDepth = iSMTexCoord0;
	else if (vFaceDeter.y == fMax)
		shadowTexCoordDepth = iSMTexCoord1;
	else if (vFaceDeter.z == fMax)
		shadowTexCoordDepth = iSMTexCoord2;
	else
		shadowTexCoordDepth = iSMTexCoord3;
		
	float shadow = (shadowTexCoordDepth.z / shadowTexCoordDepth.w <= tex2Dproj(g_samShadowFront, shadowTexCoordDepth));
		
	// Texture color * Lighting(Diffuse * Attenuation) Illumination
	return /*tex2D(g_samDiffuse, iTexCoord0) * ((fNdotL * g_vLightDiffuse) * fAttenuation **/ shadow;
}

//-----------------------------------------------------------------------------
// Technique: PointTSM
// Desc: Renders scene with Tetrahedron shadow map
//-----------------------------------------------------------------------------
technique PointTSM
{
	pass p0
	{		
		VertexShader = compile vs_2_0 VSPointTSM();
		PixelShader = compile ps_2_0 PSPointTSM();		
	}
}