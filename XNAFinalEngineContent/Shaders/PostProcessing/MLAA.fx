/***********************************************************************************************************************************************
  Copyright (C) 2010 Jorge Jimenez (jim@unizar.es)
  Copyright (C) 2010 Belen Masia (bmasia@unizar.es)
  Copyright (C) 2010 Jose I. Echevarria (joseignacioechevarria@gmail.com)
  Copyright (C) 2010 Fernando Navarro (fernandn@microsoft.com)
  Copyright (C) 2010 Diego Gutierrez (diegog@unizar.es)
  All rights reserved.
 
  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions are met:
 
  1. Redistributions of source code must retain the above copyright notice,
     this list of conditions and the following disclaimer.
 
  2. Redistributions in binary form must display the names 'Jorge Jimenez',
     'Belen Masia', 'Jose I. Echevarria', 'Fernando Navarro' and 'Diego
     Gutierrez' as 'Real-Time Rendering R&D' in the credits of the
     application, if such credits exist. The authors of this work must be
     notified via email (jim@unizar.es) in this case of redistribution.
 
  3. Neither the name of copyright holders nor the names of its contributors
     may be used to endorse or promote products derived from this software
     without specific prior written permission.
 
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ``AS
  IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
  PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL COPYRIGHT HOLDERS OR CONTRIBUTORS
  BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
  CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
  SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
  INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
  CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
  POSSIBILITY OF SUCH DAMAGE.

 Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

#include <..\Helpers\Discard.fxh>

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float2 halfPixel; // -1f / sceneTexture.Width, 1f / sceneTexture.Height

float2 pixelSize; // 1 / sceneTexture.Width, 1 / sceneTexture.Height

float thresholdColor;

float thresholdDepth;

#define MAX_SEARCH_STEPS 6
//const int maxSearchSteps = 12;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture sceneTexture : register(t11);
sampler2D sceneSampler : register(s10) = sampler_state
{
	Texture = <sceneTexture>;
	/*MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;*/
};
sampler2D sceneLinearSampler : register(s11) = sampler_state
{
	Texture = <sceneTexture>;
	/*MipFilter = NONE;
	MagFilter = LINEAR;
	MinFilter = LINEAR;*/
};

texture edgeTexture : register(t12);
sampler2D edgeSampler : register(s12) = sampler_state
{
	Texture = <edgeTexture>;
	/*MipFilter = NONE;
	MagFilter = LINEAR;
	MinFilter = LINEAR;*/
};

texture blendedWeightsTexture : register(t13);
sampler2D blendedWeightsSampler : register(s13) = sampler_state
{
	Texture = <blendedWeightsTexture>;
	/*MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;*/
};

texture depthTexture : register(t14);
sampler2D depthSampler : register(s14) = sampler_state
{
	Texture = <depthTexture>;
	/*MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;*/
};

texture areaTexture : register(t15);
sampler2D areaSampler : register(s15) = sampler_state
{
	Texture = <areaTexture>;
	/*MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;*/
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct VS_OUT
{
	float4 position		: POSITION;
	float2 uv			: TEXCOORD0;
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

VS_OUT vs_main(in float4 position : POSITION, in float2 uv : TEXCOORD)
{
	VS_OUT output = (VS_OUT)0;
	
	output.position = position;
	output.position.xy += halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	output.uv = uv; 
	
	return output;
} // vs_main

//////////////////////////////////////////////
////////////// Edge Detection ////////////////
//////////////////////////////////////////////

float4 EdgeDetectionColorPS(in float2 uv : TEXCOORD0) : COLOR0
{
    float3 weights = float3(0.2126,0.7152, 0.0722);

    float L        = dot(tex2D(sceneSampler, uv).rgb, weights);
    float Lleft    = dot(tex2D(sceneSampler, uv + float2(-pixelSize.x, 0)).rgb, weights);
    float Ltop     = dot(tex2D(sceneSampler, uv + float2(0, -pixelSize.y)).rgb, weights);
    //float Lright   = dot(tex2D(sceneSampler, uv + float2(pixelSize.x, 0)).rgb,  weights);
    //float Lbottom  = dot(tex2D(sceneSampler, uv + float2(0, pixelSize.y)).rgb,  weights);

    float2 delta = abs(L.xx - float2(Lleft, Ltop));//, Lright, Lbottom));
    float2 edges = step(thresholdColor.xx, delta);
	/*
    if (dot(edges, 1.0) == 0.0) // if there is no edge then discard.
	{
        discard; // if-discard is almost the same than return, performance wise of course.
    }*/

    return float4(edges, 0, 0);
} // EdgeDetectionColorPS

float4 EdgeDetectionDepthPS(in float2 uv : TEXCOORD0) : COLOR0
{
    float D       = tex2D(depthSampler, uv).r;
    float Dleft   = tex2D(depthSampler, uv + float2(-pixelSize.x, 0)).r;
    float Dtop    = tex2D(depthSampler, uv + float2(0, -pixelSize.y)).r;
    //float Dright  = tex2D(depthSampler, uv + float2(pixelSize.x, 0)).r;
    //float Dbottom = tex2D(depthSampler, uv + float2(0, pixelSize.y)).r;

    float2 delta = abs(D.xx - float2(Dleft, Dtop));//, Dright, Dbottom));
    // Dividing by 10 give us results similar to the color-based detection
    float2 edges = step(thresholdDepth.xx / 10.0, delta); // step(y, x) (x >= y) ? 1 : 0   In other words, is 1 if delta is greater than the threshold.
	/*
    if (dot(edges, 1.0) == 0.0) // if there is no edge then discard.
	{
        discard; // if-discard is almost the same than return, performance wise of course.
    }*/

    return float4(edges, 0, 0);
} // EdgeDetectionDepthPS

float4 EdgeDetectionColorDepthPS(in float2 uv : TEXCOORD0) : COLOR0
{
    float3 weights = float3(0.2126,0.7152, 0.0722);

    float L        = dot(tex2D(sceneSampler, uv).rgb, weights);
    float Lleft    = dot(tex2D(sceneSampler, uv + float2(-pixelSize.x, 0)).rgb, weights);
    float Ltop     = dot(tex2D(sceneSampler, uv + float2(0, -pixelSize.y)).rgb, weights);
    //float Lright   = dot(tex2D(sceneSampler, uv + float2(pixelSize.x, 0)).rgb,  weights);
    //float Lbottom  = dot(tex2D(sceneSampler, uv + float2(0, pixelSize.y)).rgb,  weights);

    float2 delta = abs(L.xx - float2(Lleft, Ltop));//, Lright, Lbottom));
    float2 edgescolor = step(thresholdColor.xx, delta);
	
    float D       = tex2D(depthSampler, uv).r;
    float Dleft   = tex2D(depthSampler, uv + float2(-pixelSize.x, 0)).r;
    float Dtop    = tex2D(depthSampler, uv + float2(0, -pixelSize.y)).r;
    //float Dright  = tex2D(depthSampler, uv + float2(pixelSize.x, 0)).r;
    //float Dbottom = tex2D(depthSampler, uv + float2(0, pixelSize.y)).r;

    delta = abs(D.xx - float2(Dleft, Dtop));//, Dright, Dbottom));
    // Dividing by 10 give us results similar to the color-based detection
    float2 edgesdepth = step(thresholdDepth.xx / 10.0, delta); // step(y, x) (x >= y) ? 1 : 0   In other words, is 1 if delta is greater than the threshold.
	
    return float4(edgescolor + edgesdepth, 0, 0);
} // EdgeDetectionDepthPS

//////////////////////////////////////////////
////////////// Blending Weight ///////////////
//////////////////////////////////////////////

float SearchXLeft(float2 texcoord)
{
    texcoord -= float2(1.5, 0.0) * pixelSize;
    float e = 0.0;
    // We offset by 0.5 to sample between edgels, thus fetching two in a row	
	[unroll(MAX_SEARCH_STEPS)]
    for (int i = 0; i < MAX_SEARCH_STEPS; i++)
	{
        e = tex2Dlod(edgeSampler, float4(texcoord, 0, 0)).g;
        // We compare with 0.9 to prevent bilinear access precision problems		
		[flatten]
		if (e < 0.9)
			break;
        texcoord -= float2(2.0, 0.0) * pixelSize;
    }
    // When we exit the loop without founding the end, we want to return
    // -2 * maxSearchSteps
    return max(-2.0 * i - 2.0 * e, -2.0 * MAX_SEARCH_STEPS);
}

float SearchXRight(float2 texcoord)
{
    texcoord += float2(1.5, 0.0) * pixelSize;
    float e = 0.0;	
	[unroll(MAX_SEARCH_STEPS)]
    for (int i = 0; i < MAX_SEARCH_STEPS; i++)
	{
        e = tex2Dlod(edgeSampler, float4(texcoord, 0, 0)).g;
		[flatten]
		if (e < 0.9)
			break;
        texcoord += float2(2.0, 0.0) * pixelSize;
    }
    return min(2.0 * i + 2.0 * e, 2.0 * MAX_SEARCH_STEPS);
}

float SearchYUp(float2 texcoord)
{
    texcoord -= float2(0.0, 1.5) * pixelSize;
    float e = 0.0;	
	[unroll(MAX_SEARCH_STEPS)]
    for (int i = 0; i < MAX_SEARCH_STEPS; i++)
	{
        e = tex2Dlod(edgeSampler, float4(texcoord, 0, 0)).r;        
		[flatten]
		if (e < 0.9)
			break;
        texcoord -= float2(0.0, 2.0) * pixelSize;
    }
    return max(-2.0 * i - 2.0 * e, -2.0 * MAX_SEARCH_STEPS);
}

float SearchYDown(float2 texcoord)
{
    texcoord += float2(0.0, 1.5) * pixelSize;
    float e = 0.0;
	[unroll(MAX_SEARCH_STEPS)]
    for (int i = 0; i < MAX_SEARCH_STEPS; i++)
	{
        e = tex2Dlod(edgeSampler, float4(texcoord, 0, 0)).r;
		[flatten]
		if (e < 0.9)
			break;
        texcoord += float2(0.0, 2.0) * pixelSize;
    }
    return min(2.0 * i + 2.0 * e, 2.0 * MAX_SEARCH_STEPS);
}

/*
float4 mad(float4 m, float4 a, float4 b)
{
    return m * a + b;
}*/

float4 mad(float4 m, float4 a, float4 b)
{
    #if defined(XBOX)
		float4 result;
		asm
		{
			mad result, m, a, b
		};
		return result;
    #else
		return m * a + b;
    #endif
}

#define NUM_DISTANCES 32
#define AREA_SIZE (NUM_DISTANCES * 5)

float2 Area(float2 distance, float e1, float e2) {
     // * By dividing by AREA_SIZE - 1.0 below we are implicitely offsetting to
     //   always fall inside of a pixel
     // * Rounding prevents bilinear access precision problems
    float2 pixcoord = NUM_DISTANCES * round(4.0 * float2(e1, e2)) + distance;
    float2 texcoord = pixcoord / (AREA_SIZE - 1.0);   
	return tex2Dlod(areaSampler, float4(texcoord, 0, 0)).rg;
}

float4 BlendingWeightCalculationPS(in float2 texcoord : TEXCOORD0) : COLOR0
{
    float4 weights = 0.0;

    float2 e = tex2D(edgeSampler, texcoord).rg;
	if (dot(e, 1.0) == 0.0) // if there is no edge then discard.
	{		
        Discard();
    }
	
    [branch]
    if (e.g) // Edge at north
	{
        float2 d = float2(SearchXLeft(texcoord), SearchXRight(texcoord));
        
        // Instead of sampling between edgels, we sample at -0.25,
        // to be able to discern what value each edgel has.
        float4 coords = mad(float4(d.x, -0.25, d.y + 1.0, -0.25),
                            pixelSize.xyxy, texcoord.xyxy);
        float e1 = tex2Dlod(edgeSampler, float4(coords.xy, 0, 0)).r;
        float e2 = tex2Dlod(edgeSampler, float4(coords.zw, 0, 0)).r;
        weights.rg = Area(abs(d), e1, e2);
    }
	
    [branch]
    if (e.r) // Edge at west
	{ 
        float2 d = float2(SearchYUp(texcoord), SearchYDown(texcoord));		
		
        float4 coords = mad(float4(-0.25, d.x, -0.25, d.y + 1.0),
                            pixelSize.xyxy, texcoord.xyxy);
        float e1 = tex2Dlod(edgeSampler, float4(coords.xy, 0, 0)).g;
        float e2 = tex2Dlod(edgeSampler, float4(coords.zw, 0, 0)).g;
        weights.ba = Area(abs(d), e1, e2);
    }
	
    return weights;
}

//////////////////////////////////////////////
/////////// Neighborhood Blending ////////////
//////////////////////////////////////////////

float4 NeighborhoodBlendingPS(in float2 texcoord : TEXCOORD0) : COLOR0
{	
    float2 topLeft = tex2D(blendedWeightsSampler, texcoord).rb;
    float right = tex2D(blendedWeightsSampler, texcoord + float2(0, pixelSize.y)).g;
    float bottom = tex2D(blendedWeightsSampler, texcoord + float2(pixelSize.x, 0)).a;
    float4 a = float4(topLeft.x, right, topLeft.y, bottom);
		
    float sum = dot(a, 1.0);
	
    [branch]
    if (sum > 0.0)
	{		
        float4 o = a * pixelSize.yyxx;
        float4 color = 0.0;
        color = mad(tex2Dlod(sceneLinearSampler, float4(texcoord + float2( 0.0, -o.r), 0, 0)), a.r, color);
        color = mad(tex2Dlod(sceneLinearSampler, float4(texcoord + float2( 0.0,  o.g), 0, 0)), a.g, color);
        color = mad(tex2Dlod(sceneLinearSampler, float4(texcoord + float2(-o.b,  0.0), 0, 0)), a.b, color);
        color = mad(tex2Dlod(sceneLinearSampler, float4(texcoord + float2( o.a,  0.0), 0, 0)), a.a, color);
        return color / sum;
    }
	else
	{	
        return tex2D(sceneSampler, texcoord);
    }
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique EdgeDetectionColor
{
	pass EdgeDetection
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 EdgeDetectionColorPS();
	}
	pass BlendingWeight
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 BlendingWeightCalculationPS();
	}
	pass NeighborhoodBlending
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 NeighborhoodBlendingPS();
	}
} // EdgeDetectionColor

technique EdgeDetectionDepth
{
	pass EdgeDetection
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 EdgeDetectionDepthPS();
	}
	pass BlendingWeight
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 BlendingWeightCalculationPS();
	}
	pass NeighborhoodBlending
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 NeighborhoodBlendingPS();
	}
} // EdgeDetectionDepth

technique EdgeDetectionColorDepth
{
	pass EdgeDetection
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 EdgeDetectionColorDepthPS();
	}
	pass BlendingWeight
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 BlendingWeightCalculationPS();
	}
	pass NeighborhoodBlending
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader  = compile ps_3_0 NeighborhoodBlendingPS();
	}
} // EdgeDetectionColorDepth