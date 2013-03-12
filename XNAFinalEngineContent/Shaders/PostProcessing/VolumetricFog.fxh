/***********************************************************************************************************************************************
This is based on Crytek presentation: Real-time Atmospheric Effects in Games

Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
************************************************************************************************************************************************/

float3 cameraPosition;
float3x3 viewI;

//float4 vfViewPos;
//float4 vfParams;
//float3 vfFogColor;
//float4 vfRampParams;

//float3 slHazeColPartialRayleighInScatter;
//float3 slHazeColPartialMieInScatter;
//float3 slSunDirection;
//float3 slPhaseFunctionConstants;

//float4 miscCamFront;

float3 vfColGradBase = float3(0.3f, 0.2f, 0.8f);
float3 vfColGradDelta = float3(1, 0, 0);

half ComputeVolumetricFog( in float3 cameraToWorldPos )
{
	/*#define atmosphereScale					vfParams.x
	#define volFogHeightDensityAtViewer 	half(vfParams.y)
	#define fogDensity						half(vfParams.z)
	#define artistTweakDensityOffset		half(vfParams.w)*/
	
	/*float heightFalloff = 0.1f;
	float globalDensity = 0.2f;

	float VolFogHeightDensityAtViewer = exp(-heightFalloff * cameraPosition.y);

	float fogInt = length(cameraToWorldPos) * VolFogHeightDensityAtViewer;
	static const float c_slopeThreshold = 0.01f;
	if(abs(cameraToWorldPos.y) > c_slopeThreshold)
	{
		float t = heightFalloff * cameraToWorldPos.y;
		fogInt *= ( 1.0 - exp( -t ) ) / t;
	}
	return exp(-globalDensity * fogInt);
	*/
	float atmosphereScale =  0.5; // heightFalloff	
	float artistTweakDensityOffset = 0;
	float fogDensity = 0.04f;
	float waterLevel = -35.5f;

	half fogInt = 1.h;

	static const float c_slopeThreshold = 0.01f;
	if(abs(cameraToWorldPos.y) > c_slopeThreshold)
	{
		float t = atmosphereScale * cameraToWorldPos.y;
		fogInt *= (1.f - exp(-t)) / t;
	}

	// NOTE: volFogHeightDensityAtViewer = log2(e) * fogDensity * exp( -atmosphereScale * ( vfViewPos.z - waterLevel ) );
	float volFogHeightDensityAtViewer = log2(2.718) * fogDensity * exp(-atmosphereScale * (cameraPosition.y - waterLevel));
	half l = length(cameraToWorldPos);
	half u = l * volFogHeightDensityAtViewer;
	fogInt = fogInt * u - artistTweakDensityOffset;

	half f = saturate(exp2(-fogInt));
	return f;

	/*half r = saturate(l * vfRampParams.x + vfRampParams.y);
	r = r * (2-r);
	//r = smoothstep(0, 1, r);
	r = r * vfRampParams.z + vfRampParams.w;
	f = (1-f) * r;
	return (1-f);*/
}

//////////////////////////////////////////////////////////////////////////
// Distance based implementation
//////////////////////////////////////////////////////////////////////////

// RET.xyz = fog color (HDR)
// RET.w = fog factor to lerp scene/object color with (i.e. lerp( RET.xyz, sceneColor.xyz, RET.w ) )
float4 GetVolumetricFogColorDistanceBased(float3 cameraToWorldPos) : COLOR
{	    
	cameraToWorldPos = mul(cameraToWorldPos, viewI);
	cameraToWorldPos = cameraToWorldPos - cameraPosition;

    half fog = ComputeVolumetricFog(cameraToWorldPos);
	half l = saturate(normalize(cameraToWorldPos.xyz).y);
	half3 fogColor = vfColGradBase + l * vfColGradDelta;
	return float4(fogColor, fog);
}