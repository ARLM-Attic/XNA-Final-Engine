/***********************************************************************************************************************************************

From the NVIDIA DirectX 10 SDK
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

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

float4 MiddleQualityPixelShaderFunction(VS_OUTPUT IN) : COLOR0
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
	
	//[branch]
	//if (useNormals)
	{
		Pr = tangent_eye_pos(IN.texUV + float2( invResolution.x, 0),                tangentPlane);
		Pl = tangent_eye_pos(IN.texUV + float2(-invResolution.x, 0),                tangentPlane);
		Pt = tangent_eye_pos(IN.texUV + float2(0,                invResolution.y),  tangentPlane);
		Pb = tangent_eye_pos(IN.texUV + float2(0,               -invResolution.y),  tangentPlane);
	}
	/*else
	{
		// Doesn't use normals
		Pr = fetch_eye_pos(IN.texUV + float2( invResolution.x,  0));
		Pl = fetch_eye_pos(IN.texUV + float2(-invResolution.x,  0));
		Pt = fetch_eye_pos(IN.texUV + float2(0,                 invResolution.y));
		Pb = fetch_eye_pos(IN.texUV + float2(0,                -invResolution.y));
    }*/

    // Screen-aligned basis for the tangent plane
    float3 dPdu = min_diff(P, Pr, Pl);
    float3 dPdv = min_diff(P, Pt, Pb) * (resolution.y * invResolution.x);

    // (cos(alpha),sin(alpha),jitter)
    float3 rand = tex2D(randomNormalSampler, IN.texUV * 200).rgb; // int2((int)IN.pos.x & 63, (int)IN.pos.y & 63)).rgb; This is new in shader model 4, imposible? to replicate in shader model 3.
		
	float ao = 0;
    float d;
    float alpha = 2.0f * M_PI / numberDirections; // Directions step	
	
	// Middle Quality	
	for (d = 0; d < 12; d++) // numberDirections
	{
		float angle = alpha * d;
		float2 dir = float2(cos(angle), sin(angle));
		float2 deltaUV = rotate_direction(dir, rand.xy) * step_size.xy;
		ao += AccumulatedHorizonOcclusionMiddleQuality(deltaUV, IN.texUV, P, numSteps, rand.z, dPdu, dPdv);
	}
		
	return float4(1.0 - ao / numberDirections * contrast, 1, 1, 1);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique MiddleQuality
{
    pass P0
    {          
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 MiddleQualityPixelShaderFunction();
    }
}