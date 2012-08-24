/******************************************************************************

    From MJP example (mpettineo@gmail.com)	
	Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

******************************************************************************/

#include <ShadowMapCommon.fxh>
#include <..\Helpers\Discard.fxh>

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 viewToLightViewProj;

//////////////////////////////////////////////
///////////// Render Shadow Map //////////////
//////////////////////////////////////////////

// Pixel shader for computing the shadow occlusion factor
float4 ps_main(in float2 uv : TEXCOORD0, in float3 frustumRay : TEXCOORD1, uniform int iFilterSize	) : COLOR0
{
	// Reconstruct position from the depth value, making use of the ray pointing towards the far clip plane	
	float depth = tex2D(depthSampler, uv).r;

	if (depth == 1) 
		return float4(1, 1, 1, 1);

	float3 positionVS = frustumRay * depth; // To convert this position into world space it only needs to add the camera position (in the pixel shader), and the frustumray multiply by the camera orientation (in the vertex shader).
	
	// Determine the depth of the pixel with respect to the light
	float4 positionLightCS = mul(float4(positionVS, 1), viewToLightViewProj);
		
	float depthLightSpace = positionLightCS.z / positionLightCS.w; // range 0 to 1

	[branch]
	if (depthLightSpace > 1) // If it is outside the far plane
	{
		Discard();
		return float4(1, 1, 1, 1);
	}
	
	// Transform from light space to shadow map texture space.
    float2 shadowTexCoord = 0.5 * positionLightCS.xy / positionLightCS.w + float2(0.5f, 0.5f);
    shadowTexCoord.y = 1.0f - shadowTexCoord.y;
        
    // Offset the coordinate by half a texel so we sample it correctly
    shadowTexCoord += (0.5f / shadowMapSize);
			
	// Get the shadow occlusion factor and output it
	float shadowTerm;
	if (iFilterSize == 0)
		shadowTerm = CalculateShadowTermPoisonPCF(depthLightSpace, shadowTexCoord, depthBias);
	else if (iFilterSize == 2)		
		shadowTerm = CalculateShadowTermBilinearPCF(depthLightSpace, shadowTexCoord, depthBias);
	else
		shadowTerm = CalculateShadowTermSoftPCF(depthLightSpace, shadowTexCoord, iFilterSize, depthBias);
	
	// Attenuate over distance.
	depthLightSpace = pow(depthLightSpace, 20);
	return float4((shadowTerm + 1 * depthLightSpace), 1, 1, 1);

} // ps_main

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique RenderShadowMap2x2PCF
{
    pass p0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader  = compile ps_3_0 ps_main(2);	
    }
} // RenderShadowMap2x2PCF

technique RenderShadowMap3x3PCF
{
    pass p0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader  = compile ps_3_0 ps_main(3);	
    }
} // RenderShadowMap3x3PCF

technique RenderShadowMap5x5PCF
{
    pass p0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader  = compile ps_3_0 ps_main(5);	
    }
} // RenderShadowMap5x5PCF

technique RenderShadowMap7x7PCF
{
    pass p0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader  = compile ps_3_0 ps_main(7);	
    }
} // RenderShadowMap7x7PCF

technique RenderShadowMapPoisonPCF
{
    pass p0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader  = compile ps_3_0 ps_main(0);	
    }
} // RenderShadowMapPoisonPCF
