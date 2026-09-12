
float2   ViewportSize; 
float2   WindowPosition;

// 1: Darkest night, 0: Daylight
float DarknessLevel;

texture DiffuseSceneTexture;
texture DistanceHeightAndBillboardAlpha;
texture LightSourceTexture;
texture EmitterLightSourceDistance;

uniform const bool UseIntegerPositions;

sampler DistanceHeightAndBillboardAlphaSampler = sampler_state
{
    Texture = (DistanceHeightAndBillboardAlpha);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

sampler DiffuseSceneSampler = sampler_state
{
    Texture = (DiffuseSceneTexture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

sampler LightSourceSampler = sampler_state
{
    Texture = (LightSourceTexture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

sampler EmitterLightSourceDistanceSampler = sampler_state
{
    Texture = (EmitterLightSourceDistance);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VS_INPUT
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;    
    float3 WorldPosition : POSITION1;
    
};

struct VS_OUTPUT
{
    float4 Position : POSITION0; // POSITION may not be used in Pixel shader! Gives 'Invalid input semantic' error!
    float2 TexCoord : TEXCOORD0;
    float2 ScreenPosition : TEXCOORD1; // copy the screen pos. info here...
    float DistanceFromViewer : TEXCOORD2;
};


float3 MoveToScreenSpace(VS_INPUT input)
{	
	float3 position = input.Position;
    
	 // FROM SPRITEBATCH:
    // Half pixel offset for correct texel centering.
  /*  if (UseIntegerPositions)
    {
		position.xy -= 0.5;
	}*/

	// Viewport adjustment.
	position.xy /= ViewportSize;
	position.xy *= float2(2, -2);
	position.xy -= float2(1, -1);
   
   
    return position;
}

VS_OUTPUT VertexShaderFunction(VS_INPUT input)
{
    VS_OUTPUT output;
        
    if (UseIntegerPositions)
    {	// not when scrolling...       
		input.Position = round(input.Position); // Important(?) use discrete pixel positions to avoid blurryness...
    }
    
    // translate
	input.Position += input.WorldPosition;
    input.Position.xy -= WindowPosition;
    output.ScreenPosition = input.Position.xy / ViewportSize; // store this in order to look up into scene render texture
    
    output.Position = float4(MoveToScreenSpace(input), 1);	  
    
    output.TexCoord = input.TexCoord;    
        
    output.DistanceFromViewer = saturate(1 - (input.WorldPosition.y - WindowPosition.y) / ViewportSize.y);
    
       
    return output;
}

VS_OUTPUT VertexShaderModelEmitterLights(VS_INPUT input)
{
    VS_OUTPUT output;
        
    if (UseIntegerPositions)
    {	// not when scrolling...       
		input.Position = round(input.Position); // Important(?) use discrete pixel positions to avoid blurryness...
    }
    
  
    output.ScreenPosition = input.TexCoord;   // !!??
    
    output.Position = float4(MoveToScreenSpace(input), 1);	  //??
    
    output.TexCoord = input.TexCoord;    
        
    
	output.DistanceFromViewer = 0; // not used
       
    return output;
}



float4 RenderLight(VS_OUTPUT input, float lightSourceDistanceFromViewer)
{
	float4 lightSourceTexture = tex2D(LightSourceSampler, input.TexCoord);	
	
	float4 diffusePixel = tex2D(DiffuseSceneSampler, input.ScreenPosition);			

	float4 distanceHeightAndBillboardAlpha = tex2D(DistanceHeightAndBillboardAlphaSampler, input.ScreenPosition); 

	// uses two bytes...
	float pixelDistance = distanceHeightAndBillboardAlpha.x; 
	float billboardAlpha = distanceHeightAndBillboardAlpha.z;

	float4 litPixel;


	// pixel must be farther away than light source:
	// todo: look at height difference also...
	
	// Look at the light source and determine the light value.
	// remember, that this value is ADDED to the original texture which has already been tinted!
	// in broad daylight, we don't want any light added: DarknessLevel = 0. At night, we get the whole value added.
		
	if (lightSourceTexture.w == 1)
	{
		// 'reflected' light
		litPixel = DarknessLevel * lightSourceTexture * diffusePixel;
	}
	else 
	{	
		// this pixel is 'emitted' light
		// so we add the contribution (modulated by the light source alpha)
		litPixel = DarknessLevel * (lightSourceTexture.w * lightSourceTexture + diffusePixel); 
	}
		
    if (pixelDistance < lightSourceDistanceFromViewer)
    {		
		// There is an occluder in front, so we diminish the light contribution by the occluder's alpha value:
		litPixel *= (1 - billboardAlpha); //billboardAlpha = 1 means: No effect from this light on this pixel
      
    }
    
    return float4(litPixel.xyz, 0);	    

}

float4 PixelShaderLightSources(VS_OUTPUT input) : COLOR0 
{	
	return RenderLight(input, input.DistanceFromViewer);	   
}

float4 PixelShaderModelEmitterLights(VS_OUTPUT input) : COLOR0 
{	
	float4 DistanceFromViewer = tex2D(EmitterLightSourceDistanceSampler, input.ScreenPosition);		
		
	return RenderLight(input, DistanceFromViewer.x);
   // return RenderLight(input, DistanceFromViewer);	    
}

technique DrawLightSources
{
    pass Pass1
    {        
		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ PixelShaderLightSources();
    }
}

technique DrawModelEmitterLights
{
    pass Pass1
    {        
		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ VertexShaderModelEmitterLights();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ PixelShaderModelEmitterLights();
    }
}