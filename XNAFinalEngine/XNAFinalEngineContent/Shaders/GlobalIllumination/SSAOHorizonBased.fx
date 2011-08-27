/***********************************************************************************************************************************************

From the NVIDIA DirectX 10 SDK
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

#define M_PI 3.14159265f
#include <..\GBuffer\GBuffer.fx>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float  numberSteps <
    string UIWidget = "slider";
    float UIMin = 1;
    float UIMax = 32;
    float UIStep = 1;
    string UIName = "Numbers of Steps";
> = 32;

float  numberDirections <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 25;
    float UIStep = 1;
    string UIName = "Numbers of Directions";
> = 25;

float  radius = 0.03;

float  angleBias <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 3.1416;
    float UIStep = 0.05;
    string UIName = "Angle Bias";
> = 0.2;

float  attenuation <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 2;
    float UIStep = 0.05;
    string UIName = "Line Attenuation";
> = 1;

float  contrast <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 3;
    float UIStep = 0.1;
    string UIName = "Contrast";
> = 2;

float2 focalLength;

float2 invFocalLength;

float2 resolution;

float2 invResolution;

float sqrRadius;

float invRadius;

float tanAngleBias;

float bounceStrength = 1000;

float bounceSingularity = 2700;

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

texture radianceTexture : Diffuse;

sampler2D radianceSampler = sampler_state
{
	Texture = <radianceTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
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
    float2 texUV : TEXCOORD1;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_OUTPUT VertexShaderFunction(float4 Position : POSITION, in float2 uv : TEXCOORD)
{	
	VS_OUTPUT Out = (VS_OUTPUT)0;
		
	Out.pos = Position;
	Out.pos.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/

    Out.texUV = uv;
		
	return Out;
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
	float z = tex2D(depthSampler, uv).r; // Single channel zbuffer texture
    return uv_to_eye(uv, z);
}

// I don't use face normals, but it works!!
float3 tangent_eye_pos(float2 uv, float4 tangentPlane, float z)
{
    // view vector going through the surface point at uv
    float3 V = fetch_eye_pos(uv);
    float NdotV = dot(tangentPlane.xyz, V);
    // intersect with tangent plane except for silhouette edges
    if (NdotV < -0.01)
		V *= (tangentPlane.w / NdotV); 
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

void integrate_direction(inout float ao, float3 P, float2 uv, float2 deltaUV,
                         float numSteps, float tanH, float sinH)
{
	[unroll]
    for (float j = 1; j <= 8/*numSteps*/; ++j) {
        uv += deltaUV;
        float3 S = fetch_eye_pos(uv);
        
        // Ignore any samples outside the radius of influence
        float d2  = length2(S - P);
		[branch]
        if (d2 < sqrRadius) 
		{
            float tanS = tangent(P, S);

            [branch]
            if(tanS > tanH) 
			{
                // Accumulate AO between the horizon and the sample
                float sinS = tanS / sqrt(1.0f + tanS*tanS);
                float r = sqrt(d2) * invRadius;
                ao += Falloff(r) * (sinS - sinH);
                
                // Update the current horizon angle
                tanH = tanS;
                sinH = sinS;
            }
        }
    }
}

float AccumulatedHorizonOcclusionLowQuality(float2 deltaUV, 
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

    float tanT = tan(-M_PI*0.5 + angleBias);
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

float AccumulatedHorizonOcclusionMiddleQuality(float2 deltaUV, 
											   float2 uv0, 
											   float3 P, 
										  	   float numSteps, 
											   float randstep,
											   float3 dPdu,
											   float3 dPdv )
{
    // Randomize starting point within the first sample distance
    float2 uv = uv0 + snap_uv_offset( randstep * deltaUV );
    
    // Snap increments to pixels to avoid disparities between xy 
    // and z sample locations and sample along a line
    deltaUV = snap_uv_offset( deltaUV );

    // Compute tangent vector using the tangent plane
    float3 T = deltaUV.x * dPdu + deltaUV.y * dPdv;

    float tanH = biased_tangent(T);
    float sinH = tanH / sqrt(1.0f + tanH*tanH);

    float ao = 0;
	[unroll]
    for(float j = 1; j <= 8/*numSteps*/; ++j)
	{
        uv += deltaUV;
        float3 S = fetch_eye_pos(uv);
        
        // Ignore any samples outside the radius of influence
        float d2  = length2(S - P);
		[branch]
        if (d2 < sqrRadius)
		{
            float tanS = tangent(P, S);

            [branch]
            if(tanS > tanH)
			{
                // Accumulate AO between the horizon and the sample
                float sinS = tanS / sqrt(1.0f + tanS*tanS);
                float r = sqrt(d2) * invRadius;
                ao += Falloff(r) * (sinS - sinH);
                
                // Update the current horizon angle
                tanH = tanS;
                sinH = sinS;
            }
        } 
    }

    return ao;
}

float AccumulatedHorizonOcclusionHighQuality(float2 deltaUV, 
                                             float2 uv0, // uvs for original sample
                                             float3 P, 
                                             float numSteps, 
                                             float randstep,
                                             float3 dPdu,
                                             float3 dPdv,
										     float3 normal)
{
    // Jitter starting point within the first sample distance
    float2 uv = (uv0 + deltaUV) + randstep * deltaUV;
    
    // Snap first sample uv and initialize horizon tangent
    float2 snapped_duv = snap_uv_offset(uv - uv0);
    float3 T = tangent_vector(snapped_duv, dPdu, dPdv);
    float tanH = tangent(T) + tanAngleBias;

    float ao = 0;
    float h0 = 0;
	float3 occluderRadiance;
	[unroll]
    for(float j = 0; j < 8/*numSteps*/; ++j)
	{
        float2 snapped_uv = snap_uv_coord(uv);
        float3 S = fetch_eye_pos(snapped_uv);   
		// next uv in image space.
		uv += deltaUV;

        // Ignore any samples outside the radius of influence
        float d2 = length2(S - P);
		[branch]
        if (d2 < sqrRadius)
		{ 
            float tanS = tangent(P, S);

            [branch]
            if (tanS > tanH) // Is this height is bigger than the bigger height of this direction so far then
			{
                // Compute tangent vector associated with snapped_uv
                float2 snapped_duv = snapped_uv - uv0;
                float3 T = tangent_vector(snapped_duv, dPdu, dPdv);
                float tanT = tangent(T) + tanAngleBias;

                // Compute AO between tangent T and sample S
                float sinS = tan_to_sin(tanS);
                float sinT = tan_to_sin(tanT);
                float r = sqrt(d2) * invRadius;
                float h = sinS - sinT;
				float falloff = Falloff(r);
                ao += falloff * (h - h0);
                h0 = h;

                // Update the current horizon angle
                tanH = tanS;
            }
        }

    }
    return ao;
}
/*
float4 AccumulatedHorizonOcclusionHighQualityWithRadiosity(float2 deltaUV, 
												 		   float2 uv0, // uvs for original sample
														   float3 P, 
														   float numSteps, 
														   float randstep,
														   float3 dPdu,
														   float3 dPdv,
														   float3 normal)
{
    // Jitter starting point within the first sample distance
    float2 uv = (uv0 + deltaUV) + randstep * deltaUV;
    
    // Snap first sample uv and initialize horizon tangent
    float2 snapped_duv = snap_uv_offset(uv - uv0);
    float3 T = tangent_vector(snapped_duv, dPdu, dPdv);
    float tanH = tangent(T) + tanAngleBias;

    float ao = 0;
    float h0 = 0;
	float3 occluderRadiance;
	[unroll]
    for(float j = 0; j < 8/*numSteps*//*; ++j)
	{
        float2 snapped_uv = snap_uv_coord(uv);
        float3 S = fetch_eye_pos(snapped_uv);   
		// next uv in image space.
		uv += deltaUV;

        // Ignore any samples outside the radius of influence
        float d2 = length2(S - P);
		[branch]
        if (d2 < sqrRadius) { 
            float tanS = tangent(P, S);

            [branch]
            if (tanS > tanH) // Is this height is bigger than the bigger height of this direction so far then
			{
                // Compute tangent vector associated with snapped_uv
                float2 snapped_duv = snapped_uv - uv0;
                float3 T = tangent_vector(snapped_duv, dPdu, dPdv);
                float tanT = tangent(T) + tanAngleBias;

                // Compute AO between tangent T and sample S
                float sinS = tan_to_sin(tanS);
                float sinT = tan_to_sin(tanT);
                float r = sqrt(d2) * invRadius;
                float h = sinS - sinT;
				float falloff = Falloff(r);
                ao += falloff * (h - h0);
                h0 = h;

                // Update the current horizon angle
                tanH = tanS;

				// Compute indirect bounce radiance
					
				// First read sender radiance from occluder			
				float3 directRadiance = tex2D(radianceSampler, snapped_uv).rgb;

				float3 occluderNormal = SampleNormal(snapped_uv);
				occluderNormal.z = -occluderNormal.z;
			
				// Compute the bounce geometric term towards the blocker.
				float3 delta = P.xyz - S.xyz;
				float3 normalizedDelta = normalize(delta);			
				float unclampedBounceGeometricTerm =  max(0.0, dot(normalizedDelta, -normal)) *  max(0.0, dot(normalizedDelta, occluderNormal)) / max(dot(delta, delta), bounceSingularity);
				float bounceGeometricTerm = min(unclampedBounceGeometricTerm, bounceSingularity);
			
				// The radiance due to bounce
				float3 bounceRadiance = bounceStrength * directRadiance * bounceGeometricTerm;
			
				// Finally, add the indirect light to the light sum
				occluderRadiance += bounceRadiance * falloff;
            }
        }

    }
    return float4(occluderRadiance, ao);
}*/

float4 PixelShaderFunction(uniform bool useNormals, uniform int qualityMode, VS_OUTPUT IN) : COLOR0
{
	// Retrieve position in view space
    float3 P = fetch_eye_pos(IN.texUV);

	if (P.z > 0.95) // Ignore far geometry
		return float4(1, 1, 1, 1);

    // Project the radius of influence g_R from eye space to texture space.
    // The scaling by 0.5 is to go from [-1,1] to [0,1].
    float2 step_size = 0.5 * radius  * focalLength / P.z; // Project radius
    // Early out if the projected radius is smaller than 1 pixel.
    float numSteps = min (numberSteps, min(step_size.x * resolution.x, step_size.y * resolution.y));	// If project radius is to small...
	step_size = step_size / ( numSteps + 1 ); // real step size

    // Nearest neighbor pixels on the tangent plane
    float3 Pr, Pl, Pt, Pb;

    // I don't use face normals and the cost to have them is very high. However, the results are good and the performance is slightly better.
	float4 tangentPlane;
	float3 N = SampleNormal(IN.texUV);
	N.z = -N.z; // I'm not sure about this, but it works. I suppose that this happen because DirectX is left handed and XNA is not.
	tangentPlane = float4(N, dot(P, N)); // In view space of course.
	
	[branch]
	if (useNormals)
	{
		Pr = tangent_eye_pos(IN.texUV + float2( invResolution.x, 0),                tangentPlane, P.z);
		Pl = tangent_eye_pos(IN.texUV + float2(-invResolution.x, 0),                tangentPlane, P.z);
		Pt = tangent_eye_pos(IN.texUV + float2(0,                invResolution.y),  tangentPlane, P.z);
		Pb = tangent_eye_pos(IN.texUV + float2(0,               -invResolution.y),  tangentPlane, P.z);
	}
	else
	{
		// Doesn't use normals
		Pr = fetch_eye_pos(IN.texUV + float2( invResolution.x,  0));
		Pl = fetch_eye_pos(IN.texUV + float2(-invResolution.x,  0));
		Pt = fetch_eye_pos(IN.texUV + float2(0,                 invResolution.y));
		Pb = fetch_eye_pos(IN.texUV + float2(0,                -invResolution.y));
    }

    // Screen-aligned basis for the tangent plane
    float3 dPdu = min_diff(P, Pr, Pl);
    float3 dPdv = min_diff(P, Pt, Pb) * (resolution.y * invResolution.x);

    // (cos(alpha),sin(alpha),jitter)
    float3 rand = tex2D(RandNormal, IN.texUV * 200).rgb; // int2((int)IN.pos.x & 63, (int)IN.pos.y & 63)).rgb; This is new in shader model 4, imposible? to replicate in shader model 3.
		
	float ao = 0;
    float d;
    float alpha = 2.0f * M_PI / numberDirections; // Directions step
	/*
	[branch]
	switch (qualityMode)
	{
		case 0: // Low Quality
		{			
			for (d = 0; d < 12; ++d) // numberDirections
			{
					float angle = alpha * d;
					float2 dir = float2(cos(angle), sin(angle));
					float2 deltaUV = rotate_direction(dir, rand.xy) * step_size.xy;
					ao += AccumulatedHorizonOcclusionLowQuality(deltaUV, IN.texUV, P, numSteps, rand.z);
			}
			ao *= 2.0;			
		}
		break;
		case 1: // Middle Quality
		{			
			for (d = 0; d < 12; d++) // numberDirections
			{
				float angle = alpha * d;
				float2 dir = float2(cos(angle), sin(angle));
				float2 deltaUV = rotate_direction(dir, rand.xy) * step_size.xy;
				ao += AccumulatedHorizonOcclusionMiddleQuality(deltaUV, IN.texUV, P, numSteps, rand.z, dPdu, dPdv);
			}
		}
		break;
		default: // High Quality*/
		{
			for (d = 0; d < 12; d++) // numberDirections
			{
				float angle = alpha * d;
				float2 dir = float2(cos(angle), sin(angle));
				float2 deltaUV = rotate_direction(dir, rand.xy) * step_size.xy;
				ao += AccumulatedHorizonOcclusionHighQuality(deltaUV, IN.texUV, P, numSteps, rand.z, dPdu, dPdv, N);
			}
		}
		/*break;
	}*/
	return float4(1.0 - ao / numberDirections * contrast, 1, 1, 1);
}
/*
float4 PixelShaderFunctionRadiosity(VS_OUTPUT IN) : COLOR0
{
	// Retrieve position in view space
    float3 P = fetch_eye_pos(IN.texUV);

	if (P.z > 0.95) // Ignore far geometry
		return float4(1, 1, 1, 1);

    // Project the radius of influence g_R from eye space to texture space.
    // The scaling by 0.5 is to go from [-1,1] to [0,1].
    float2 step_size = 0.5 * radius  * focalLength / P.z; // Project radius
    // Early out if the projected radius is smaller than 1 pixel.
    float numSteps = min (numberSteps, min(step_size.x * resolution.x, step_size.y * resolution.y));	// If project radius is to small...
	step_size = step_size / ( numSteps + 1 ); // real step size

    // Nearest neighbor pixels on the tangent plane
    float3 Pr, Pl, Pt, Pb;
    
	// I don't use face normals and the cost to have them is very high. However, the results are good and the performance is slightly better.
	float4 tangentPlane;

	float3 N = SampleNormal(IN.texUV);
	N.z = -N.z; // I'm not sure about this, but it works. I suppose that this happen because DirectX is left handed and XNA is not. I'm almost sure now.

    tangentPlane = float4(N, dot(P, N)); // In view space of course.

	Pr = tangent_eye_pos(IN.texUV + float2( invResolution.x, 0),                tangentPlane, P.z);
	Pl = tangent_eye_pos(IN.texUV + float2(-invResolution.x, 0),                tangentPlane, P.z);
	Pt = tangent_eye_pos(IN.texUV + float2(0,                invResolution.y),  tangentPlane, P.z);
	Pb = tangent_eye_pos(IN.texUV + float2(0,               -invResolution.y),  tangentPlane, P.z);

	// Screen-aligned basis for the tangent plane
    float3 dPdu = min_diff(P, Pr, Pl);
    float3 dPdv = min_diff(P, Pt, Pb) * (resolution.y * invResolution.x);

    // (cos(alpha),sin(alpha),jitter)
    float3 rand = tex2D(RandNormal, IN.texUV * 200).rgb; // int2((int)IN.pos.x & 63, (int)IN.pos.y & 63)).rgb; This is new in shader model 4, imposible? to replicate in shader model 3.
		
	float4 radianceAo = 0;
    float d;
    float alpha = 2.0f * M_PI / numberDirections; // Directions step	
	
	for (d = 0; d < 10; d++) // numberDirections
	{
		float angle = alpha * d;
		float2 dir = float2(cos(angle), sin(angle));
		float2 deltaUV = rotate_direction(dir, rand.xy) * step_size.xy;
		radianceAo += AccumulatedHorizonOcclusionHighQualityWithRadiosity(deltaUV, IN.texUV, P, numSteps, rand.z, dPdu, dPdv, N);
	}
	return float4(radianceAo.rgb, 1.0 - radianceAo.a / numberDirections * contrast);
}
*/

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique LowQuality
{
    pass P0
    {          
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction(true, 0);
    }
}

technique MiddleQuality
{
    pass P0
    {          
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction(true, 1);
    }
}

technique HighQuality
{
    pass P0
    {          
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction(true, 2);
    }
}
/*
technique HighQualityRadiosity
{
    pass P0
    {          
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunctionRadiosity();
    }
}*/