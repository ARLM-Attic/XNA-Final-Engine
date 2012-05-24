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

#include <..\Helpers\SkinningCommon.fxh>

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_OUT SkinnedWithoutTexture(in float4 position : POSITION,
	                         in float3 normal   : NORMAL,
							 in int4 indices    : BLENDINDICES0,
							 in float4 weights  : BLENDWEIGHT0)
{
	VS_OUT output = (VS_OUT)0;

	SkinTransform(position, normal, indices, weights, 4);
	output.position = mul(position, worldViewProj);
	output.postProj = output.position;
	output.normalWS = mul(normal, worldIT);
	output.viewWS   = normalize(cameraPosition - mul(position, world));
	
	return output;
} // SkinnedWithoutTexture

VS_OUT SkinnedWithTexture(in float4 position : POSITION,
	                      in float3 normal   : NORMAL,
						  in float2 uv       : TEXCOORD0,
						  in int4 indices    : BLENDINDICES0,
						  in float4 weights  : BLENDWEIGHT0)
{
	VS_OUT output = (VS_OUT)0;

	SkinTransform(position, normal, indices, weights, 4);
	output.position = mul(position, worldViewProj);
	output.postProj = output.position;	
	output.normalWS = mul(normal, worldIT);
	output.viewWS   = normalize(cameraPosition - mul(position, world));
	output.uv = uv;
	
	return output;
} // vs_mainWithTexture