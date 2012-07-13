/***********************************************************************************************************************************************

From the NVIDIA DirectX 10 SDK
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

float AccumulatedHorizonOcclusionHighQuality(float2 deltaUV, 
                                             float2 uv0, // uvs for original sample
                                             float3 P, 
                                             float numSteps, 
                                             float randstep,
                                             float3 dPdu,
                                             float3 dPdv)
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

float4 HighQualityPixelShaderFunction(VS_OUTPUT input) : COLOR0
{
	float depth = tex2D(depthSampler, input.uv).r; // Single channel zbuffer texture
	if (depth == 1)
	{
		Discard();
	}
	// Retrieve position in view space
	float3 P = uv_to_eye(input.uv, depth);	

    // Project the radius of influence g_R from eye space to texture space.
    // The scaling by 0.5 is to go from [-1,1] to [0,1].
    float2 step_size = 0.5 * radius  * focalLength / P.z; // Project radius
    // Early out if the projected radius is smaller than 1 pixel.
    float numSteps = min (numberSteps, min(step_size.x * resolution.x, step_size.y * resolution.y));	// If project radius is to small...
	step_size = step_size / ( numSteps + 1 ); // real step size

    // Nearest neighbor pixels on the tangent plane
    float3 Pr, Pl, Pt, Pb;
		 
	/*[branch]
	if (useNormals)
	{
		float3 N = SampleNormal(input.uv);	
		N.z = -N.z; // I'm not sure about this, but it works. I suppose that this happen because DirectX is left handed and XNA is not	
		// I don't use face normals.
		float4 tangentPlane = float4(N, dot(normalize(P), N)); // In view space of course.

		Pr = tangent_eye_pos(input.uv + float2( invResolution.x, 0),                tangentPlane);
		Pl = tangent_eye_pos(input.uv + float2(-invResolution.x, 0),                tangentPlane);
		Pt = tangent_eye_pos(input.uv + float2(0,                invResolution.y),  tangentPlane);
		Pb = tangent_eye_pos(input.uv + float2(0,               -invResolution.y),  tangentPlane);
	}
	else*/
	{
		// Doesn't use normals
		Pr = fetch_eye_pos(input.uv + float2( invResolution.x,  0));
		Pl = fetch_eye_pos(input.uv + float2(-invResolution.x,  0));
		Pt = fetch_eye_pos(input.uv + float2(0,                 invResolution.y));
		Pb = fetch_eye_pos(input.uv + float2(0,                -invResolution.y));
    }

    // Screen-aligned basis for the tangent plane
    float3 dPdu = min_diff(P, Pr, Pl);
    float3 dPdv = min_diff(P, Pt, Pb) * (resolution.y * invResolution.x);

    // (cos(alpha),sin(alpha),jitter)
    float3 rand = tex2D(randomNormalSampler, input.uv * 20).rgb; // int2((int)IN.pos.x & 63, (int)IN.pos.y & 63)).rgb; This is new in shader model 4, imposible? to replicate in shader model 3.
		
	float ao = 0;
    float d;
    float alpha = 2.0f * M_PI / numberDirections; // Directions step		
	
	// High Quality
	for (d = 0; d < 12; d++) // numberDirections
	{
		float angle = alpha * d;
		float2 dir = float2(cos(angle), sin(angle));
		float2 deltaUV = rotate_direction(dir, rand.xy) * step_size.xy;
		ao += AccumulatedHorizonOcclusionHighQuality(deltaUV, input.uv, P, numSteps, rand.z, dPdu, dPdv);
	}
		
	float result = 1.0 - ao / numberDirections * contrast;
	return float4(result, result, result, 1);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique HighQuality
{
    pass P0
    {          
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 HighQualityPixelShaderFunction();
    }
}