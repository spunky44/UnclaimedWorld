
/*
Billboard effect shader, used for MemoryFacts
*/

float2   ViewportSize; 
float2   WindowPosition;

/*texture DiffuseSceneTexture;*/
uniform texture DistanceHeightAndBillboardAlpha;
uniform texture OverlayTexture;


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

/*uniform texture OverlayGradient;
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
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;    
    float3 WorldPosition : POSITION1;
    float2 TruncatedEffectCoordinate : TEXCOORD1;
    float DrawScanlines : TEXCOORD2;
    float4 GradientColor1 : TEXCOORD3;
    float4 GradientColor2 : TEXCOORD4;
	float4 GradientColor3 : TEXCOORD5;
};

struct VS_OUTPUT
{
    float4 Position : POSITION0; // POSITION may not be used in Pixel shader! Gives 'Invalid input semantic' error!
    float2 TexCoord : TEXCOORD0;
    float2 ScreenPosition : TEXCOORD1; // copy the screen pos. info here...
    float DistanceFromViewer : TEXCOORD2;
    float2 TruncatedEffectCoordinate : TEXCOORD3;
    float DrawScanlines : TEXCOORD4;
    float4 GradientColor1 : TEXCOORD5;
	float4 GradientColor2 : TEXCOORD6;
	float4 GradientColor3 : TEXCOORD7;
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
	input.Position += input.WorldPosition;
    input.Position.xy -= WindowPosition;
    output.ScreenPosition = input.Position.xy / ViewportSize; // store this in order to look up into scene render texture
    
    output.Position = float4(MoveToScreenSpace(input), 1);	  

    output.TexCoord = input.TexCoord;    
        
    output.DistanceFromViewer = saturate(1 - (input.WorldPosition.y - WindowPosition.y) / ViewportSize.y);
    
    output.TruncatedEffectCoordinate = input.TruncatedEffectCoordinate;
    output.DrawScanlines = input.DrawScanlines;
    
    output.GradientColor1 = input.GradientColor1;
    output.GradientColor2 = input.GradientColor2;
    output.GradientColor3 = input.GradientColor3;

    return output;
}


float4 PixelShaderOverlay(VS_OUTPUT input) : COLOR0 
{
	float4 texturePixel = tex2D(OverlaySampler, input.TexCoord);	
			
	float4 distanceHeightAndBillboardAlpha = tex2D(DistanceHeightAndBillboardAlphaSampler, input.ScreenPosition); // diffuseTexture(x,y).a; 

	//float4 litPixel;
	
	// uses two bytes...
	float pixelDistance = distanceHeightAndBillboardAlpha.x; 
	float billboardAlpha = distanceHeightAndBillboardAlpha.z;
	
	float lightSourceDistanceFromViewer = input.DistanceFromViewer; 
	
	float3 scanlines = tex2D(ScanlinesSampler, input.TruncatedEffectCoordinate);
    
	// pixel must be farther away than light source:			
		
    if (pixelDistance < lightSourceDistanceFromViewer)
    {		
		// There is an occluder in front, so we diminish the light contribution by the occluder's alpha value:
		
		texturePixel.w = texturePixel.w * (1 - billboardAlpha);
       
    }
    
    float3 output = Blend(texturePixel.xyz, 1.2 * scanlines, BlendOverlayf); 
      
    float grayValue = output.x;

	if (output.x < 0.5)
	{
		output = lerp(input.GradientColor1.rgb, input.GradientColor2.rgb, grayValue * 2.0);
		
		//output.rgb = lerp(float3(1,0.2,0), float3(1,1,0), output.x * 2.0);
	}
	else {
		output = lerp(input.GradientColor2.rgb, input.GradientColor3.rgb, (grayValue - 0.5) * 2.0);

		//output.rgb = lerp(float3(1,1,0), float3(1,1,1), (output.x - 0.5) * 2.0);
	}

	//Lars the white areas during the placement cycle anim seem to not be as transparent as the other areas?

	//output.rgb *= input.Color.rgb;
	float alpha = lerp(input.GradientColor1.a, input.GradientColor3.a, grayValue);

    return float4(output.xyz, alpha * texturePixel.w * 0.75); // * input.Color.w); 
}

technique DrawOverlay
{
    pass Pass1
    {
        // TODO: set renderstates here.

		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ PixelShaderOverlay();
    }
}
