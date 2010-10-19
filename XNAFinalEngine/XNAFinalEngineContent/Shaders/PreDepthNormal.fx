/***********************************************************************************************************************************************
Copyright (c) 2008-2010, Laboratorio de Investigaci�n y Desarrollo en Visualizaci�n y Computaci�n Gr�fica - 
                         Departamento de Ciencias e Ingenier�a de la Computaci�n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

�	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

�	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

�	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

Author: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
************************************************************************************************************************************************/

#include <CommonDepthNormals.fxh>
//#define FX_COMPOSER  // When we are working in FX Composer

//////////////////////////////////////////////
///////////////// Script /////////////////////
//////////////////////////////////////////////

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "object";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=DepthAndNormals?HighPresicionDepth?OnlyNormals;";
	
> = 0.8; // version de SAS

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique DepthAndNormals
	<
		string Script = "Pass=p0;";
	>
{
    pass p0
	<
		string Script = "Draw=geometry;";
	>
    {
        VertexShader = compile vs_3_0 DepthNormalsVertexShaderFunction();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
#else
        CullMode = CCW;    // For The Engine	
#endif
        PixelShader = compile ps_3_0 DepthNormalsPixelShaderFunction();
    }
}

technique HighPresicionDepth
	<
		string Script = "Pass=p0;";
	>
{
    pass p0
	<
		string Script = "Draw=geometry;";
	>
    {
        VertexShader = compile vs_3_0 DepthNormalsVertexShaderFunction();
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
#else
        CullMode = CCW;    // For The Engine	
#endif
        PixelShader = compile ps_3_0 HighPresicionDepthPixelShaderFunction();
    }
}

technique OnlyNormals
	<
		string Script = "Pass=p0;";
	>
{
    pass p0
	<
		string Script = "Draw=geometry;";
	>
    {
        VertexShader = compile vs_3_0 DepthNormalsVertexShaderFunction();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
#else
        CullMode = CCW;    // For The Engine
#endif
		PixelShader = compile ps_3_0 OnlyNormalsPixelShaderFunction();
    }
}