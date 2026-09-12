
float2   ViewportSize;
uniform const float2   WindowPosition;

uniform const bool UseIntegerPositions;

Texture spriteSheetTexture;
sampler spriteSheetTextureSampler = sampler_state { texture = <spriteSheetTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

uniform const float AlphaAdjustment; 

#include "SpriteShader.fxh"
#include "OutlineShader.fxh"

struct VertexShaderInput
{
    float3 Position         : POSITION0;
    float2 TextureCoords    : TEXCOORD0;    
    float3 WorldPosition	: POSITION1;  
    float4 TintColor		: TEXCOORD1;    
};

struct VertexShaderOutput
{
    float4 Position			: POSITION0;
    float2 TextureCoords    : TEXCOORD0;
    float4 TintColor		: TEXCOORD1;   
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};



float3 MoveToScreenSpace(VertexShaderInput input)
{	
	float3 position = input.Position;
    
	 // FROM SPRITEBATCH:
    // Half pixel offset for correct texel centering.
   /* if (UseIntegerPositions)
    {
		position.xy -= 0.5;
	}*/

	// Viewport adjustment.
	position.xy /= ViewportSize;
	position.xy *= float2(2, -2);
	position.xy -= float2(1, -1);
   
    return position;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    if (UseIntegerPositions)
    {	// not when scrolling...       
		input.Position = round(input.Position); // Important(?) use discrete pixel positions to avoid blurryness...
    }
    
    // translate: 
	input.Position += input.WorldPosition;		  
    
    input.Position.xy -= WindowPosition;
    
    output.Position = float4(MoveToScreenSpace(input), 1);    
  //  output.Position = float4(input.Position, 1);
    
    output.TextureCoords = input.TextureCoords;
    output.TintColor = input.TintColor;
    
    return output;  
}

	
	


PixelShaderOutput PixelShaderFunction(VertexShaderOutput input, uniform const bool debugLighting) : COLOR0
{
	PixelShaderOutput Output;
   
   /* Output.Color = tex2D(spriteSheetTextureSampler, input.TextureCoords);    
    Output.Color *= input.TintColor; */
    
	float4 colorToBlend = tex2D(spriteSheetTextureSampler, input.TextureCoords);
	colorToBlend *= input.TintColor; 

	/* apply normal map lighting: */
	colorToBlend = ApplyLighting(colorToBlend, input.TextureCoords, debugLighting, colorToBlend.w);

	Output.Color = colorToBlend;

    return Output;
}

PixelShaderOutput PixelShaderOutlineFunction(VertexShaderOutput input) //: COLOR0
{
    // Look up the texture and normalmap values.
	PixelShaderOutput output;

	//sampler texSampler = spriteSheetTextureSampler;
	//float2 texc = input.TextureCoords;
	//float4 Color = GetOutlineColor(texSampler, texc, input.TintColor);

	// #MONOCHANGE
	float2 texc = input.TextureCoords;
	float4 Color = GetOutlineColor(spriteSheetTextureSampler, texc, input.TintColor);

	Color.a *= input.TintColor.a;
	Color.rgb *= input.TintColor.a;
	 if(Color.a == 0)
	 {
		discard;
	 }
	 output.Color = Color;
	return output;

}

PixelShaderOutput PixelShaderFunctionFalse(VertexShaderOutput input) //: COLOR0
{
	return PixelShaderFunction(input, false);
}
technique RenderGroundSprites
{
    pass Pass1
    {        
		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunctionFalse(); // PixelShaderFunction(false);
    }
}

technique RenderOutlineGroundSprites
{
    pass Pass1
    {        
		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ PixelShaderOutlineFunction();
    }
}

PixelShaderOutput PixelShaderFunctionTrue(VertexShaderOutput input)// : COLOR0
{
	return PixelShaderFunction(input, true);
}
technique RenderGroundSpritesDebugLighting
{
    pass Pass1
    {        
		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ PixelShaderFunctionTrue(); // PixelShaderFunction(true);
    }
}