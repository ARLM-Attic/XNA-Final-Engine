// Just for testing

#define M_PI 3.14159265f
#include <..\GBuffer\GBuffer.fx>
#include <..\Helpers\SphericalHarmonics.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 g_FocalLen;

float3 frustumCorners[4];

const float PI = 3.141592;

int patternSize = 2;

// The number of samples for one pixel
uniform int sampleCount = 16;

// The radius in world units the samples that come from a unit sphere are scaled to
uniform float sampleRadius = 0.01;

// Singularity for distance
uniform float singularity = 0.01;

// Strength of the occlusion effect itself
uniform float strength = 4;

float4x4 ViewI;

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
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position		: POSITION;
	float2 uv			: TEXCOORD0;
	float2 tex			: TEXCOORD1;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

float3 FrustumRay(in float2 uv)
{
	float  index = uv.x + (uv.y * 2);
	return frustumCorners[index];
}

VS_OUT VertexShaderFunction(in float4 position : POSITION, in float2 uv : TEXCOORD)
{
	VS_OUT output = (VS_OUT)0;
	
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	output.uv = uv; 

	output.tex = position.xy / g_FocalLen;
	
	return output;
} // VertexShaderFunction

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

// Compute the local frame (currently no check for singularities)
float3x3 computeTripodMatrix(const float3 direction, const float3 up = float3(0.01, 0.99, 0)) 
{			
	float3 tangent   = normalize(cross(up, direction));
	float3 cotangent = normalize(cross(direction, tangent));
	return float3x3(tangent, cotangent, direction);
}

//----------------------------------------------------------------------------------
float4 PixelShaderFunction(in float2 uv : TEXCOORD0, in float2 tex : TEXCOORD2, in float2 fragCoord : VPOS) : COLOR0
{
	// Accumulation over radiance
	float3 directRadianceSum = float3(0, 0, 0);
	float3 occluderRadianceSum = float3(0, 0, 0);
	float3 ambientRadianceSum = float3(0, 0, 0);
	float  ambientOcclusion = 0;

	float  z = tex2D(depthSampler, uv).r;
	float3 viewPosition = float3(z * tex, z);

	// Account for far plane
    if(z >= 1)
		return float4(0, 0, 0, 0);

	float3 normal = SampleNormal(uv);	
	//normal.z = -normal.z; // I'm not sure about this, but it works. I suppose that this happen because DirectX is left handed and XNA is not.
		
	// A matrix that transforms from the unit hemisphere along z = -1 to the local frame along this normal
	float3x3 localMatrix = computeTripodMatrix(normal);
	
	// The index of the current pattern
	// We use one out of patternSize * patternSize pre-defined hemisphere patterns.
	// The i'th pixel in every sub-rectangle uses always the same i-th sub-pattern.
	//int patternIndex = int(fragCoord.x) % patternSize + (int(fragCoord.y) % patternSize) * patternSize;
					
	// Loop over all samples from the current pattern
	for(int i = 0; i < sampleCount ; i++)
	{		
		// Get the i-th sample direction and tranfrom it to local space.
		float3 sample = mul(tex2D(randomNormalSampler, uv * 200).rgb, localMatrix); // TODO!!!
			
		float3 normalizedSample = normalize(sample);
			
		// Go sample-radius steps along the sample direction, starting at the current pixel world space location
		float3 viewSampleOccluderPosition = viewPosition + sampleRadius * sample;
				
		float2 r =  viewSampleOccluderPosition.xy / viewSampleOccluderPosition.z;
		float2 tx_n = float2(0.5, -0.5) * (g_FocalLen * r + float2(1.0, -1.0));    
		float zSample = tex2D(depthSampler, tx_n).r;
		float3 occluderPosition = float3(zSample * r, zSample);
		//float3 occluderNormal = SampleNormal(tx_n);
			
		/*
		// remove influence of undefined pixels 
		if (length(occluderNormal) == 0) {
			occluderPosition = vec4(100000.0, 100000.0, 100000.0, 1.0);
		}*/
							
		// First compute a delta towards the blocker, its length and its normalized version.
		float distanceTerm = abs(viewPosition.z - occluderPosition.z /*+ depthBias*/) < singularity ? 1.0 : 0.0;
			
		float visibility = 1.0 - strength * (occluderPosition.z > viewPosition.z ? 1.0 : 0.0) * distanceTerm;
									
		// Geometric term of the current pixel towards the current sample direction
		float receiverGeometricTerm = max(0.0, dot(normalizedSample, normal));

		// Get the irradiance in the current direction
		/*float theta = acos(normalizedSample.y);              
		float phi = atan(normalizedSample.z, normalizedSample.x);
		phi += lightRotationAngle;
		if (phi < 0) phi += 2*PI;
		if (phi > 2*PI) phi -= 2*PI;*/
						
		float3 senderRadiance = SampleSH(normalize(mul(normalizedSample, ViewI))); //texture2D(envmapTexture, vec2( phi / (2.0*PI), 1.0 - theta / PI ) ).rgb;

		// Compute radiance as the usual triple product of visibility, irradiance and BRDF.
		// Note, that we are not limited to diffuse illumination.
		// For practical reasons, we post-multiply with the diffuse color.

		float3 radiance = visibility * receiverGeometricTerm * senderRadiance;
			
		// Accumulate the radiance from this sample
		directRadianceSum += radiance;
			
		float3 ambientRadiance = senderRadiance * receiverGeometricTerm;
		ambientRadianceSum += ambientRadiance;
		ambientOcclusion += visibility;
		/*
		// Compute indirect bounce radiance
		// First read sender radiance from occluder			
		vec3 directRadiance = sampleDirectRadiance(occlusionTexCoord);
			
		// Compute the bounce geometric term towards the blocker.
		vec3 delta = position.xyz - occluderPosition.xyz;
		vec3 normalizedDelta = normalize(delta);			
		float unclampedBounceGeometricTerm = 
			max(0.0, dot(normalizedDelta, -normal)) * 
			max(0.0, dot(normalizedDelta, occluderNormal)) /
			max(dot(delta, delta), bounceSingularity);				
		float bounceGeometricTerm = min(unclampedBounceGeometricTerm, bounceSingularity);
			
		// The radiance due to bounce
		vec3 bounceRadiance = bounceStrength * directRadiance * bounceGeometricTerm;						
			
		// Compute radiance from this occcluder (mixing bounce and scatter)
		// vec3 occluderRadiance = bounceRadiance * receiverGeometricTerm;
		vec3 occluderRadiance = bounceRadiance;
			
		// Finally, add the indirect light to the light sum

		occluderRadianceSum += occluderRadiance;	*/		
	}
		
	// Clamp to zero.
	// Althought there should be nothing negative here it is suitable to allow single samples do DARKEN with their contribution.
	// This can be used to exaggerate the directional effect and gives nicely colored shadows (instead of AO).
	directRadianceSum   = max(float3(0, 0, 0), directRadianceSum);
	occluderRadianceSum = max(float3(0, 0, 0), occluderRadianceSum);

	// Add direct and indirect radiance
	float3 radianceSum = directRadianceSum + occluderRadianceSum;
		
	// Multiply by solid angle for one sample
	radianceSum *= 2.0 * PI / sampleCount;
				
	// Store final radiance value in the framebuffer
	return float4(radianceSum / 500, 1);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique SSDO
{
    pass P0
    {          
        VertexShader = compile vs_3_0 VertexShaderFunction();
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = CCW; 
		AlphaBlendEnable = false;
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}