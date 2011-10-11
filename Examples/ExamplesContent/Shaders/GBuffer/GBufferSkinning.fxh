/***********************************************************************************************************************************************
Copyright (c) 2008-2011, Laboratorio de Investigaci?n y Desarrollo en Visualizaci?n y Computaci?n Gr?fica - 
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

#define MAX_BONES   72
float4x3 Bones[MAX_BONES];

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct SkinnedWithTextureVS_INPUT
{
	float4 position : POSITION;
    float3 normal   : NORMAL;
    float2 uv       : TEXCOORD0;
    int4   indices  : BLENDINDICES0;
    float4 weights  : BLENDWEIGHT0;
};

//////////////////////////////////////////////
///////////////// Helpers ////////////////////
//////////////////////////////////////////////

void SkinTransform(inout SkinnedWithTextureVS_INPUT input, uniform int weightsPerVertex)
{
    float4x3 skinning = 0;

    [unroll]
    for (int i = 0; i < weightsPerVertex; i++)
    {
        skinning += Bones[input.indices[i]] * input.weights[i];
    }

    input.position.xyz = mul(input.position, skinning);
    input.normal = mul(input.normal, (float3x3)skinning);
}

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

WithTextureVS_OUTPUT SkinnedWithTextureVS(SkinnedWithTextureVS_INPUT input)
{
	WithTextureVS_OUTPUT output;
   
	SkinTransform(input, 4);
	output.position = mul(input.position, worldViewProj);
	output.depth    = -mul(input.position, worldView).z / farPlane;	
	output.normal   = mul(input.normal, worldViewIT); // The normals are in view space.
	output.uv = input.uv;

	return output;
} // SkinnedWithTextureVS