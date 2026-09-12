/*
Include file.
Some of the stuff that is shared between the skinned and the unskinned model shaders.
*/

struct PixelShaderOutput 
{
   float4 Color:   COLOR0;  
   // MRT Doesn't work with multisampling on DX9... See you in 2011?
   //float4 DepthHeightBillboardAlpha:   COLOR1;
};

// Color and Depth info for the emitting models (MRT)
struct EmitterPixelShaderOutput
{
    float4 color : COLOR;       
	float4 DistanceFromViewer : COLOR1;
};

// Output structure for the vertex shader that renders normal and depth information.
struct NormalDepthVertexShaderOutput
{
	float4 Position : SV_Position; // POSITION0;
	float4 Color : COLOR0;
};

// Simple pixel shader for rendering the normal and depth information.
float4 NormalDepthPixelShader(NormalDepthVertexShaderOutput input) : COLOR0 //float4 color : COLOR0) : COLOR0
{
	return input.Color; // float4(color.rgb, 1); // color; // float4(1, 1, 1, 1); 
}

PixelShaderOutput PS_DepthHeightBillboardAlpha(VertexShaderOutput input)
{
	PixelShaderOutput output;
	
	// Three component render target:
		
	/// x: 10 bits - DistanceFromViewer - used in lighting (LightSourcesEffect) to determine if shapes are occluding the light sources
    /// y: 10 bits - Height over ground - not currently used
    /// z: 10 bits - Billboard Alpha - used in CloudShadows.fx to overlay shadows - Important: We set Billboard Alpha = 1. This means we are unaffected by drop shadows, including our own!
	/// w: Don't Care (2 bits only)    
				
	//output.color = float4(0, 1, 1, 1); 	  
	output.Color = float4(input.DistanceFromViewer.x, 1, 1, 1); // NEW: proper distance!		
				
					
	return output;        
}