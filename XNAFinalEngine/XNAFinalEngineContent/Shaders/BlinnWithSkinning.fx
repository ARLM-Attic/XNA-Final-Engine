#define MaxBones 59
float4x4 Bones[MaxBones];

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 WorldIT       : WorldInverseTranspose <string UIWidget="None";>;
float4x4 WorldViewProj : WorldViewProjection   <string UIWidget="None";>;
float4x4 World         : World                 <string UIWidget="None";>;
float4x4 ViewI         : ViewInverse           <string UIWidget="None";>;

//////////////////////////////////////////////
///////////////// Lights /////////////////////
//////////////////////////////////////////////

// Ambient light //

float3 AmbientLightColor : SPECULAR <
	string UIName = "Ambient Light Color";
	string UIWidget = "Color";
> = {0.5f, 0.5f, 0.5f};

// Point light //

float3 PointLightPos : POSITION <
	string UIName = "Point Position";
	string Object = "PointLight";
	string Space = "World";
> = {500.0f, 500.0f, 0.0f};

float3 PointLightColor : SPECULAR <
	string UIName = "Point Light Color";
	string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f};

// Directional light //

float3 DirectionalLightDir : DIRECTION <
	string UIName = "Directional Direction";
	string Object = "DirectionalLight";
	string Space = "World";
> = {-0.65f, 0.65f, 0.39f};

float3 DirectionalLightColor : SPECULAR <
	string UIName = "Directional Light Color";
	string UIWidget = "Color";
> = {0.5f, 0.5f, 0.53f};

// Spot light //

float3 SpotLightPos : POSITION <
	string UIName = "Spot Position";
	string Object = "SpotLight";
	string Space = "World";
> = {500.18f, -510.10f, -510.12f};

float3 SpotLightDir : DIRECTION <
	string UIName = "Spot Direction";
	string Object = "SpotLight";
	string Space = "World";
> = {0.0f, -0.91f, -0.42f};

float SpotLightCone <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 90.5;
    float UIStep = 0.1;
    string UIName = "Spot Cone Angle";
> = 60.0;

float3 SpotLightColor : Specular <
	string UIName = "Spot Light Color";
	string Object = "SpotLight";
	string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f};

float SpotLightIntensity <
	string UIName = "Spot Intensity";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 2;
	float UIStep = 0.1;
> = 2;

//////////////////////////////////////////////
//////////////// Surface /////////////////////
//////////////////////////////////////////////

float AlphaBlending
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName = "Alpha Blending";
> = 0.75f;

float UScale
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 16.0;
    float UIStep = 1.0;
    string UIName = "U Texture Repeat";
> = 1.0;

float VScale
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 10.0;
    float UIStep = 1.0;
    string UIName = "V Texture Repeat";
> = 1.0;

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
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture DiffuseTexture : DIFFUSE
<
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

sampler2D DiffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

struct appdata {
    float4 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL; 
    float4 BoneIndices : BLENDINDICES0;
    float4 BoneWeights : BLENDWEIGHT0;   
};

struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV			: TEXCOORD0;    
    float3 WorldNormal	: TEXCOORD1;
    float3 WorldView	: TEXCOORD2;    
    float3 PointLightVec: TEXCOORD3;
    float3 SpotLightVec : TEXCOORD4;    
};

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

vertexOutput VertexShader(appdata IN) {
	
    vertexOutput OUT;   
    
    // Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
   
    skinTransform += Bones[IN.BoneIndices.x] * IN.BoneWeights.x;
    skinTransform += Bones[IN.BoneIndices.y] * IN.BoneWeights.y;
    skinTransform += Bones[IN.BoneIndices.z] * IN.BoneWeights.z;
    skinTransform += Bones[IN.BoneIndices.w] * IN.BoneWeights.w;
    
    // Skin the vertex position.
    //float4 Po = mul(IN.Position, skinTransform);
    // Skin the vertex normal, then compute lighting.
    //float3 Nw = normalize(mul(IN.Normal, skinTransform));
       
    float3 Nw = normalize(mul(IN.Normal,WorldIT).xyz);
    float4 Po = float4(IN.Position.xyz,1.0);	    // object coordinates
    OUT.WorldNormal = Nw;
    float3 Pw = mul(Po,World).xyz;		    	// world coordinates
    
    OUT.PointLightVec = PointLightPos - Pw;
    OUT.SpotLightVec = SpotLightPos - Pw.xyz;    
    OUT.UV = float2(UScale,VScale) * IN.UV.xy;
    
    float3 Vn = normalize(ViewI[3].xyz - Pw);	// obj coords
    OUT.WorldView = Vn;		
    OUT.HPosition = mul(Po,WorldViewProj);		// screen clipspace coords
    return OUT;
}

struct vertexOutputPrueba {
    float4 HPosition	: POSITION;
    float2 UV			: TEXCOORD0;    
};

vertexOutputPrueba VertexShaderPrueba(appdata IN)
{
    vertexOutputPrueba OUT;  
    
    // Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
   
    skinTransform += Bones[IN.BoneIndices.x] * IN.BoneWeights.x;
    skinTransform += Bones[IN.BoneIndices.y] * IN.BoneWeights.y;
    skinTransform += Bones[IN.BoneIndices.z] * IN.BoneWeights.z;
    skinTransform += Bones[IN.BoneIndices.w] * IN.BoneWeights.w;
    
    // Skin the vertex position.
    float4 Position = mul(IN.Position, skinTransform);
	       	
	//position = float4(position.xyz, 1);  // Monstruo
	       	
    OUT.HPosition = mul(Position, WorldViewProj);
    
    OUT.UV = IN.UV;
    
    return OUT;
}

float4 PixelShaderPrueba(vertexOutputPrueba IN) : COLOR0
{
    float4 color = tex2D(DiffuseSampler, IN.UV);

    //color.rgb *= input.Lighting;
    
    return color;
}

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 PixelShader(vertexOutput IN) : COLOR {
	float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldView);
    float3 diffContrib;
    float3 specContrib;
    // Point Light //
    float3 Ln = normalize(IN.PointLightVec);    
    float3 Hn = normalize(Vn + Ln);
    float hdn = dot(Hn,Nn);
    float ldn = dot(Ln,Nn);
    float4 litVec = lit(ldn,hdn,SpecExponent);
    diffContrib = litVec.y * PointLightColor;
    specContrib = SpecIntensity * litVec.z * diffContrib;
    // Directional Light //
    float3 Ln3 = DirectionalLightDir;
    float3 Hn3 = normalize(Vn + Ln3);
    float hdn3 = dot(Hn3,Nn);
    float ldn3 = dot(Ln3,Nn);
    float4 litVec3 = lit(ldn3,hdn3,SpecExponent);
    diffContrib += litVec3.y * DirectionalLightColor;
    specContrib += ((litVec3.z * SpecIntensity) * litVec3.y * DirectionalLightColor);
    // Spot Light // 
    float3 Ln2 = normalize(IN.SpotLightVec);    
    float CosSpotAng = cos(SpotLightCone*(float)(3.141592/180.0));
    float dl = dot(SpotLightDir,Ln2);
    dl = ((dl-CosSpotAng)/(((float)1.0)-CosSpotAng));
    if (dl>0) 
    {
    	float ldn2 = dot(Ln2,Nn);
    	float3 Hn2 = normalize(Vn + Ln2);
    	float hdn2 = dot(Hn2,Nn);    
    	float4 litVec2 = lit(ldn2,hdn2,SpecExponent);
    	ldn = litVec2.y * SpotLightIntensity;
		ldn *= dl;
		diffContrib += ldn * SpotLightColor;
    	specContrib += ((ldn * litVec2.z * SpecIntensity) * SpotLightColor);
    }
    // Final Color Calculations //
    float3 colorTex = tex2D(DiffuseSampler, IN.UV).xyz;
    float3 result = colorTex * (diffContrib + AmbientLightColor) + specContrib;
    return float4(result.xyz, AlphaBlending);
}

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique ModelShader30
{
	pass P0
	{
		VertexShader = compile vs_2_0 VertexShader();
		ZEnable = true;
		ZWriteEnable = true;
		//CullMode = None; // For FX Composer
		CullMode = CCW;    // For The Engine	
		PixelShader = compile ps_3_0 PixelShader();
	}
}

technique Skinning
{
	pass P0
	{
		VertexShader = compile vs_2_0 VertexShaderPrueba();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None; // For FX Composer
		//CullMode = CCW;    // For The Engine	
		PixelShader = compile ps_3_0 PixelShaderPrueba();
	}
}