/***********************************************************************************************************************************************
Copyright (c) 2008-2010, Laboratorio de Investigaci?n y Desarrollo en Visualizaci?n y Computaci?n Gr?fica - 
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

Author: Schneider, Jos? Ignacio (jis@cs.uns.edu.ar)
************************************************************************************************************************************************/

#include <Common.fxh> // Common illumination code.

//#define FX_COMPOSER  // When we are working in FX Composer

//////////////////////////////////////////////
///////////////// Script /////////////////////
//////////////////////////////////////////////

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?BlinnWithTexture:BlinnWithoutTexture:BlinnWithTextureOnlyDirectional:BlinnWithoutTextureOnlyDirectional;";
> = 0.8;

//////////////////////////////////////////////
//////////////// Surface /////////////////////
//////////////////////////////////////////////

float SpecIntensity
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 10.0;
    float UIStep = 0.01;
    string UIName = "Specular Intensity";
> = 2.0;

float SpecExponent : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 100.0;
    float UIStep = 0.01;
    string UIName = "Specular Exponent";
> = 30.0;

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct vertexOutput
{
    float4 HPosition	: POSITION;
    float2 UV			: TEXCOORD0;    
    float3 N			: TEXCOORD1;
    float3 V	        : TEXCOORD2;    
    float3 PointLightVec: TEXCOORD3;
    float3 SpotLightVec : TEXCOORD4;
};


struct vertexOutputOnlyDirectional
{
    float4 HPosition	: POSITION;
    float2 UV			: TEXCOORD0;    
    float3 N			: TEXCOORD1;
    float3 V			: TEXCOORD2;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

vertexOutput VSBlinn(appdata IN)
{	
    vertexOutput OUT;
	
	// Position
    OUT.HPosition = mul(IN.Position, WorldViewProj);		// screen clipspace coords
	
	// Normal //
    OUT.N = mul(IN.Normal, WorldIT).xyz;                   // world coordinates 
	
	// Light information
	float3 Pw = mul(IN.Position, World).xyz;		    	// world coordinates
    OUT.PointLightVec = PointLightPos - Pw;
    OUT.SpotLightVec  = SpotLightPos - Pw;
	
    // Texture coordinates
	OUT.UV = float2(UScale, VScale) * IN.UV.xy;             // Mosaic effect
	
	// Eye position - P
    OUT.V = normalize(ViewI[3].xyz - Pw);	        		// world coords
    
    return OUT;
} // VSBlinn

vertexOutputOnlyDirectional VSBlinnOnlyDirectional(appdata IN)
{
    vertexOutputOnlyDirectional OUT;
	
	// Position
    OUT.HPosition = mul(IN.Position, WorldViewProj);		// screen clipspace coords
	
	// Normal //
    OUT.N = mul(IN.Normal, WorldIT).xyz;          			// world coordinates
	
	// Light information
	float3 Pw = mul(IN.Position, World).xyz;		    	// world coordinates
	
    // Texture coordinates
	OUT.UV = float2(UScale, VScale) * IN.UV.xy;             // Mosaic effect
	
	// Eye position - P
    OUT.V = normalize(ViewI[3].xyz - Pw);	       			// world coords
    
    return OUT;
} // VSBlinnOnlyDirectional

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

half4 PSBlinn(uniform bool textured, vertexOutput IN) : COLOR
{
	float3 N  = normalize(IN.N);
    float3 V  = normalize(IN.V);
    float3 diffContrib; // = float3(0,0,0);
    float3 specContrib; // = float3(0,0,0);
    // Point Light //
    float3 L       = normalize(IN.PointLightVec);
    float3 H       = normalize(V + L);
    float  HdN     = dot(H, N);
    float  LdN     = dot(L, N);
    float4 litVec  = lit(LdN, HdN, SpecExponent);
    diffContrib    = litVec.y * PointLightColor;
    specContrib    = SpecIntensity * litVec.z * diffContrib;
    // Directional Light //
    L 		       = normalize(-DirectionalLightDir);
    H      	       = normalize(V + L);
    HdN    		   = dot(H, N);
    LdN  		   = dot(L, N);
    litVec  	   = lit(LdN, HdN, SpecExponent);
    diffContrib   += litVec.y * DirectionalLightColor;
    specContrib   += ((litVec.z * SpecIntensity) * litVec.y * DirectionalLightColor);
    // Spot Light // 
   	L			     = normalize(IN.SpotLightVec);    
    float CosSpotAng = cos(SpotLightCone*(float)(3.141592/180.0));
    float DdL        = dot(normalize(-SpotLightDir), L);
    DdL              = ((DdL - CosSpotAng)/(((float)1.0) - CosSpotAng));
    if (DdL > 0) 
    {
		H = normalize(V + L);
    	LdN = dot(L, N);    	
    	HdN = dot(H,N);
    	litVec = lit(LdN, HdN, SpecExponent);
		diffContrib +=  litVec.y * SpotLightIntensity * DdL * SpotLightColor;
    	specContrib +=  litVec.y * SpotLightIntensity * litVec.z * SpecIntensity * SpotLightColor;
    }
    // Final Color Calculations //
	float3 result;
	if (textured)
	{
    	float4 colorTex = tex2D(DiffuseSampler, IN.UV).rgba;
		result = (colorTex.a * colorTex * (diffContrib + AmbientLightColor)) + ((1 - colorTex.a) * SurfaceColor * (diffContrib + AmbientLightColor)) + specContrib;
	}
	else
	{
		result = SurfaceColor * (diffContrib + AmbientLightColor) + specContrib;    
	}
	return float4(result.rgb, AlphaBlending);
} // PSBlinn

half4 PSBlinnOnlyDirectional(uniform bool textured, vertexOutputOnlyDirectional IN) : COLOR
{
	float3 N  = normalize(IN.N);
    float3 V  = normalize(IN.V);
    float3 diffContrib; // = float3(0,0,0);
    float3 specContrib; // = float3(0,0,0);
    // Directional Light //
    float3 L 	   = normalize(-DirectionalLightDir);
    float3 H       = normalize(L);
    float HdN      = dot(H, N);
    float LdN  	   = dot(L, N);
    float4 litVec  = lit(LdN, HdN, SpecExponent);
    diffContrib    = litVec.y * DirectionalLightColor;
    specContrib    = ((litVec.z * SpecIntensity) * litVec.y * DirectionalLightColor);
    // Final Color Calculations //
	float3 result;
	if (textured)
	{
    	float3 colorTex = tex2D(DiffuseSampler, IN.UV).xyz;
		result = colorTex * (diffContrib + AmbientLightColor) + specContrib;
	}
	else
	{
		result = SurfaceColor * (diffContrib + AmbientLightColor) + specContrib;
	}	
	return float4(specContrib, 1);
    return float4(result.xyz, AlphaBlending);
} // PSBlinnOnlyDirectional

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique BlinnWithoutTexture
{
	pass P0
	{
		VertexShader = compile vs_3_0 VSBlinn();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
#else
        CullMode = CCW;    // For The Engine	
#endif
        AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;	
		PixelShader = compile ps_3_0 PSBlinn(false);
	}
}

technique BlinnWithTexture
{
	pass P0
	{
		VertexShader = compile vs_3_0 VSBlinn();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer
#else
        CullMode = CCW;    // For The Engine	
#endif
        AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;	
		PixelShader = compile ps_3_0 PSBlinn(true);
	}
}

technique BlinnWithoutTextureOnlyDirectional
{
	pass P0
	{
		VertexShader = compile vs_3_0 VSBlinnOnlyDirectional();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer	
#else
        CullMode = CCW;    // For The Engine	
#endif
        AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;	
		PixelShader = compile ps_3_0 PSBlinnOnlyDirectional(false);
	}
}

technique BlinnWithTextureOnlyDirectional
{
	pass P0
	{
		VertexShader = compile vs_3_0 VSBlinnOnlyDirectional();
		ZEnable = true;
		ZWriteEnable = true;
#ifdef FX_COMPOSER
        CullMode = None;   // For FX Composer	
#else
        CullMode = CCW;    // For The Engine	
#endif
        AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;	
		PixelShader = compile ps_3_0 PSBlinnOnlyDirectional(true);
	}
}