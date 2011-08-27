/***********************************************************************************************************************************************

From AMD
Paper: Dynamic Parallax Occlusion Mapping with Approximate Soft Shadows
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

// Specifies texture dimensions for computation of mip level at render time (width, height)
float2 objectNormalTextureSize;

// The mip level id for transitioning between the full computation for parallax occlusion mapping and the bump mapping computation.
int LODThreshold;

// The minimum number of samples for sampling the height field profile
int minimumNumberSamples;

// The maximum number of samples for sampling the height field profile
int maximumNumberSamples;

// Describes the useful range of values for the height field
float heightMapScale;

//////////////////////////////////////////////
//////////////// Functions ///////////////////
//////////////////////////////////////////////

float2 CalculateParallaxUV(float2 uv, float2 parallaxOffsetTS, float3 viewVS, float3x3 tangentToView, sampler2D objectNormalSampler)
{
    // Adaptive in-shader level-of-detail system implementation. Compute the 
	// current mip level explicitly in the pixel shader and use this information 
	// to transition between different levels of detail from the full effect to 
	// simple bump mapping. See the above paper for more discussion of the approach
	// and its benefits.
		   
	// Compute the current gradients:
	float2 uvPerSize = uv * objectNormalTextureSize;

	// Compute all 4 derivatives in x and y in a single instruction to optimize:
	float2 dxSize, dySize;
	float2 dx, dy;

	float4(dxSize, dx) = ddx(float4(uvPerSize, uv));
	float4(dySize, dy) = ddy(float4(uvPerSize, uv));
                  
	float  mipLevel;      
	float  mipLevelInt;    // mip level integer portion
	float  mipLevelFrac;   // mip level fractional amount for blending in between levels

	float  minTexCoordDelta;
	float2 dTexCoords;

	// Find min of change in u and v across quad: compute du and dv magnitude across quad
	dTexCoords = dxSize * dxSize + dySize * dySize;

	// Standard mipmapping uses max here
	minTexCoordDelta = max( dTexCoords.x, dTexCoords.y );

	// Compute the current mip level  (* 0.5 is effectively computing a square root before )
	mipLevel = max( 0.5 * log2( minTexCoordDelta ), 0 );
    
	// Start the current sample located at the input texture coordinate, which would correspond
	// to computing a bump mapping result:
	float2 texSample = uv;

	if ( mipLevel <= (float) LODThreshold )
	{
		//===============================================//
		// Parallax occlusion mapping offset computation //
		//===============================================//

		// Utilize dynamic flow control to change the number of samples per ray 
		// depending on the viewing angle for the surface. Oblique angles require 
		// smaller step sizes to achieve more accurate precision for computing displacement.
		// We express the sampling rate as a linear function of the angle between 
		// the geometric normal and the view direction ray:		
		int nNumSteps = (int) lerp( maximumNumberSamples, minimumNumberSamples, dot(viewVS, tangentToView[2])); // tangentToView[2] == normalVS

		// Intersect the view ray with the height field profile along the direction of
		// the parallax offset ray (computed in the vertex shader. Note that the code is
		// designed specifically to take advantage of the dynamic flow control constructs
		// in HLSL and is very sensitive to specific syntax. When converting to other examples,
		// if still want to use dynamic flow control in the resulting assembly shader,
		// care must be applied.
		// 
		// In the below steps we approximate the height field profile as piecewise linear
		// curve. We find the pair of endpoints between which the intersection between the 
		// height field profile and the view ray is found and then compute line segment
		// intersection for the view ray and the line segment formed by the two endpoints.
		// This intersection is the displacement offset from the original texture coordinate.
		// See the above paper for more details about the process and derivation.
		//

		float fCurrHeight = 0.0;
		float fStepSize   = 1.0 / (float) nNumSteps;
		float fPrevHeight = 1.0;
		float fNextHeight = 0.0;

		int    nStepIndex = 0;
		bool   bCondition = true;
				
		float2 vTexOffsetPerStep = fStepSize * parallaxOffsetTS;
		float2 vTexCurrentOffset = uv;
		float  fCurrentBound     = 1.0;
		float  fParallaxAmount   = 0.0;

		float2 pt1 = 0;
		float2 pt2 = 0;
       
		float2 texOffset2 = 0;

		while ( nStepIndex < nNumSteps ) 
		{
			vTexCurrentOffset -= vTexOffsetPerStep;

			// Sample height map which in this case is stored in the alpha channel of the normal map:
			fCurrHeight = tex2Dgrad(objectNormalSampler, vTexCurrentOffset, dx, dy).a;

			fCurrentBound -= fStepSize;

			if ( fCurrHeight > fCurrentBound ) 
			{   
			pt1 = float2( fCurrentBound, fCurrHeight );
			pt2 = float2( fCurrentBound + fStepSize, fPrevHeight );

			texOffset2 = vTexCurrentOffset - vTexOffsetPerStep;

			nStepIndex = nNumSteps + 1;
			fPrevHeight = fCurrHeight;
			}
			else
			{
			nStepIndex++;
			fPrevHeight = fCurrHeight;
			}
		}   

		float fDelta2 = pt2.x - pt2.y;
		float fDelta1 = pt1.x - pt1.y;
      
		float fDenominator = fDelta2 - fDelta1;
      
		// SM 3.0 requires a check for divide by zero, since that operation will generate
		// an 'Inf' number instead of 0, as previous models (conveniently) did:
		if ( fDenominator == 0.0f )
		{
			fParallaxAmount = 0.0f;
		}
		else
		{
			fParallaxAmount = (pt1.x * fDelta2 - pt2.x * fDelta1 ) / fDenominator;
		}
      
		float2 vParallaxOffset = parallaxOffsetTS * (1 - fParallaxAmount );

		// The computed texture offset for the displaced point on the pseudo-extruded surface:
		float2 texSampleBase = uv - vParallaxOffset;
		texSample = texSampleBase;

		// Lerp to bump mapping only if we are in between, transition section:
		if (mipLevel > (float)(LODThreshold - 1) )
		{
			// Lerp based on the fractional part:
			mipLevelFrac = modf( mipLevel, mipLevelInt );

			// Lerp the texture coordinate from parallax occlusion mapped coordinate to bump mapping
			// smoothly based on the current mip level:
			texSample = lerp( texSampleBase, uv, mipLevelFrac);

		}   
	}
	return texSample;
} // CalculateParallaxUV