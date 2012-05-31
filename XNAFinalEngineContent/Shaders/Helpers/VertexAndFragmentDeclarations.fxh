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
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct WithoutTextureVS_INPUT 
{
   float4 position : POSITION;
   float3 normal   : NORMAL;
};

struct WithTextureVS_INPUT
{
   float4 position : POSITION;
   float3 normal   : NORMAL;
   float2 uv       : TEXCOORD0;
};

struct WithTangentVS_INPUT
{
   float4 position : POSITION;
   float3 normal   : NORMAL;
   float3 tangent  : TANGENT;
   // I can eliminate the binormals so that this vertex declaration fits in 32 bytes. Still, it is more fast to just pass the value.
   float3 binormal : BINORMAL; 
   float2 uv       : TEXCOORD0;
};

struct WithoutTextureVS_OUTPUT 
{
   float4 position    : POSITION0;
   float4 normalDepth : TEXCOORD0;   
};

struct WithTextureVS_OUTPUT 
{
   float4 position         : POSITION0;
   float4 normalDepth      : TEXCOORD0;  
   float2 uv			   : TEXCOORD1;
};

struct WithTangentVS_OUTPUT 
{
   float4 position         : POSITION0;
   float3 uvDepth          : TEXCOORD0;   
   float3x3 tangentToView  : TEXCOORD1;
};

struct WithParallaxVS_OUTPUT 
{
   float4 position         : POSITION0;
   float3 uvDepth          : TEXCOORD0;
   float2 parallaxOffsetTS : TEXCOORD1;
   float3 viewVS           : TEXCOORD2;
   float3x3 tangentToView  : TEXCOORD3;
};

// The type of an input to a shader is used differently than you might expect. 
// The method in which data is loaded into the registers either from a vertex buffer into a vertex
// shader or from the vertex shader output to the pixel shader input registers is well-defined
// in the Direct3D spec. That is, shader input values are always expanded into a vector of
// four floats. This means that the datatype declaration is more of a hint than a specification
// of how the data is loaded into the shader. Taking advantage of this provides a couple of
// optimization opportunities.
// A common optimization used by shader assembly writers is to take advantage of
// the way in which data is expanded when loaded into registers. For example, in vertex
// shaders, the w component will be set to 1.0 if no w component is present in the vertex
// buffer. The y and z components will be set to 0.0 if not present in the vertex buffer. The
// most common place for this to be useful is for the position in vertex shaders. It is very
// common to need the w component to be set to 1.0 when multiplying by the World matrix,
// but the vertex buffer typically only contains x, y and z components. If the position input
// parameter is declared as a float3, then an extra instruction to copy a 1.0 into the w
// component would be required. If the parameter were declared as a float4, then the w
// component would be set to 1.0f by the hardware loading the input registers. The
// compiler cannot do this type of optimization automatically since this optimization
// requires knowledge of what data is in the vertex buffer.
// Another optimization is to make sure and declare all input parameters with the
// appropriate type for their usage in the shader. For example, if the incoming data is
// integer and the data is going to be used for addressing purposes.
// declare the parameter as an int to avoid truncation. The subtle issue with declaring
// inputs as ints is that the values in the input should truly be integer values. Otherwise,
// the generated code might not run correctly due to the optimizations the compiler will
// make based upon the assumption that the input data is truly integer data.