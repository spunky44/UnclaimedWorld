// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.

sampler TextureSampler : register(s0);
//15
#define SAMPLE_COUNT 15 

float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];


//float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
float4 PixelShaderFunction(float4 position : SV_Position, float4 col : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        c += tex2D(TextureSampler, texCoord + SampleOffsets[i]) * SampleWeights[i];
    }
    
    return c;
}


technique GaussianBlur
{
    pass Pass1
    {
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ PixelShaderFunction();
    }
}
