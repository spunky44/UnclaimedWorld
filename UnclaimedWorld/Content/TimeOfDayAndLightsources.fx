
//Input variables

texture baseTexture;
float4 AmbientColorForLightSources;
float alphaFactor;

sampler baseSampler = 
sampler_state
{
    Texture = < baseTexture >;
 /*   MipFilter = LINEAR;  // xna 3 - gives blurred rendering!
    MinFilter = LINEAR; 
    MagFilter = LINEAR; */

	// xna 4
	MipFilter = POINT; 
    MinFilter = POINT;
    MagFilter = POINT;

    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};


struct VS_INPUT
{
    float4 ObjectPos: POSITION;
    float2 TextureCoords: TEXCOORD0;
};

struct VS_OUTPUT 
{
   float4 ScreenPos:   POSITION;
   float2 TextureCoords: TEXCOORD0;
};

struct PS_OUTPUT 
{
   float4 Color:   COLOR;
};



VS_OUTPUT SimpleVS(VS_INPUT In)
{
   VS_OUTPUT Out;

    //Move to screen space
    Out.ScreenPos = In.ObjectPos;
    Out.TextureCoords = In.TextureCoords;
    
    return Out;
}

PS_OUTPUT RenderAmbientLightPS(VS_OUTPUT In)
{
    PS_OUTPUT Out;
    
    Out.Color = tex2D(baseSampler, In.TextureCoords);    
  
	Out.Color *= AmbientColorForLightSources; 
    
    return Out;
}

PS_OUTPUT ProcessLightSourcesMapPS(VS_OUTPUT In)
{
    PS_OUTPUT Out;
    
    Out.Color = tex2D(baseSampler, In.TextureCoords);
    
  
	if (Out.Color.a == 1)
	{
		// occluded pixel - render ambient light:
		Out.Color = AmbientColorForLightSources; // float4(ambientColor.rgb, 1);
	}	
	
    
    return Out;
}

PS_OUTPUT ApplyShadowMapPS(VS_OUTPUT In)
{
    PS_OUTPUT Out;
    
    Out.Color = tex2D(baseSampler, In.TextureCoords);
    
    // make shadows translucent. tint with blue?
    Out.Color.a *= alphaFactor * 0.25f;
    
    return Out;
}

//--------------------------------------------------------------//
// Technique Section for Simple screen transform
//--------------------------------------------------------------//
technique AmbientLight
{
   pass Single_Pass
   {
       /* SrcBlend = SrcAlpha; 
        DestBlend = InvSrcAlpha; */

	   VertexShader = compile vs_4_0_level_9_1  SimpleVS();
	   PixelShader = compile ps_4_0_level_9_1 RenderAmbientLightPS();
   }
}

technique ApplyShadowMap
{
   pass Single_Pass
   {
        SrcBlend = SrcAlpha; 
        DestBlend = InvSrcAlpha; 

		VertexShader = compile vs_4_0_level_9_1 /*vs_2_0*/ SimpleVS();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ ApplyShadowMapPS();
   }
}
