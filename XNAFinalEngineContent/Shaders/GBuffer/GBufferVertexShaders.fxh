/***********************************************************************************************************************************************
Copyright (c) 2008-2012, Laboratorio de Investigaci?n y Desarrollo en Visualizaci?n y Computaci?n Gr?fica - 
                         Departamento de Ciencias e Ingenier?a de la Computaci?n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

?	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

?	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

?	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
************************************************************************************************************************************************/

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 worldViewProj : register(c0); // Projection uses all 4x4 matrix values.
float4x3 worldView     : register(c4); // We could use a float4x3 matrix because the last column is trivial.
float3x3 worldViewIT   : register(c7); // No translation information so float3x3 is enough.

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float  farPlane;

// Terrain //
float2 uvRectangleMin, uvRectangleSide;    // UV Rectangle.
bool   farTerrain;                         // Is this grid a far terrain grid? A far terrain grid uses the big color texture.
float  farTerrainBeginDistance, flatRange; // Related to flatting out far surfaces.

// Optimization: if the game is in its final steps of development and performance is still an issue then you can pack the shaders attributes.
// Unfortunately the packoffset keyword is only available in shader model 4/5, so this has to be done manually and that will hurt readability.

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture displacementTexture : register(t2);
sampler2D displacementSampler : register(s2) = sampler_state
{
	Texture = <displacementTexture>;
    /*ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;*/
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

WithoutTextureVS_OUTPUT WithoutTextureVS(WithoutTextureVS_INPUT input)
{
	WithoutTextureVS_OUTPUT output;
   
	output.position = mul(input.position, worldViewProj);
	output.normalDepth.w    = -mul(input.position, worldView).z / farPlane;
	// The normals are in view space.
	output.normalDepth.xyz   = mul(input.normal, worldViewIT);

	return output;
} // WithoutTextureVS

WithTextureVS_OUTPUT WithSpecularTextureVS(WithTextureVS_INPUT input)
{
	WithTextureVS_OUTPUT output;

	output.position = mul(input.position, worldViewProj);
	output.normalDepth.w    = -mul(input.position, worldView).z / farPlane;	
	// The normals are in view space.
	output.normalDepth.xyz   = mul(input.normal, worldViewIT);
	output.uv = input.uv;

	return output;
} // WithTextureVS

WithTangentVS_OUTPUT WithNormalMapVS(WithTangentVS_INPUT input)
{
	WithTangentVS_OUTPUT output;
   
	output.position  = mul(input.position, worldViewProj);
	output.uvDepth.z = -mul(input.position, worldView).z / farPlane;
   
	// Generate the tanget space to view space matrix
	output.tangentToView[0] = mul(input.tangent,  worldViewIT);
	output.tangentToView[1] = mul(cross(input.tangent, input.normal), worldViewIT);
	output.tangentToView[2] = mul(input.normal,   worldViewIT);

	output.uvDepth.xy = input.uv;

	return output;
} // WithTangentVS

WithParallaxVS_OUTPUT WithParallaxVS(WithTangentVS_INPUT input)
{
	WithParallaxVS_OUTPUT output;
   
	output.position   = mul(input.position, worldViewProj);
	float3 positionVS = mul(input.position, worldView);
	output.uvDepth.z  = -positionVS.z / farPlane;
   
	// Generate the tanget space to view space matrix
	output.tangentToView[0] = mul(input.tangent,  worldViewIT);
	output.tangentToView[1] = mul(input.binormal, worldViewIT); // binormal = cross(input.tangent, input.normal)
	output.tangentToView[2] = mul(input.normal,   worldViewIT);

	output.viewVS = normalize(-positionVS);

	output.uvDepth.xy = input.uv;

	// Compute the ray direction for intersecting the height field profile with current view ray.

	float3 viewTS = mul(output.tangentToView, output.viewVS);
         
	// Compute initial parallax displacement direction:
	float2 parallaxDirection = normalize(viewTS.xy);
       
	// The length of this vector determines the furthest amount of displacement:
	float fLength        = length( viewTS );
	float parallaxLength = sqrt( fLength * fLength -viewTS.z * viewTS.z ) / viewTS.z; 
       
	// Compute the actual reverse parallax displacement vector:
	// Need to scale the amount of displacement to account for different height ranges in height maps.
	// This is controlled by an artist-editable parameter heightMapScale.
	output.parallaxOffsetTS = parallaxDirection * parallaxLength * heightMapScale;

	return output;
} // WithParallaxVS

/*WithTextureVS_OUTPUT TerrainVS(WithTangentVS_INPUT input)
{
	WithTextureVS_OUTPUT output = (WithTextureVS_OUTPUT)0;

	// Change the uv space from the grid to the whole terrain.
	float2 displacementUV = input.uv;
	displacementUV.y = 1 - displacementUV.y;
	displacementUV   = float2(displacementUV * uvRectangleSide + uvRectangleMin);
	displacementUV.y = 1 - displacementUV.y;
		
	input.position.y = tex2Dlod(displacementSampler, float4(displacementUV, 0, 0)) / 100;
	//position.y = LinearToGamma(tex2Dlod(displacementSampler, float4(displacementUV, 0, 0))) / 2;
	float3 distance = mul(input.position, worldView);	
	[branch]
	if (farTerrain)
	{		
		float flatLerp = saturate((distance - farTerrainBeginDistance) / flatRange);
		input.position.y = lerp(input.position.y, 0, flatLerp);
	}
	output.uv = displacementUV;

	output.position = mul(input.position, worldViewProj);	
	output.depth    = -distance.z / farPlane;
    
	return output;
} // TerrainVS*/