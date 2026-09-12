//----------------------------------------------------
//--                                                --
//--             www.riemers.net                 --
//--         Series 4: Advanced terrain             --
//--                 Shader code                    --
//--                                                --
//----------------------------------------------------

//------- Constants --------
/*
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
*/


float3 xLightDirection;

float4 ClipPlane0; // NEW XNA 4


// contains parameters too, like NormalMap:
#include "SpriteShader.fxh"

// NEW:
float2   ViewportSize; 
uniform const float2   WindowPosition;
float   NearPlane;
float   FarPlane;
float	ZOffset;

uniform const bool DoClipping;

uniform const bool UseIntegerPositions;

//uniform const float terrain1Noise;
//uniform const float terrain2Noise;
uniform const float terrain3Noise;

//------- Texture Samplers --------
Texture texture0;
sampler TextureSampler0 = sampler_state { texture = <texture0> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture texture1;
sampler TextureSampler1 = sampler_state { texture = <texture1> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture texture2;
sampler TextureSampler2 = sampler_state { texture = <texture2> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture texture3;
sampler TextureSampler3 = sampler_state { texture = <texture3> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture texture4;
sampler TextureSampler4 = sampler_state { texture = <texture4> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture perlinTexture;
sampler PerlinSampler = sampler_state { texture = <perlinTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};



//Texture perlinBigTexture;
//sampler PerlinBigSampler = sampler_state { texture = <perlinBigTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};


struct VertexShaderInput
{
    float3 inPos : POSITION0;    
    float2 inTexCoords: TEXCOORD0;    
    float4 inTexWeights: TEXCOORD1;    
	float4 tintColor0: TEXCOORD2;    
	float4 tintColor1: TEXCOORD3;    
	float4 tintColor2: TEXCOORD4;   
	
	//float4 NoiseChannelToUse    : TEXCOORD5;    
	float4 AlphaSharpness		: TEXCOORD5; 
	float4 NoiseScaling			: TEXCOORD6; 
};

struct VertexShaderOutput
{
	float4 Position         : POSITION0;   
        
    float2 TextureCoords    : TEXCOORD0;   
    float4 TextureWeights    : TEXCOORD1;   
    
    float4 TintColor0       : TEXCOORD2;   
    float4 TintColor1       : TEXCOORD3;   
    float4 TintColor2       : TEXCOORD4;   
          
    float4 AlphaSharpness	 : TEXCOORD5;  
    float4 NoiseScaling			: TEXCOORD6;   

	float4 ClipDistances : TEXCOORD7; // was: #MONOCHANGE
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};


float3 MoveToScreenSpace(float3 position)
{	
	if (UseIntegerPositions)
	{
		position = round(position); // use discrete pixel positions to avoid blurryness...
    
		// FROM SPRITEBATCH:
		// Half pixel offset for correct texel centering.	
		position.xy -= 0.5;
	}
	
	// Viewport adjustment.
	position.xy /= ViewportSize;
	position.xy *= float2(2, -2);
	position.xy -= float2(1, -1);
   
     
    
    return position;
}

// OBS!!! float3 for Position!
VertexShaderOutput MultiTexturedVS(VertexShaderInput input)
{   	
    
    VertexShaderOutput Output = (VertexShaderOutput)0;
	
    
    // translate
    float3 position = float3(input.inPos.xyz); 
    
	float4 test = float4(input.inPos,1);
	Output.ClipDistances = dot(test, ClipPlane0);  // was:  #MONOCHANGE //MSS - Water Refactor added
	

	position.xy -= WindowPosition;
    
	// VERY Important! use discrete pixel positions to avoid blurryness...
    position = MoveToScreenSpace(position);	  
    
    //position.z = oldStylePosition.z;
    Output.Position = float4(position.xy, (input.inPos.z + ZOffset)/(FarPlane - NearPlane), 1); 
    
    
    Output.TextureCoords = input.inTexCoords;
    Output.TextureWeights = input.inTexWeights;
    
    Output.TintColor0 = input.tintColor0;    
    Output.TintColor1 = input.tintColor1;
    Output.TintColor2 = input.tintColor2;
//    Output.TintColor3 = input.tintColor3;
    
  //  Output.NoiseChannelToUse = input.NoiseChannelToUse;
    Output.AlphaSharpness = input.AlphaSharpness;
    Output.NoiseScaling = input.NoiseScaling;
    
    return Output;    
}



float4 BlendTexture(float weight, float4 baseColor, float4 colorToBlend, float4 tintColor, float alphaNoise, bool blendWithBase)
{
	float alpha;
	
	
	weight *= colorToBlend.w; // translucent textures!
    
	// this gives 0 noise when weight is 0 or 1:
	alpha = saturate(weight + (0.5 - abs(0.5 - weight)) * alphaNoise);	
		
			  
    
   // alpha = smoothstep(0, 1, alpha);

	
	colorToBlend *= tintColor;
	if (blendWithBase)
	{
		//alpha = alpha colorToBlend.w
		// blend with the previous texture in this batch:
		return (1 - alpha) * baseColor + colorToBlend * alpha;
	}
	else 
	{	// we are first in the batch, so blend with the render target using alpha/weight:
		colorToBlend.w = alpha;
		return colorToBlend;
	}

}

float CalculateAlpha(const float weight, const float alphaNoise, const float textureAlpha)
{
	float alpha;	    
	
	alpha = saturate(textureAlpha * (weight + (0.5 - abs(0.5 - weight)) * alphaNoise));	
		
	return alpha;
}

float4 BlendTexture_PreMultipliedAlpha(float weight, float4 baseColor, float4 colorToBlend, float4 tintColor, float alphaNoise, bool blendWithBase)
{
	float alpha = CalculateAlpha(weight, alphaNoise, colorToBlend.w);
	    
	// this gives 0 noise when weight is 0 or 1:
	//alpha = saturate(weight + (0.5 - abs(0.5 - weight)) * alphaNoise);	
	
	//alpha *= colorToBlend.w; // translucent textures!	
	//alpha = smoothstep(0, 1, alpha); // necessary?
	
	
	colorToBlend *= tintColor;
	if (blendWithBase)
	{
		// alpha = alpha colorToBlend.w
		// blend with the previous texture in this batch:
		return (1 - alpha) * baseColor + alpha * colorToBlend; // premultiply - no change???!!!!
	}
	else 
	{	// we are first in the batch, so blend with the render target using alpha/weight:
		colorToBlend *= alpha; // premultiply!!!!
		colorToBlend.w = alpha; // this makes no difference...?
		return colorToBlend;
	}

}

//  AlphaSharpness can be negative. This will invert the noise.
float GetAlphaX(const VertexShaderOutput PSIn, const float noiseScaling, const float alphaSharpness)
{	
	return alphaSharpness * (0.5f - tex2D(PerlinSampler, noiseScaling * PSIn.TextureCoords).x);
}
float GetAlphaY(const VertexShaderOutput PSIn, const float noiseScaling, const float alphaSharpness)
{	
	return alphaSharpness * (0.5f - tex2D(PerlinSampler, noiseScaling * PSIn.TextureCoords).y);
}
float GetAlphaZ(const VertexShaderOutput PSIn, const float noiseScaling, const float alphaSharpness)
{	
	return alphaSharpness * (0.5f - tex2D(PerlinSampler, noiseScaling * PSIn.TextureCoords).z);
}


//**********************************

PixelShaderOutput MultiTexturedPS(const VertexShaderOutput PSIn, uniform const float terrain1Noise, uniform const float terrain2Noise) 
{
	PixelShaderOutput Output = (PixelShaderOutput)0;
	/*
	Output.Color.a = 0.2;
	Output.Color.g = 1;
	return Output;*/

	
	
	if (DoClipping)
	{
		clip(PSIn.ClipDistances);  // was: #MONOCHANGE

		Output.Color = 1;
		return Output;
	}

		
	
	float alphaNoise1 = 0;
		
	if (terrain1Noise == 0){
		alphaNoise1 = GetAlphaX(PSIn, PSIn.NoiseScaling.x, PSIn.AlphaSharpness.x);	
	}
	else if (terrain1Noise == 1){
		alphaNoise1 = GetAlphaY(PSIn, PSIn.NoiseScaling.x, PSIn.AlphaSharpness.x);	
	}
	else if(terrain1Noise == 2){
		alphaNoise1 = GetAlphaZ(PSIn, PSIn.NoiseScaling.x, PSIn.AlphaSharpness.x);	
	}


	Output.Color = BlendTexture_PreMultipliedAlpha(PSIn.TextureWeights.x, Output.Color, tex2D(TextureSampler0, PSIn.TextureCoords), PSIn.TintColor0, alphaNoise1, false);

	float alphaNoise2 = 0;
	
	if (terrain2Noise != -1.0f) // onlyOneTerrainInBatch == false)
	{	
		if (terrain2Noise == 0){
			alphaNoise2 = GetAlphaX(PSIn, PSIn.NoiseScaling.y, PSIn.AlphaSharpness.y);	
		}
		else if (terrain2Noise == 1){
			alphaNoise2 = GetAlphaY(PSIn, PSIn.NoiseScaling.y, PSIn.AlphaSharpness.y);	
		}
		else if(terrain2Noise == 2){
			alphaNoise2 = GetAlphaZ(PSIn, PSIn.NoiseScaling.y, PSIn.AlphaSharpness.y);	
		}
		
		Output.Color = BlendTexture_PreMultipliedAlpha(PSIn.TextureWeights.y, Output.Color, tex2D(TextureSampler1, PSIn.TextureCoords), PSIn.TintColor1, alphaNoise2, true);
	    
		
		float alphaNoise3 = 0;
		
		if (terrain3Noise == 0){
			alphaNoise3 = GetAlphaX(PSIn, PSIn.NoiseScaling.z, PSIn.AlphaSharpness.z);	
		}
		else if (terrain3Noise == 1){
			alphaNoise3 = GetAlphaY(PSIn, PSIn.NoiseScaling.z, PSIn.AlphaSharpness.z);	
		}
		else if(terrain3Noise == 2){
			alphaNoise3 = GetAlphaZ(PSIn, PSIn.NoiseScaling.z, PSIn.AlphaSharpness.z);	
		}
		
		Output.Color = BlendTexture_PreMultipliedAlpha(PSIn.TextureWeights.z, Output.Color, tex2D(TextureSampler2, PSIn.TextureCoords), PSIn.TintColor2, alphaNoise3, true);
	
	}
	
	return Output;

	
}

PixelShaderOutput RenderRocksPS(const VertexShaderOutput PSIn, uniform const float terrain1Noise, uniform const bool debugLighting) 
{
	

	PixelShaderOutput Output = (PixelShaderOutput)0;          
    
	if (DoClipping) 
	{
		clip(PSIn.ClipDistances);  //was: #MONOCHANGE
		Output.Color = 1;
			return Output;	
	}

	float alphaNoise1 = 0;
	//alphaNoise1 = GetAlphaZ(PSIn, PSIn.NoiseScaling.x, PSIn.AlphaSharpness.x);	


	if (terrain1Noise == 0){
		alphaNoise1 = GetAlphaX(PSIn, PSIn.NoiseScaling.x, PSIn.AlphaSharpness.x);	
	}
	else if (terrain1Noise == 1){
		alphaNoise1 = GetAlphaY(PSIn, PSIn.NoiseScaling.x, PSIn.AlphaSharpness.x);	
	}
	else if(terrain1Noise == 2){
		alphaNoise1 = GetAlphaZ(PSIn, PSIn.NoiseScaling.x, PSIn.AlphaSharpness.x);	
	}

	
	float4 colorToBlend = tex2D(TextureSampler0, PSIn.TextureCoords);
	
	float weight = PSIn.TextureWeights.x;
	
	float alpha = CalculateAlpha(weight, alphaNoise1, colorToBlend.w);	    
		
	colorToBlend *= PSIn.TintColor0;
	
	/* apply normal map lighting: */
	colorToBlend = ApplyLighting(colorToBlend, PSIn.TextureCoords, debugLighting, 1);
		
	
	// we are only ones in the batch, so blend with the render target using alpha/weight:
	colorToBlend *= alpha; // premultiply!!!!
	colorToBlend.w = alpha;
	
	Output.Color = colorToBlend;
	return Output;

}

//****************** TECHNIQUES *****************************

// for PIX debugging - optimizations disabled:

//technique RenderRocksSingleLayer0
//{
//    pass Pass0
//    {
//        VertexShader = compile vs_3_0 NEWMultiTexturedVS(); 
//        PixelShader = compile ps_3_0 RenderRocksPS(0.0f, false);        
//    }
//}
//technique RenderRocksSingleLayer1
//{
//    pass Pass0
//    {
//        VertexShader = compile vs_3_0 NEWMultiTexturedVS(); 
//        PixelShader = compile ps_3_0 RenderRocksPS(1.0f, false);        
//    }
//}
//technique RenderRocksSingleLayer2
//{
//    pass Pass0
//    {
//        VertexShader = compile vs_3_0 NEWMultiTexturedVS(); 
//        PixelShader = compile ps_3_0 RenderRocksPS(2.0f, false);        
//    }
//}

PixelShaderOutput RenderRocksPS0False(const VertexShaderOutput PSIn)
{
	return RenderRocksPS(PSIn, 0.0f, false);
}
technique RenderRocksSingleLayer0
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 RenderRocksPS0False(); // RenderRocksPS(0.0f, false);
    }
}

PixelShaderOutput RenderRocksPS1False(const VertexShaderOutput PSIn)
{
	return RenderRocksPS(PSIn, 1.0f, false);
}
technique RenderRocksSingleLayer1
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 RenderRocksPS1False(); // RenderRocksPS(1.0f, false);
    }
}

PixelShaderOutput RenderRocksPS2False(const VertexShaderOutput PSIn)
{
	return RenderRocksPS(PSIn, 2.0f, false);
}
technique RenderRocksSingleLayer2
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 RenderRocksPS2False(); // RenderRocksPS(2.0f, false);
    }
}


// For lighting only:
PixelShaderOutput RenderRocksPS0True(const VertexShaderOutput PSIn)
{
	return RenderRocksPS(PSIn, 0.0f, true);
}
technique DebugRenderRocksSingleLayer0
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 RenderRocksPS0True(); // RenderRocksPS(0.0f, true);
    }
}

PixelShaderOutput RenderRocksPS1True(const VertexShaderOutput PSIn)
{
	return RenderRocksPS(PSIn, 1.0f, true);
}
technique DebugRenderRocksSingleLayer1
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 RenderRocksPS1True(); // RenderRocksPS(1.0f, true);
    }
}

PixelShaderOutput RenderRocksPS2True(const VertexShaderOutput PSIn)
{
	return RenderRocksPS(PSIn, 2.0f, true);
}
technique DebugRenderRocksSingleLayer2
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 RenderRocksPS2True(); // RenderRocksPS(2.0f, true);
    }
}

//*****************************************
PixelShaderOutput MultiTexturedPS0(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 0.0f, -1.0f);
}
technique MultiTextured0
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS0(); // MultiTexturedPS(0.0f, -1.0f);
    }
}

PixelShaderOutput MultiTexturedPS1(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 1.0f, -1.0f);
}
technique MultiTextured1
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS1(); // MultiTexturedPS(1.0f, -1.0f);
        
    }
}

PixelShaderOutput MultiTexturedPS2(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 2.0f, -1.0f);
}
technique MultiTextured2
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS2(); // MultiTexturedPS(2.0f, -1.0f);
    }
}

//***************
PixelShaderOutput MultiTexturedPS00(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 0.0f, 0.0f);
}
technique MultiTextured00
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS00(); // MultiTexturedPS(0.0f, 0.0f);
    }
}

PixelShaderOutput MultiTexturedPS10(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 1.0f, 0.0f);
}
technique MultiTextured10
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS10(); // MultiTexturedPS(1.0f, 0.0f);
        
    }
}

PixelShaderOutput MultiTexturedPS20(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 2.0f, 0.0f);
}
technique MultiTextured20
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS20(); // MultiTexturedPS(2.0f, 0.0f);
    }
}

PixelShaderOutput MultiTexturedPS01(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 0.0f, 1.0f);
}
technique MultiTextured01
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS01(); // MultiTexturedPS(0.0f, 1.0f);
    }
}

PixelShaderOutput MultiTexturedPS11(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 1.0f, 1.0f);
}
technique MultiTextured11
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS11(); // MultiTexturedPS(1.0f, 1.0f);
        
    }
}

PixelShaderOutput MultiTexturedPS21(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 2.0f, 1.0f);
}

technique MultiTextured21
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS21(); // MultiTexturedPS(2.0f, 1.0f);
    }
}

PixelShaderOutput MultiTexturedPS02(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 0.0f, 2.0f);
}

technique MultiTextured02
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS02(); // MultiTexturedPS(0.0f, 2.0f);
    }
}

PixelShaderOutput MultiTexturedPS12(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 1.0f, 2.0f);
}

technique MultiTextured12
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS12(); // MultiTexturedPS(1.0f, 2.0f);
        
    }
}

PixelShaderOutput MultiTexturedPS22(const VertexShaderOutput PSIn)
{
	return MultiTexturedPS(PSIn, 2.0f, 2.0f);
}

technique MultiTextured22
{
    pass Pass0
    {
		VertexShader = compile vs_4_0 MultiTexturedVS();
		PixelShader = compile ps_4_0 MultiTexturedPS22(); // MultiTexturedPS(2.0f, 2.0f);
    }
}


// use this for debugging in PIX - disables optimizations:
//technique MultiTextured0
//{
//    pass Pass0
//    {
//        VertexShader = compile vs_3_0 NEWMultiTexturedVS(); 
//        PixelShader = compile ps_3_0 MultiTexturedPS(0.0f);
//    }
//}
//technique MultiTextured1
//{
//    pass Pass0
//    {
//        VertexShader = compile vs_3_0 NEWMultiTexturedVS(); 
//        PixelShader = compile ps_3_0 MultiTexturedPS(1.0f);
//        
//    }
//}
//technique MultiTextured2
//{
//    pass Pass0
//    {
//        VertexShader = compile vs_3_0 NEWMultiTexturedVS(); 
//        PixelShader = compile ps_3_0 MultiTexturedPS(2.0f);        
//    }
//}

