float2   ViewportSize;


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

uniform const texture ScanlinesTexture;
uniform const sampler ScanlinesSampler : register(s1) = sampler_state
{
	Texture = (ScanlinesTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap; 
	AddressV = Wrap;
};
/*
uniform const texture BigReflectionTexture;
uniform const sampler BigReflectionSampler : register(s2) = sampler_state
{
	Texture = (BigReflectionTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp; 
	AddressV = Clamp;
};
uniform const texture SmallReflectionTexture;
uniform const sampler SmallReflectionSampler : register(s3) = sampler_state
{
	Texture = (SmallReflectionTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp; 
	AddressV = Clamp;
};*/
uniform const texture GrungeTexture;
uniform const sampler GrungeSampler : register(s4) = sampler_state
{
	Texture = (GrungeTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};


/*
uniform const texture NoiseBackgroundTexture;
uniform const sampler NoiseBackgroundSampler : register(s4) = sampler_state
{
	Texture = (NoiseBackgroundTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};
uniform const texture BallTexture;
uniform const sampler BallSampler : register(s5) = sampler_state
{
	Texture = (BallTexture);
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
	float2 TruncatedEffectCoordinate : TEXCOORD1;
	float2 ScaledEffectCoordinate : TEXCOORD2;	
	float IsMonochrome : TEXCOORD3;
	float Alpha : TEXCOORD4;

};
/*
struct VertexShaderInput
{
    float3 Position : POSITION0; 
    float2 TexCoord : TEXCOORD0;
    float2 TruncatedEffectCoordinate : TEXCOORD1;
    float2 ScaledEffectCoordinate : TEXCOORD2;
    float2 BigReflectionCoordinate : TEXCOORD3;
    float2 SmallReflectionCoordinate : TEXCOORD4;
    float IsMonochrome : TEXCOORD5;
    float Alpha: TEXCOORD6;
    
};*/

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float2 TruncatedEffectCoordinate : TEXCOORD1;
	float2 ScaledEffectCoordinate : TEXCOORD2;
	float IsMonochrome : TEXCOORD3;
	float Alpha : TEXCOORD4;
};
/*
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float2 TruncatedEffectCoordinate : TEXCOORD1;
    float2 ScaledEffectCoordinate : TEXCOORD2;
    float2 BigReflectionCoordinate : TEXCOORD3;
    float2 SmallReflectionCoordinate : TEXCOORD4;
    float IsMonochrome : TEXCOORD5;
    float Alpha: TEXCOORD6;
};*/

struct PixelShaderOutput
{
   float4 Color:   COLOR0;  
};

float3 MoveToScreenSpace(VertexShaderInput input)
{	
	float3 position = input.Position;
    
	 // FROM SPRITEBATCH:
	 // This was a DX 9 issue only.
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
    output.TruncatedEffectCoordinate = input.TruncatedEffectCoordinate;
    output.ScaledEffectCoordinate = input.ScaledEffectCoordinate;
   /* output.BigReflectionCoordinate = input.BigReflectionCoordinate;
    output.SmallReflectionCoordinate = input.SmallReflectionCoordinate;*/
    output.IsMonochrome = input.IsMonochrome;
    output.Alpha = input.Alpha;
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input, uniform bool hiQuality) : COLOR0
{    
	/*opskrift (nedefra og op): hvor intet er angivet er blending mode: normal og opacity 100%.

	empty BG or
	image BG
	HSV adjustment layer:  lightness -52
	scanlines. blending mode: overlay. opacity 45%
	lamp reflection. blending mode: screen. op. 89%
	HSV adjustment layer:  S -100 (ingen farve)
	blue colour. (fladt farvelag med R:60 G:112 B:173).   blending mode: overlay
	grunge. blending mode: soft light.
	dropshadow
	fingerprint
	frame
	*/
	
/*	float ball = tex2D(BallSampler, input.CRTScaledEffectCoordinate).r;
	float2 distortion = (input.TexCoord - (0.5, 0.5)) * ball * 0.5;
	input.TexCoord += distortion;  // * 0.1
	input.CRTScaledEffectCoordinate += distortion;
	input.CRTTruncatedEffectCoordinate += distortion;
	*/
	
    float4 content = tex2D(PanelContentSampler, input.TexCoord);
    float3 scanlines = tex2D(ScanlinesSampler, input.TruncatedEffectCoordinate).xyz;
    
   /* float4 reflections;
    
    if (input.BigReflectionCoordinate.x > 4)
    {
		reflections = tex2D(SmallReflectionSampler, input.SmallReflectionCoordinate);    
    }
    else 
    {
		reflections = tex2D(BigReflectionSampler, input.BigReflectionCoordinate);    
    }*/
    
    
    float4 grunge = tex2D(GrungeSampler, input.ScaledEffectCoordinate);
      
    float3 output; //= content;
    
    if (input.IsMonochrome > 0)
    {
		// desaturate
		float3 grayXfer = float3(0.3, 0.59, 0.11);
		float grayf = dot(grayXfer, float3(content.x, content.y, content.z));
		//float3 gray = float3(grayf, grayf, grayf);
		 
		 // blue tint
		 // 0.23 0.44 0.68	
		//output = float4(grayf, grayf, grayf, content.w);
		output = float3(grayf, grayf, grayf); // #MONOCHANGE
    }
    else 
    {
		//output = content;
		output = content.xyz; // #MONOCHANGE
    }
    
 
    output = Blend(output, 0.6 * scanlines, BlendOverlayf); // !!!
 //   output = Blend(output, reflections.w * reflections, BlendScreenf);
    
    
	if (input.IsMonochrome > 0)
	{
		// blue tint:
		if (hiQuality)
		{
			output = Blend(output, float3(0.835, 0.8, 0.1), BlendOverlayf); //Blend(output, float3(0.235, 0.44, 0.68), BlendOverlayf);
		}
		else 
		{
			output = float3(0.5 * output.x, 0.7 * output.y, 1 * output.z); 
		}
		
    }
    
    float grungeAlpha = grunge.w * 0.2; //0.32;     
     
    output = Blend(output, grungeAlpha * grunge, BlendScreenf); //BlendSoftLightf);     
    
    
    return float4(output, input.Alpha) * input.Alpha;   
    
}

float4 PixelShaderFunctionHQTrue(VertexShaderOutput input) : COLOR0
{
	return PixelShaderFunction(input, true);
}
float4 PixelShaderFunctionHQFalse(VertexShaderOutput input) : COLOR0
{
	return PixelShaderFunction(input, false);
}

technique CRT
{
    pass Pass1
    {
        
		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ PixelShaderFunctionHQFalse();
    }
}

technique CRT_HighQuality 
{
    pass Pass1
    {
        
		VertexShader = compile vs_4_0_level_9_1 /*vs_3_0*/ VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 /*ps_3_0*/ PixelShaderFunctionHQTrue();
    }
}

technique ColorCRT
{
    pass Pass1
    {
        
		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ PixelShaderFunctionHQFalse();
    }
}