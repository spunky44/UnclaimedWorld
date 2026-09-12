// Shader for Kensei.Debug.Shape

/*---------------------------------------------------------------------------*/
// Shader Constants
/*---------------------------------------------------------------------------*/

float4x4 renderMatrix;

/*---------------------------------------------------------------------------*/
// Vertex Structures
/*---------------------------------------------------------------------------*/

struct VertexInput
{
	float3 pos		: POSITION;
	float4 color	: COLOR;
};

struct VertexOutput 
{
	float4 pos		: POSITION;
	float4 color	: COLOR;
};

/*---------------------------------------------------------------------------*/
// Technique LineRendering3D
/*---------------------------------------------------------------------------*/

VertexOutput LineRendering3DVS( VertexInput In )
{
	VertexOutput Out;
	
	Out.pos = mul( float4( In.pos, 1 ), renderMatrix );
	Out.color = In.color;

	return Out;
}

float4 LineRendering3DPS( VertexOutput In ) : Color
{
	return In.color;
}

technique LineRendering3D
{
	pass PassFor3D
	{
		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ LineRendering3DVS();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ LineRendering3DPS();
	}
}

/*---------------------------------------------------------------------------*/
// Technique LineRendering2D
/*---------------------------------------------------------------------------*/

VertexOutput LineRendering2DVS( VertexInput In )
{
	VertexOutput Out;
	
	Out.pos = float4( In.pos, 1 );
	Out.color = In.color;

	return Out;
}

float4 LineRendering2DPS( VertexOutput In ) : Color
{
	return In.color;
}

technique LineRendering2D
{
	pass PassFor2D
	{
		VertexShader = compile vs_4_0_level_9_1 /*vs_1_1*/ LineRendering2DVS();
		PixelShader = compile ps_4_0_level_9_1 /*ps_2_0*/ LineRendering2DPS();
	}
}
