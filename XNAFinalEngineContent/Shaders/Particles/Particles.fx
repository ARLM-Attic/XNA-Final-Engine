/***********************************************************************************************************************************************

Based in the ParticleEffect.fx shader of XNA Community
License: Microsoft_Permissive_License
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

#include <..\GBuffer\GBufferReader.fxh>
#include <..\Helpers\GammaLinearSpace.fxh>
#include <..\Helpers\Discard.fxh>

//////////////////////////////////////////////
//////////////// Matrices ////////////////////
//////////////////////////////////////////////

float4x4 viewProjection;
float4x4 Projection;
float4x4 ProjI;

//////////////////////////////////////////////
/////////////// Parameters ///////////////////
//////////////////////////////////////////////

float farPlane;
float2 halfPixel;
float fadeDistance;
float2 ViewportScale;

// The current time, in seconds.
float  CurrentTime;

// Parameters describing how the particles animate.
float  Duration;
float  DurationRandomness;
float3 Gravity;
float  EndVelocity;
float4 MinColor;
float4 MaxColor;

// These float2 parameters describe the min and max of a range.
// The actual value is chosen differently for each particle,
// interpolating between x and y by some random amount.
float2 RotateSpeed;
float2 StartSize;
float2 EndSize;

// How many images the texture is tiled into, in X and Y.
float2 tiles;

//////////////////////////////////////////////
///////////////// Textures ///////////////////
//////////////////////////////////////////////

texture particleTexture : register(t3);
sampler textureSampler : register(s3) = sampler_state
{
    Texture = <particleTexture>;
    /*MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;*/
};

//////////////////////////////////////////////
////////////// Data Structs //////////////////
//////////////////////////////////////////////

// Vertex shader input structure describes the start position and
// velocity of the particle, and the time at which it was created,
// along with some random values that affect its size and rotation.
struct VertexShaderInput
{
    float2 Corner   : POSITION0;
    float3 Position : POSITION1;
    float3 Velocity : NORMAL0;
    float4 Random   : COLOR0;
    float  Time     : TEXCOORD0;
};

// Vertex shader output structure specifies the position and color of the particle.
struct VertexShaderOutput
{
    float4 position          : POSITION0;
	float4 screenPosition 	 : TEXCOORD0;
	float  particleDepth     : TEXCOORD1;
    float4 color             : COLOR0;
    float2 textureCoordinate : COLOR1;
};

//////////////////////////////////////////////
///////////////// Functons ///////////////////
//////////////////////////////////////////////

// Vertex shader helper for computing the position of a particle.
float4 ComputeParticlePosition(float3 position, float3 velocity, float age, float normalizedAge)
{
    float startVelocity = length(velocity);

    // Work out how fast the particle should be moving at the end of its life,
    // by applying a constant scaling factor to its starting velocity.
    float endVelocity = startVelocity * EndVelocity;
    
    // Our particles have constant acceleration, so given a starting velocity
    // S and ending velocity E, at time T their velocity should be S + (E-S)*T.
    // The particle position is the sum of this velocity over the range 0 to T.
    // To compute the position directly, we must integrate the velocity
    // equation. Integrating S + (E-S)*T for T produces S*T + (E-S)*T*T/2.

    float velocityIntegral = startVelocity * normalizedAge +
                             (endVelocity - startVelocity) * normalizedAge *
                                                             normalizedAge / 2;
     
    position += normalize(velocity) * velocityIntegral * Duration;
    
    // Apply the gravitational force.
    position += Gravity * age * normalizedAge;
    
    // Apply the camera view and projection transforms.
    return mul(float4(position, 1), viewProjection);
} // ComputeParticlePosition

// Vertex shader helper for computing the size of a particle.
float ComputeParticleSize(float randomValue, float normalizedAge)
{
    // Apply a random factor to make each particle a slightly different size.
    float startSize = lerp(StartSize.x, StartSize.y, randomValue);
    float endSize = lerp(EndSize.x, EndSize.y, randomValue);
    
    // Compute the actual size based on the age of the particle.
    float size = lerp(startSize, endSize, normalizedAge);
    
    // Project the size into screen coordinates.
    return size * Projection._m11;
} // ComputeParticleSize

// Vertex shader helper for computing the color of a particle.
float4 ComputeParticleColor(float4 projectedPosition, float randomValue, float normalizedAge)
{
    // Apply a random factor to make each particle a slightly different color.
    float4 color = lerp(MinColor, MaxColor, randomValue);
    
    // Fade the alpha based on the age of the particle. This curve is hard coded
    // to make the particle fade in fairly quickly, then fade out more slowly:
    // plot x*(1-x)*(1-x) for x=0:1 in a graphing program if you want to see what
    // this looks like. The 6.7 scaling factor normalizes the curve so the alpha
    // will reach all the way up to fully solid.
    
    color.a *= normalizedAge * (1-normalizedAge) * (1-normalizedAge) * 6.7;
   
    return color;
} // ComputeParticleColor

// Vertex shader helper for computing the rotation of a particle.
float2x2 ComputeParticleRotation(float randomValue, float age)
{    
    // Apply a random factor to make each particle rotate at a different speed.
    float rotateSpeed = lerp(RotateSpeed.x, RotateSpeed.y, randomValue);
    
    float rotation = rotateSpeed * age;

    // Compute a 2x2 rotation matrix.
    float c = cos(rotation);
    float s = sin(rotation);
    
    return float2x2(c, -s, s, c);
} // ComputeParticleRotation

//////////////////////////////////////////////
////////////// Vertex Shader /////////////////
//////////////////////////////////////////////

// Custom vertex shader animates particles entirely on the GPU.
VertexShaderOutput ParticleVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Compute the age of the particle.
    float age = CurrentTime - input.Time;
    
    // Apply a random factor to make different particles age at different rates.
    age *= 1 + input.Random.x * DurationRandomness;
    
    // Normalize the age into the range zero to one.
    float normalizedAge = saturate(age / Duration);

    // Compute the particle position, size, color, and rotation.
    output.position = ComputeParticlePosition(input.Position, input.Velocity, age, normalizedAge);

    float size = ComputeParticleSize(input.Random.y, normalizedAge);
    float2x2 rotation = ComputeParticleRotation(input.Random.w, age);

	output.position.xy += mul(input.Corner, rotation) * size * ViewportScale;
    
    output.color = ComputeParticleColor(output.position, input.Random.z, normalizedAge);	
	output.color.rgb = GammaToLinear(output.color.rgb);

    output.textureCoordinate = (input.Corner + 1) / 2;		

	float ageTilesSpace =  normalizedAge * tiles.x * tiles.y;
	output.textureCoordinate.y = (output.textureCoordinate.y / tiles.y) + ((int)(ageTilesSpace / tiles.y) / tiles.y);
	float ageTilesSpacedivTilesY = ageTilesSpace / tiles.y;
	int selectedTiledX = (ageTilesSpacedivTilesY - (int)ageTilesSpacedivTilesY) * tiles.x;
	output.textureCoordinate.x = (output.textureCoordinate.x / tiles.x) + (selectedTiledX / tiles.x);

	output.screenPosition = output.position;
	output.particleDepth = -mul(output.position, ProjI).z / farPlane;
    
    return output;
} // ParticleVertexShader

//////////////////////////////////////////////
/////////////// Pixel Shader /////////////////
//////////////////////////////////////////////

float4 HardParticlePixelShader(VertexShaderOutput input) : COLOR0
{	
	float4 color = tex2D(textureSampler, input.textureCoordinate);
	color.rgb = GammaToLinear(color.rgb);	
	return color * input.color;
} // HardParticlePixelShader

float4 SoftParticlePixelShader(VertexShaderOutput input) : COLOR0
{	
	// Obtain screen position. You have to do this in here so that the clip-space position interpolates correctly.
    input.screenPosition.xy /= input.screenPosition.w;

	// Obtain textureCoordinates corresponding to the current pixel
	// The screen coordinates are in [-1,1]*[1,-1]
    // The texture coordinates need to be in [0,1]*[0,1]
    float2 uv = 0.5f * (float2(input.screenPosition.x, -input.screenPosition.y) + 1) + halfPixel; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/

	// Reconstruct position from the depth value, making use of the ray pointing towards the far clip plane	
	float depth = tex2D(depthSampler, uv).r;	

	float depthDiff = depth - input.particleDepth;
    if(depthDiff <= 0)
	{
        Discard();
	}

    float depthFade = saturate((fadeDistance * depthDiff) / (1 - input.particleDepth)); // I include the particle’s distance to the camera into the equation.
       
    input.color.a *= depthFade;
		
	float4 color = tex2D(textureSampler, input.textureCoordinate);
	color.rgb = GammaToLinear(color.rgb);
    return color * input.color;
} // SoftParticlePixelShader

//////////////////////////////////////////////
//////////////// Techniques //////////////////
//////////////////////////////////////////////

technique HardParticles
{
    pass P0
    {
        VertexShader = compile vs_3_0 ParticleVertexShader();
        PixelShader  = compile ps_3_0 HardParticlePixelShader();
    }
} // HardParticles

technique SoftParticles
{
    pass P0
    {
        VertexShader = compile vs_3_0 ParticleVertexShader();
        PixelShader  = compile ps_3_0 SoftParticlePixelShader();
    }
} // SoftParticles
