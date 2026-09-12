//-----------------------------------------------------------------------------
// Billboard.fx
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

// positioning:
//float ModelYCorrectionFactor;
//float CameraTargetY;


float2   ViewportSize; 
uniform const float2   WindowPosition;

uniform const float ShadowFactor;
uniform const float ShadowXAlignment;

uniform const float4x4 Rotation;
uniform const float4x4 ShadowScaling;

// Lighting parameters.
uniform const float3 LightPosition;

// Copied in multitex.fx:
uniform const float3 LightColor = float3(1.15, 1.15, 1.0); //1.15;
uniform const float3 AmbientColorForNormalMapping = float3(0.15, 0.15, 0.35);

uniform const bool UseIntegerPositions;

// Parameters controlling the wind effect.
float3 WindDirection = float3(1, 0, 0);
float WindWaveSize = 0.1;
float WindRandomness = 1;
float WindSpeed = 4;
float WindAmount = 2; //0.5f;
float WindTime;


// Parameters describing the billboard itself.
float BillboardWidth;
float BillboardHeight;

texture DiffuseTexture;
texture NormalTexture;

#include "OutlineShader.fxh"

sampler TextureSampler = sampler_state
{
    Texture = (DiffuseTexture);
		
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;

	
    
    AddressU = Clamp;
    AddressV = Clamp;
};

sampler NormalSampler = sampler_state
{
    Texture = (NormalTexture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};


struct VS_INPUT
{
	float3 Position : SV_Position; //POSITION0;
    float2 TexCoord : TEXCOORD0;
    float2 NormalTextureCoordinate : TEXCOORD1;
    
    float Random : TEXCOORD2;
    float Bendyness : TEXCOORD3;
    float FlipNormals : TEXCOORD4;
    float WidthHeightRatio : TEXCOORD5;
    
    float3 WorldPosition : POSITION1;    
	float4 Tint : TEXCOORD6;
};


struct VS_OUTPUT
{
	float4 Position : SV_Position; //: POSITION0;
    float2 TexCoord : TEXCOORD0;
    float2 NormalTextureCoordinate : TEXCOORD1;
    float2 DistanceFromViewer : TEXCOORD2;
    float FlipNormals : TEXCOORD3;
    float4 Tint: TEXCOORD4;
 /*   float4 Color : COLOR0;*/
};

struct PS_OUTPUT 
{
   float4 Color:   COLOR0;
   // MRT Doesn't work with multisampling on DX9... See you in 2011?
   //float4 DepthHeightBillboardAlpha:   COLOR1;
};



float3 MoveToScreenSpace(VS_INPUT input)
{	
	float3 position = input.Position;
		
	
	// FROM SPRITEBATCH:
    // Half pixel offset for correct texel centering.	
    
	/*
    if (UseIntegerPositions)
    {	// not when scrolling...      
	
		position.xy -= 0.5;
	}*/
	
	// Viewport adjustment.
	position.xy /= ViewportSize;
	position.xy *= float2(2, -2);
	position.xy -= float2(1, -1);
	
	
    return position;
}

float3 ApplyWind(VS_INPUT input)
{
   
    float3 position = input.Position;
       
    // Work out how this vertex should be affected by the wind effect.
    float waveOffset = dot(position, WindDirection) * WindWaveSize;
    
    waveOffset += input.Random * WindRandomness;
    
    // Wind makes things wave back and forth in a sine wave pattern.
    float wind = sin(WindTime * WindSpeed + waveOffset) * WindAmount;
    
    // But it should only affect the top two vertices of the billboard!
	wind *= input.Bendyness;   
   
    position += WindDirection * wind;
    
      
    return position;
}

VS_OUTPUT VertexShaderShadow(VS_INPUT input)
{
	VS_OUTPUT output;
	float3 position = ApplyWind(input);
	
	// rotate and scale:
		
	float4x4 currentShadowScaling = ShadowScaling;
	currentShadowScaling[0][0] *= lerp(1, 1.0/input.WidthHeightRatio, ShadowXAlignment); // (1.0/input.WidthHeightRatio); // 
	
	position = mul(position, currentShadowScaling); //ShadowScaling);
	position = mul(position, Rotation);
    
	
	// translate: (use a matrix instead?)
	position += input.WorldPosition;
	
	position.xy -= WindowPosition;
    	
	
	input.Position = position;
	
	// do rotation, stretching etc. here...
	
	position = MoveToScreenSpace(input);
	
	output.Position = float4(position, 1);    
	output.TexCoord = input.TexCoord;    
    output.NormalTextureCoordinate = input.NormalTextureCoordinate;
    
    //output.IsLightSource = input.IsLightSource;
    output.DistanceFromViewer = 0; // This is Don't Care because we are rendering shadows...
	
	output.FlipNormals = input.FlipNormals;
	output.Tint = input.Tint;
	return output;
}

VS_OUTPUT VertexShaderOutlineFunction(VS_INPUT input)
{
    VS_OUTPUT output; 
    
	
    if (UseIntegerPositions)
    {	// not when scrolling...    
	  
		input.Position = round(input.Position); // VERY Important! use discrete pixel positions to avoid blurryness...
    }
    
    // translate: 
	input.Position += input.WorldPosition;
	
	// compute this before converting position to screen:
	float waveOffset = dot(input.Position, WindDirection) * WindWaveSize;
	
    input.Position.xy -= WindowPosition;
    
    // WIND    
	input.Position = ApplyWind(input);
	
    float3 position = MoveToScreenSpace(input);	  
    
    output.Position = float4(position, 1);    
    output.TexCoord = input.TexCoord;    
    output.NormalTextureCoordinate = input.NormalTextureCoordinate;
      
    float distanceFromViewer = saturate(1 - (input.WorldPosition.y - WindowPosition.y) / ViewportSize.y);
    
    // uses two bytes for higher precision:
    output.DistanceFromViewer.x = distanceFromViewer;
    output.DistanceFromViewer.y = 1; // use this for height above ground...
    
    output.FlipNormals = input.FlipNormals;
    output.Tint = input.Tint;
    return output;
}


VS_OUTPUT VertexShaderFunction(VS_INPUT input)
{
    VS_OUTPUT output; 
    
	
    if (UseIntegerPositions)
    {	// not when scrolling...    
	  
		input.Position = round(input.Position); // VERY Important! use discrete pixel positions to avoid blurryness...
    }
    
    // translate: 
	input.Position += input.WorldPosition;
		

	// compute this before converting position to screen:
	float waveOffset = dot(input.Position, WindDirection) * WindWaveSize;
	
    input.Position.xy -= WindowPosition;
    

	//input.Position = round(input.Position); 


    // WIND    
	input.Position = ApplyWind(input);
	

    float3 position = MoveToScreenSpace(input);	  
    
    output.Position = float4(position, 1);    
    output.TexCoord = input.TexCoord;    
    output.NormalTextureCoordinate = input.NormalTextureCoordinate;
      
    float distanceFromViewer = saturate(1 - (input.WorldPosition.y - WindowPosition.y) / ViewportSize.y);
    
    // uses two bytes for higher precision:
    output.DistanceFromViewer.x = distanceFromViewer;
    output.DistanceFromViewer.y = 1; // use this for height above ground...
    
    output.FlipNormals = input.FlipNormals;
    output.Tint = input.Tint;
    return output;
}



PS_OUTPUT PixelShaderOutlineFunction(VS_OUTPUT input) 
{
	PS_OUTPUT output;

	//sampler texSampler = TextureSampler;
	float2 texc = input.TexCoord;
	float4 Color = GetOutlineColor(TextureSampler, texc, input.Tint);
	Color.a *= input.Tint.a;
	Color.rbg *= input.Tint.a;

	 if(Color.a == 0)
	 {
		discard;
	 }
	output.Color = Color;
	return output;
}


PS_OUTPUT PixelShaderFunction(VS_OUTPUT input, uniform bool computeLighting, uniform bool debugLight) 
{
	PS_OUTPUT output;
	
    // Look up the texture and normalmap values.
	float4 tex = tex2D(TextureSampler, input.TexCoord);
	float4 normal = tex2D(NormalSampler, input.NormalTextureCoordinate);

	
    if (computeLighting) // && normal.a > 0) 
    {
		// Compute lighting.
		
		float3 lightPos = LightPosition;
		if (input.FlipNormals > 0)
		{			
			lightPos.x = -lightPos.x;
		}
		
		
		float lightAmount = max(dot(normal, lightPos), 0);	
		
		lightAmount = saturate(lightAmount);
		// keep the light/darkness within reasonable levels:
		lightAmount = 0.7 * lightAmount + 0.3;
		
		// apply depth map alpha (alpha = 0 means we ignore lighting):
		float lightingEffect = ShadowFactor * normal.a;
		
		float3 color = AmbientColorForNormalMapping + lightAmount * LightColor;
	
		
	    //output.Color = float4(tex * color, tex.a);   	
	    
	    if (debugLight)
	    {   // only light:
			lightAmount = lerp(1.0, lightAmount, normal.a);
			output.Color = float4(lightAmount, lightAmount, lightAmount, tex.a);
	    }
	    else 
	    {
			//the real one: XNA 3
			output.Color = float4(lerp(tex, tex * color, lightingEffect), tex.a);	
	    }	   	    
	  }
	  else 
	  {
		output.Color = tex;				
	  }
	  
	  // TESTING
	//  output.Color = normal;

	  return output;
}


PS_OUTPUT PS_DepthHeightBillboardAlpha(VS_OUTPUT input)
{
		/// x: 10 bits - DistanceFromViewer - used in lighting (LightSourcesEffect) to determine if shapes are occluding the light sources
        /// y: 10 bits - Height over ground - not currently used
        /// z: 10 bits - Billboard Alpha - used in CloudShadows.fx to overlay shadows
        
	PS_OUTPUT output;
	
	// Look up the texture and normalmap values.
    float4 tex = tex2D(TextureSampler, input.TexCoord);
    
	  
    
    // we use the 2 bit alpha channel together with the alpha blend mode to skip this pixel if it is empty.
    if (tex.a > 0)
	{	// OLD: alpha blend is very discrete... just 4 values... is it a problem...? Perhaps dump the height (y) component, it isn't used anyway yet.
		// NEW XNA 4: the render target format is now different...
		
		//output.Color = float4(input.DistanceFromViewer.x, 1, tex.a, 1);    // we need to blend the tex.a value with the value already in the scene (behind it). But this gives problems at the edges when the billboard overlaps another.
		
		output.Color = float4(input.DistanceFromViewer.x, 1, tex.a, tex.a);  // not so good either, because many sprites are translucent when they shouldn't be (big hollow)...  
		//output.Color = float4(input.DistanceFromViewer.x, 1, tex.a, tex.a) * tex.a;  // xna 4
    }
    else 
    {	 
		output.Color = float4(input.DistanceFromViewer.x, 1, tex.a, 0);    // xna 3
		//output.Color = float4(0, 0, 0, 0);    // xna 4
    }
	
	return output;        
}


//float4 PixelShaderRenderToNormalsAndDepthMap(float2 texCoord : TEXCOORD0) : COLOR0
float4 PixelShaderRenderToNormalsAndDepthMap(VS_OUTPUT input) : COLOR0
{
	//float4 tex = tex2D(TextureSampler, texCoord);
	float4 tex = tex2D(TextureSampler, input.TexCoord);
		
	if (tex.a < 0.2) {
		discard;
	}	

	//return float4(1, 1, 1, 1);
	return float4(0, 0, 0, 1);
	
}


//float4 PixelShaderShadow(float2 texCoord : TEXCOORD0) : COLOR0
float4 PixelShaderShadow(VS_OUTPUT input) : COLOR0
{
	float4 tex = tex2D(TextureSampler, input.TexCoord);
	return float4(0, 0, 0.1, tex.a); 
	//return tex;	
}

PS_OUTPUT PixelShaderFunctionTrueFalse(VS_OUTPUT input)
{
	return PixelShaderFunction(input, true, false);
}

technique Standard
{
    pass RenderAll
    {
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunctionTrueFalse(); // PixelShaderFunction(true, false);
    }   
}

technique Outline
{
    pass RenderAll
    {
		VertexShader = compile vs_4_0_level_9_1 VertexShaderOutlineFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderOutlineFunction();
    }   
}

PS_OUTPUT PixelShaderFunctionTrueTrue(VS_OUTPUT input)
{
	return PixelShaderFunction(input, true, true);
}
technique StandardDebugLighting
{
        
    pass RenderAll
    {
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunctionTrueTrue(); // PixelShaderFunction(true, true);
        
    }   
  
}

PS_OUTPUT PixelShaderFunctionFalseFalse(VS_OUTPUT input)
{
	return PixelShaderFunction(input, false, false);
}
technique StandardAtNight
{
	pass p0
    {
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunctionFalseFalse(); // PixelShaderFunction(false, false);
        
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
    }

}

technique NormalsAndDepthMap
{
	pass p0
    {
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderRenderToNormalsAndDepthMap();
		
    }
}


technique DepthHeightBillboardAlpha
{
    pass P0
    {
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PS_DepthHeightBillboardAlpha();
    }
}

technique Shadow
{
	pass p0
    {
		VertexShader = compile vs_4_0_level_9_1 VertexShaderShadow();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderShadow();
			
    }
}
