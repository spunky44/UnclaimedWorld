//-----------------------------------------------------------------------------
// PostprocessEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

sampler SceneSampler : register(s0);

// Settings controlling the edge detection filter.
float EdgeWidth = 1;
float EdgeIntensity = 1;

// How sensitive should the edge detection be to tiny variations in the input data?
// Smaller settings will make it pick up more subtle edges, while larger values get
// rid of unwanted noise.
float NormalThreshold = 0.5;
float DepthThreshold = 0.1;

// How dark should the edges get in response to changes in the input data?
float NormalSensitivity = 1;
float DepthSensitivity = 10;

// Pass in the current screen resolution.
float2 ScreenResolution;


/*
// This texture contains the main scene image, which the edge detection
// and/or sketch filter are being applied over the top of.
texture SceneTexture;

sampler SceneSampler : register(s0) = sampler_state
{
    Texture = (SceneTexture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};*/


// This texture contains normals (in the color channels) and depth (in alpha)
// for the main scene image. Differences in the normal and depth data are used
// to detect where the edges of the model are.
texture NormalDepthTexture;

sampler NormalDepthSampler : register(s1) = sampler_state
{
    Texture = (NormalDepthTexture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};


// This texture contains an overlay sketch pattern, used to create the hatched
// pencil drawing effect.
/*texture SketchTexture;

sampler SketchSampler : register(s2) = sampler_state
{
    Texture = (SketchTexture);

    AddressU = Wrap;
    AddressV = Wrap;
};*/


// Pixel shader applies the edge detection postprocessing.
//float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
float4 PixelShaderFunction(float4 position : SV_Position, float4 col : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	
    // Look up the original color from the main scene.
   float3 scene = tex2D(SceneSampler, texCoord).xyz;
	
	// return float4(scene.x, scene.x, scene.x, 1);

    // Apply the edge detection filter
    
    // Look up four values from the normal/depth texture, offset along the
    // four diagonals from the pixel we are currently shading.
    float2 edgeOffset = EdgeWidth / ScreenResolution;
    
    float4 n1 = tex2D(NormalDepthSampler, texCoord + float2(-1, -1) * edgeOffset);
    float4 n2 = tex2D(NormalDepthSampler, texCoord + float2( 1,  1) * edgeOffset);
    float4 n3 = tex2D(NormalDepthSampler, texCoord + float2(-1,  1) * edgeOffset);
    float4 n4 = tex2D(NormalDepthSampler, texCoord + float2( 1, -1) * edgeOffset);

    // Work out how much the normal and depth values are changing.
    float4 diagonalDelta = abs(n1 - n2) + abs(n3 - n4);

    float normalDelta = dot(diagonalDelta.xyz, 1);
    float depthDelta = diagonalDelta.w;
    
    // Filter out very small changes, in order to produce nice clean results.
    normalDelta = saturate((normalDelta - NormalThreshold) * NormalSensitivity);
    depthDelta = saturate((depthDelta - DepthThreshold) * DepthSensitivity);

    // Does this pixel lie on an edge?
    float edgeAmount = saturate(normalDelta + depthDelta) * EdgeIntensity;
    
    // Apply the edge detection result to the main scene color.
    scene *= (1 - edgeAmount);
    
	//return float4(0.5, edgeAmount, edgeAmount, 1);

    return float4(scene, 1);
}


// Compile the pixel shader for doing edge detection.
technique EdgeDetect
{
    pass P0
    {
		PixelShader = compile ps_4_0_level_9_3 /*ps_2_0*/ PixelShaderFunction();
    }
}


