
float2   ViewportSize; 
float2   WindowPosition;

/*texture DiffuseSceneTexture;*/
uniform texture DistanceHeightAndBillboardAlpha;
uniform texture OverlayTexture;

//uniform texture OverlayGradient;

sampler DistanceHeightAndBillboardAlphaSampler = sampler_state
{
    Texture = (DistanceHeightAndBillboardAlpha);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};


sampler OverlaySampler = sampler_state
{
    Texture = (OverlayTexture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

// unused...
/*
sampler OverlayGradientSampler = sampler_state 
{ 
    Texture = (OverlayGradient);     
}; */

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

#define BlendOverlayf(base, blend) 	(base < 0.5 ? (2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend)))

// Component wise blending
#define Blend(base, blend, funcf) 		float3(funcf(base.r, blend.r), funcf(base.g, blend.g), funcf(base.b, blend.b))

#define BlendOverlay(base, blend) 		Blend(base, blend, BlendOverlayf)


struct VS_INPUT
{
    float3 Position		: POSITION0;
    float2 TexCoord		: TEXCOORD0;       
    float2 TruncatedEffectCoordinate : TEXCOORD1;
    float DrawScanlines : TEXCOORD2;
    float4 Color		: TEXCOORD3;    
    
};

struct VS_OUTPUT
{
    float4 Position : POSITION0; // POSITION may not be used in Pixel shader! Gives 'Invalid input semantic' error!
    float2 TexCoord : TEXCOORD0;
    float2 ScreenPosition : TEXCOORD1; // copy the screen pos. info here...
    float2 TruncatedEffectCoordinate : TEXCOORD3;
    float DrawScanlines : TEXCOORD4;
    float4 Color		: TEXCOORD5;    
};


float3 MoveToScreenSpace(VS_INPUT input)
{	
	float3 position = input.Position;
    
	 // FROM SPRITEBATCH:
    // Half pixel offset for correct texel centering.
	//position.xy -= 0.5;

	// Viewport adjustment.
	position.xy /= ViewportSize;
	position.xy *= float2(2, -2);
	position.xy -= float2(1, -1);
   
   
    return position;
}

VS_OUTPUT VertexShaderFunction(VS_INPUT input)
{
    VS_OUTPUT output;
        
    // translate
	/*
	input.Position += input.WorldPosition;
    input.Position.xy -= WindowPosition;
    */
    
    output.ScreenPosition = input.Position.xy / ViewportSize; // store this in order to look up into scene render texture
    
    output.Position = float4(MoveToScreenSpace(input), 1);	  
    
    output.TexCoord = input.TexCoord;    
            
    output.TruncatedEffectCoordinate = input.TruncatedEffectCoordinate;
    output.DrawScanlines = input.DrawScanlines;
    output.Color = input.Color;
       
    return output;
}


float4 PixelShaderOverlay(VS_OUTPUT input) : COLOR0 
{
	float4 overlayPixel = tex2D(OverlaySampler, input.TexCoord);	
		
	float4 distanceHeightAndBillboardAlpha = tex2D(DistanceHeightAndBillboardAlphaSampler, input.ScreenPosition); // diffuseTexture(x,y).a; 

	//float4 litPixel;
	
	// uses two bytes...
	float pixelDistance = distanceHeightAndBillboardAlpha.x; 
	float billboardAlpha = distanceHeightAndBillboardAlpha.z;
	float3 scanlines = tex2D(ScanlinesSampler, input.TruncatedEffectCoordinate);
    
  		
	// we are on the ground, so always hide behind billboards:
	overlayPixel.w = overlayPixel.w * (1 - billboardAlpha);
          
    // blend scanlines in (not working?)
    //float3 output = Blend(overlayPixel.xyz, scanlines, BlendOverlayf); 
    
    // yellow: facd00
    // output *= float3(0.98, 0.803, 0);
    
    //Now sample the gradient map at the greyscale value - not here...
    //output.rgb = tex1D(OverlayGradientSampler, output.x);
   
   if (input.DrawScanlines == 1)
   { 
		return float4(scanlines.xyz, overlayPixel.w) * input.Color; 
   }
   else 
   {
		return float4(input.Color.xyz, overlayPixel.w * input.Color.w); 
   }

    //return float4(output.xyz, overlayPixel.w) * input.Color; 
}

float4 PixelShaderInfluence(VS_OUTPUT input) : COLOR0
{
	float4 overlayPixel = tex2D(OverlaySampler, input.TexCoord);
		
	float3 scanlines = tex2D(ScanlinesSampler, input.TruncatedEffectCoordinate);


	// blend scanlines in (not working?)
	//float3 output = Blend(overlayPixel.xyz, scanlines, BlendOverlayf); 

	// yellow: facd00
	// output *= float3(0.98, 0.803, 0);

	//Now sample the gradient map at the greyscale value - not here...
	//output.rgb = tex1D(OverlayGradientSampler, output.x);

	return float4(scanlines.xyz, overlayPixel.w) * input.Color;	
}

technique DrawOverlayGroundSprite
{
    pass Pass1
    {
        // TODO: set renderstates here.

		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ PixelShaderOverlay();
    }
}

technique DrawInfluenceOverlay
{
	pass Pass1
	{		
		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ PixelShaderInfluence();
	}
}
