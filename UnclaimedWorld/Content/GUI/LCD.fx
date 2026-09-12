float2   ViewportSize;
//float	 Alpha;

//-----------------------------------------------------------------------------
// Texture sampler
//-----------------------------------------------------------------------------

uniform const texture PanelContentTexture;
uniform const sampler PanelContentSampler : register(s0) = sampler_state
{
	Texture = (PanelContentTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap; 
	AddressV = Wrap;
};
/*
uniform const texture ReflectionTexture;
uniform const sampler ReflectionSampler : register(s2) = sampler_state
{
	Texture = (ReflectionTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp; 
	AddressV = Clamp;
};*/


uniform const texture GrungeTexture;
uniform const sampler GrungeSampler : register(s3) = sampler_state
{
	Texture = (GrungeTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap; 
	AddressV = Wrap;
};
uniform const texture CleanedTexture;
uniform const sampler CleanedSampler : register(s4) = sampler_state
{
	Texture = (CleanedTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp; 
	AddressV = Clamp;
};
/*
uniform const texture LampReflectionTexture;
uniform const sampler LampReflectionSampler : register(s5) = sampler_state
{
	Texture = (LampReflectionTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp; 
	AddressV = Clamp;
};*/
#define BlendScreenf(base, blend) 		(1.0 - ((1.0 - base) * (1.0 - blend)))
#define BlendOverlayf(base, blend) 	(base < 0.5 ? (2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend)))
#define BlendSoftLightf(base, blend) 	((blend < 0.5) ? (2.0 * base * blend + base * base * (1.0 - 2.0 * blend)) : (sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend)))

// Component wise blending
#define Blend(base, blend, funcf) 		float3(funcf(base.r, blend.r), funcf(base.g, blend.g), funcf(base.b, blend.b))

#define BlendScreen(base, blend) 		Blend(base, blend, BlendScreenf)
#define BlendOverlay(base, blend) 		Blend(base, blend, BlendOverlayf)
#define BlendSoftLight(base, blend) 	Blend(base, blend, BlendSoftLightf)

struct VertexShaderInput
{
    float3 Position : POSITION0; 
    float2 TexCoord : TEXCOORD0;   
    float2 ScaledEffectCoordinate : TEXCOORD1;
    float DrawDust : TEXCOORD2;
    float2 GrungeCoordinate : TEXCOORD3;
    float Alpha : TEXCOORD4;  
};
/*
struct VertexShaderInput
{
	float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float2 BigReflectionCoordinate : TEXCOORD1;
	float2 ScaledEffectCoordinate : TEXCOORD2;
	float DrawDust : TEXCOORD3;
	float2 GrungeCoordinate : TEXCOORD4;
	float Alpha : TEXCOORD5;
	 float UseLampReflection : TEXCOORD6;
};*/
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;	
	float2 ScaledEffectCoordinate : TEXCOORD1;
	float DrawDust : TEXCOORD2;
	float2 GrungeCoordinate : TEXCOORD3;
	float Alpha : TEXCOORD4;	
};
/*
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float2 BigReflectionCoordinate : TEXCOORD1;
    float2 ScaledEffectCoordinate : TEXCOORD2;
    float DrawDust : TEXCOORD3;
    float2 GrungeCoordinate : TEXCOORD4;
    float Alpha : TEXCOORD5;
    float UseLampReflection : TEXCOORD6;
};*/

struct PixelShaderOutput
{
   float4 Color:   COLOR0;  
};

float3 MoveToScreenSpace(VertexShaderInput input)
{	
	float3 position = input.Position;
    
	 // This was a DX 9 issue only:
    // Half pixel offset for correct texel centering.
	//position.xy -= 0.5;

	// Viewport adjustment.
	position.xy /= ViewportSize;
	position.xy *= float2(2, -2);
	position.xy -= float2(1, -1);
   
    // SCREEN SPACE:
    //output.Position = float4(position, 1);    
    
    return position;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float3 position = MoveToScreenSpace(input);	  
    
    output.Position = float4(position, 1);    
    output.TexCoord = input.TexCoord;    
   // output.BigReflectionCoordinate = input.BigReflectionCoordinate;
    output.ScaledEffectCoordinate = input.ScaledEffectCoordinate;
    output.GrungeCoordinate = input.GrungeCoordinate;
    output.DrawDust = input.DrawDust;
    output.Alpha = input.Alpha;
  //  output.UseLampReflection = input.UseLampReflection;
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{    
    float4 content = tex2D(PanelContentSampler, input.TexCoord);
     
   // float4 reflections = tex2D(ReflectionSampler, input.BigReflectionCoordinate);
   // float4 lampReflections = tex2D(LampReflectionSampler, input.BigReflectionCoordinate);
    
   // float4 grunge = tex2D(GrungeSampler, input.TruncatedEffectCoordinate);
    float4 grunge = tex2D(GrungeSampler, input.GrungeCoordinate);
    float4 cleanedWindow = tex2D(CleanedSampler, input.ScaledEffectCoordinate);
    
  
    float3 output = content.xyz;
        
    
    if (input.DrawDust)
    {
		float grungeAlpha = grunge.x * cleanedWindow.r * 0.3; //grunge.w * 0.40;
		output = Blend(output, grungeAlpha * grunge, BlendScreenf); 
    }
        

    return float4(output, input.Alpha); 
    
}

technique LCD
{
    pass Pass1
    {        
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
    }
}

/*
technique LCDFancy
{
    pass Pass1
    {       
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}*/