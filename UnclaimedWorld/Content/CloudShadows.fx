
//Input variables
float2   ViewportSize; 
float2   WindowPosition;

float CloudCoverLimit;
float ShadowAlpha;
float2 CloudPosition;
float CloudEdgeSharpness;

//uniform const float4 ShadowColor = float4(0, 0, 0.2, 0);
float4 ShadowColor = float4(0, 0, 0.2, 0);


//-----------------------------------------------------------------------------
// Texture sampler
//-----------------------------------------------------------------------------

uniform const texture CloudTexture;
uniform const sampler CloudSampler : register(s0) = sampler_state
{
	Texture = (CloudTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap; 
	AddressV = Wrap;
};

uniform const texture GroundDropShadowTexture;
uniform const sampler GroundDropShadowSampler : register(s1) = sampler_state
{
	Texture = (GroundDropShadowTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

uniform const texture BillboardDepthHeightMap;
uniform const sampler BillboardDepthHeightMapSampler : register(s2) = sampler_state
{
	Texture = (BillboardDepthHeightMap);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

struct VS_INPUT
{
    float4 Position: POSITION;
    float2 TextureCoords: TEXCOORD0;
};

struct VS_OUTPUT 
{
   float4 Position: POSITION;
   float2 ScreenPosition: TEXCOORD1;
   float2 TextureCoords: TEXCOORD0;
};

struct PS_OUTPUT 
{
   float4 Color:   COLOR;
};



VS_OUTPUT VertexShaderFunction(VS_INPUT input)
{
    VS_OUTPUT output;

	output.Position = input.Position;
	output.ScreenPosition = input.Position.xy;
	output.TextureCoords = input.TextureCoords;

	//output.TextureCoords *= 0.5;
	
	// the 4 vertices have positions upper left:-1,-1 to lower right:1,1 
	// convert to range 0 - 1 for sampling the cloud texture (the cloud texture is scaled to fill the entire window!)
    output.ScreenPosition += 1;
    output.ScreenPosition *= 0.5;



	//MLo: further reduce texture scale, to make cloud environment four times as big
    output.ScreenPosition *= 0.25;
 



    //output.ScreenPosition *= 0.2;
    
    // translate by window position:
    output.ScreenPosition.y -= (WindowPosition.y / ViewportSize.y)*.25;
	output.ScreenPosition.x += (WindowPosition.x / ViewportSize.x)*.25;
    
    // translate by cloud position to get the drifting effect:
    output.ScreenPosition += CloudPosition;
    
    
    
    return output;
}


PS_OUTPUT RenderCloudShadowsPS(VS_OUTPUT input)
{
    PS_OUTPUT Out;
    
    Out.Color = ShadowColor;
    
	/*Out.Color = float4(1.0, 1.0, 1.0, 1.0);
	return Out;*/


    float2 screenPos = input.ScreenPosition;
    
    //float4 cloud = tex2D(CloudSampler, input.TextureCoords);	
    float4 cloud = tex2D(CloudSampler, input.ScreenPosition);	


	
	// get the previously rendered drop shadows on this pixel:
	float4 dropShadowsContribution = tex2D(GroundDropShadowSampler, input.TextureCoords); //input.ScreenPosition);		
	float4 depthMapInfo = tex2D(BillboardDepthHeightMapSampler, input.TextureCoords); //input.ScreenPosition);
	
	// Drop shadows go under billboards, while cloud shadows go over!
	// But what about the shadows from flyers?
	
	// depthMap is a two component render target:
    // x: DistanceFromViewer (Depth), y: Height, z: Billboard Alpha, w: Don't Care
	dropShadowsContribution.a *= (1 - depthMapInfo.z); // ground is hidden behind billboards when z = 1, partially hidden when z = 0.5 and free when z = 0.
	
	// adjust the alpha with this fudge factor to avoid light edges around the billboards...
	dropShadowsContribution.a *= 3;
	
	float cloudContribution = 0;
    if (cloud.x > CloudCoverLimit)
    {		
		cloudContribution = saturate((cloud.x - CloudCoverLimit) * CloudEdgeSharpness);		
    } 
	
    Out.Color.a = saturate(cloudContribution + dropShadowsContribution.a);
    
    
    
	// make shadows translucent. tint with blue?
	Out.Color.a *= ShadowAlpha * 0.25f;
	
	//Out.Color = cloud;

    return Out;
}

//--------------------------------------------------------------//
// Technique Section for Simple screen transform
//--------------------------------------------------------------//


technique RenderCloudShadows
{
   pass Single_Pass
   {
        SrcBlend = SrcAlpha; 
        DestBlend = InvSrcAlpha; 

		VertexShader = compile vs_4_0_level_9_1 /*ps_1_1*/ VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ RenderCloudShadowsPS();
   }
}
