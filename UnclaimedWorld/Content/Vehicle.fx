uniform const bool EnvironmentMapEnabled = false;
uniform const bool DirtMapEnabled = false;
uniform const bool DetailsMapEnabled = false;

uniform const float	ReflectivityFactor = 1;

float3	ReplaceColor0;
float3	ReplaceColor1;
float3	ReplaceColor2;
float3	ReplaceColor3;

//-----------------------------------------------------------------------------
// Texture sampler
//-----------------------------------------------------------------------------

uniform const texture BasicTexture;
uniform const sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (BasicTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

uniform const texture DirtMap;
uniform const sampler DirtMapSampler : register(s1) = sampler_state
{
	Texture = (DirtMap);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

uniform const texture EnvironmentMap;
uniform const sampler EnvironmentMapSampler : register(s2) = sampler_state
{
	Texture = (EnvironmentMap);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

uniform const texture DetailsMap;
uniform const sampler DetailsMapSampler : register(s3) = sampler_state
{
	Texture = (DetailsMap);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

uniform texture DistanceHeightAndBillboardAlpha;
sampler DistanceHeightAndBillboardAlphaSampler = sampler_state
{
    Texture = (DistanceHeightAndBillboardAlpha);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

uniform texture OverlayGradient;
sampler OverlayGradientSampler = sampler_state 
{ 
    Texture = (OverlayGradient);     
}; 

uniform const texture ScanlinesTexture;
uniform const sampler ScanlinesSampler = sampler_state
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


//-----------------------------------------------------------------------------
// Material settings
//-----------------------------------------------------------------------------

uniform const float3	DiffuseColor	: register(c5) = 1;
uniform const float		Alpha			: register(c6) = 1;
uniform const float3	EmissiveColor	: register(c7) = 0;
uniform const float3	SpecularColor	: register(c8) = 0;
uniform const float		SpecularPower	: register(c9) = 0;

uniform const float		Reflectivity = 0.5f;
uniform const float		DirtLevel = 0; // Grime/grunge is zero until Morten separates the basictexture and grunge.

//-----------------------------------------------------------------------------
// Lights
// All directions and positions are in world space and must be unit vectors
//-----------------------------------------------------------------------------

uniform const float3	AmbientLightColor		: register(c10);

uniform const float3	DirLight0Direction		: register(c11);
uniform const float3	DirLight0DiffuseColor	: register(c12);
uniform const float3	DirLight0SpecularColor	: register(c13);

uniform const float3	DirLight1Direction		: register(c14);
uniform const float3	DirLight1DiffuseColor	: register(c15);
uniform const float3	DirLight1SpecularColor	: register(c16);

uniform const float3	DirLight2Direction		: register(c17);
uniform const float3	DirLight2DiffuseColor	: register(c18);
uniform const float3	DirLight2SpecularColor	: register(c19);


//-----------------------------------------------------------------------------
// Matrices
//-----------------------------------------------------------------------------

uniform const float4x4	World		: register(vs, c20);	// 20 - 23
uniform const float4x4	View		: register(vs, c24);	// 24 - 27
uniform const float4x4	Projection	: register(vs, c28);	// 28 - 31
uniform const float4x4	UncorrectedWorld;	
//uniform const float	UncorrectedWorldPositionY;	
uniform const float2 ScanlinesTextureDimensions;

uniform const float3	EyePosition		: register(c4);		// in world space

//NEW (overlay):
uniform const float2   ViewportSize : register (c32); 
uniform const float2   WindowPosition: register (c34); 

// overlay / lights:
uniform const float		AlphaFactor = 1;

struct ColorPair
{
	float3 Diffuse;
	float3 Specular;
};

struct VertexShaderInput
{
    float4	Position	: POSITION;
	float2	TexCoord	: TEXCOORD0;
	float3	Normal		: NORMAL;
/*	float4	Color		: COLOR;*/
};

struct VertexShaderOutput
{
    float4	PositionPS	: POSITION;		// POSITION may not be used in Pixel shader! Gives 'Invalid input semantic' error!
	float2	TexCoord	: TEXCOORD0;
	float4	PositionWS	: TEXCOORD1;
	float3	NormalWS	: TEXCOORD2;
	float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
	
	float3 Reflection : TEXCOORD3; // for reflection/environment mapping
    float3 Fresnel : COLOR1;	// for reflection/environment mapping
    float2 DistanceFromViewer : TEXCOORD4;
    float4 ScreenPosition : TEXCOORD5; // copy the screen pos. info here...
    float2 ScanLinesTexCoords	: TEXCOORD6;
};



#include "ModelShader.fxh"
	

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, uniform bool environmentMapEnabled)
{
    VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 uncorrectedPosition = mul(input.Position, UncorrectedWorld);

	float4 pos_vs = mul(worldPosition, View);
	float4 pos_ps = mul(pos_vs, Projection);
	
	output.PositionPS		= pos_ps; // assign, but must not be used...!
	output.PositionWS		= worldPosition;
	
	output.ScreenPosition = pos_ps; // use this instead...
	
	output.ScanLinesTexCoords.x = output.ScreenPosition.x * ScanlinesTextureDimensions.x; // ViewportSize.x / 640; //move to params!!!
	output.ScanLinesTexCoords.y = output.ScreenPosition.y * ScanlinesTextureDimensions.y; // ViewportSize.y / 496; 
		
	

	// use the uncorrected world matrix when computing the distance from the viewer. verify by comparing with billboards on the same line near the top/bottom parts of the screen.
	output.DistanceFromViewer = saturate(1 - (uncorrectedPosition.y - WindowPosition.y) / ViewportSize.y);
    	

	/*float4 uncorrectedWorldPosition = mul(input.Position, UncorrectedWorld);	
	output.DistanceFromViewer = saturate(1 - (uncorrectedWorldPosition.y - WindowPosition.y) / ViewportSize.y);
    */
     
     // OLD: Models are treated as points, but otherwise they now have correct depth for lighting etc:   
	//output.DistanceFromViewer = saturate(1 - (UncorrectedWorldPositionY - WindowPosition.y) / ViewportSize.y);
        
	
	float3 worldNormal = normalize(mul(input.Normal, World));
	
	output.NormalWS		= worldNormal;
	output.Diffuse.rgb	= 1.0; //input.Color.rgb;
	// why is alpha less than 1 on skimmer???
	output.Diffuse.a	= Alpha; //input.Color.a * Alpha;
	output.TexCoord		= input.TexCoord;	
	
	if (environmentMapEnabled)
	{
	    // Compute a reflection vector for the environment map.
		float3 eyePosition = mul(-View._m30_m31_m32, transpose(View));

		float3 viewVector = worldPosition - eyePosition;
    
		output.Reflection = reflect(viewVector, worldNormal);
	    
		// Approximate a Fresnel coefficient for the environment map.
		// This makes the surface less reflective when you are looking
		// straight at it, and more reflective when it is viewed edge-on.
		output.Fresnel = ReflectivityFactor * Reflectivity * saturate(1 + dot(normalize(viewVector), worldNormal));
	}
	else 
	{
		output.Reflection = float3(0, 0, 0);
		output.Fresnel = float3(0, 0, 0);
	}
	
    return output;
}

//-----------------------------------------------------------------------------
// Compute per-pixel lighting.
// When compiling for pixel shader 2.0, the lit intrinsic uses more slots
// than doing this directly ourselves, so we don't use the intrinsic.
// E: Eye-Vector
// N: Unit vector normal in world space
//-----------------------------------------------------------------------------
ColorPair ComputePerPixelLights(float3 E, float3 N, bool isOverlay)
{
	ColorPair result;
	
	result.Diffuse = AmbientLightColor;
	result.Specular = 0;
	
	// Light0
	float3 L = -DirLight0Direction;
	float3 H = normalize(E + L);
	float dt = max(0,dot(L,N));
    result.Diffuse += DirLight0DiffuseColor * dt;
    if (!isOverlay && dt != 0)
		result.Specular += DirLight0SpecularColor * pow(max(0.00001,dot(H,N)), SpecularPower);

	// Light1
	L = -DirLight1Direction;
	H = normalize(E + L);
	dt = max(0,dot(L,N));
    result.Diffuse += DirLight1DiffuseColor * dt;
    if (!isOverlay && dt != 0)
	    result.Specular += DirLight1SpecularColor * pow(max(0.00001,dot(H,N)), SpecularPower);
    
	// Light2
	L = -DirLight2Direction;
	H = normalize(E + L);
	dt = max(0,dot(L,N));
    result.Diffuse += DirLight2DiffuseColor * dt;
    // skip specular contribution. Not enough shader instructions...
 /*   if (dt != 0)
	    result.Specular += DirLight2SpecularColor * pow(max(0.00001,dot(H,N)), SpecularPower);*/
    
    result.Diffuse *= DiffuseColor;
    result.Diffuse += EmissiveColor;
    result.Specular *= SpecularColor;
		
	return result;
}

 // ---------ooooo EDGE DETECT ooooooo------------------
    // OUTPUT Normal AND DEPTH INFO FOR EDGE DETECT
    


// Alternative vertex shader outputs normal and depth values, which are then
// used as an input for the edge detection filter in EdgeDetect.fx.
NormalDepthVertexShaderOutput NormalDepthVertexShader(VertexShaderInput input)
{
	NormalDepthVertexShaderOutput output;
			
    
	// Apply camera matrices to the input position.
	output.Position = mul(mul(mul(input.Position, World), View), Projection);
    
	float3 worldNormal = mul(input.Normal, World);

	// The output color holds the normal, scaled to fit into a 0 to 1 range.
	output.Color.rgb = (worldNormal + 1) / 2;

	// The output alpha holds the depth, scaled to fit into a 0 to 1 range.
	output.Color.a = output.Position.z / output.Position.w;
    
	return output;    
}




PixelShaderOutput PixelShaderFunction(VertexShaderOutput input, uniform bool computeLighting, uniform bool monochrome, uniform bool drawAsOverlay) //: COLOR0
{
	PixelShaderOutput output;
		
	if (computeLighting)
	{
		//return float4(tex2D(TextureSampler, input.TexCoord), 1.0);
		//return tex2D(TextureSampler, input.TexCoord);
	    
		float3 posToEye = EyePosition - input.PositionWS.xyz;
		
		float3 N = normalize(input.NormalWS);
		float3 E = normalize(posToEye);
		
		ColorPair lightResult = ComputePerPixelLights(E, N, drawAsOverlay);
		
		float4 base = tex2D(TextureSampler, input.TexCoord);
		float3 colorreplaced;
		
		/* do color replace */
		if (base.a < 0.9) 
		{
			if (base.a > 0.75)
			{
				colorreplaced = ReplaceColor0;
			}
			else if (base.a > 0.5)
			{
				colorreplaced = ReplaceColor1;
			}
			else if (base.a > 0.25)
			{
				colorreplaced = ReplaceColor2;
			}
			else 
			{
				colorreplaced = ReplaceColor3;
			}
		}
		else 
		{
			colorreplaced.rgb = base.rgb; 
		}
		
		if (DetailsMapEnabled) 
		{
			float4 details = tex2D(DetailsMapSampler, input.TexCoord);
			colorreplaced.rgb = details.a * details.rgb + (1 - details.a) * colorreplaced.rgb;
		}
		
		float dirtAmount;
		float inverseDirtAmount;
		
		if (!drawAsOverlay && DirtMapEnabled) 
		{
			float4 dirt = tex2D(DirtMapSampler, input.TexCoord);
			dirtAmount = DirtLevel * dirt.a;
			inverseDirtAmount = 1 - dirtAmount;
			
			colorreplaced.rgb = dirtAmount * dirt.rgb + inverseDirtAmount * colorreplaced.rgb;
		}
		else 
		{
			dirtAmount = 0;
			inverseDirtAmount = 1;
		}
		
		float3 diffuse = colorreplaced * lightResult.Diffuse; 
		
		float3 color = diffuse + inverseDirtAmount * lightResult.Specular;
		
		/* OLD:
		float3 diffuse = ((1.0 - DirtLevel) * tex2D(TextureSampler, input.TexCoord) 
							+ DirtLevel * tex2D(DirtMapSampler, input.TexCoord)) 
								* float4(lightResult.Diffuse * input.Diffuse.rgb, input.Diffuse.a);
		
		float3 color = diffuse + lightResult.Specular; 
		*/
		
		// Use the Fresnel coefficient to interpolate between texture and environment map.
		if (!drawAsOverlay && EnvironmentMapEnabled) 
		{
			float3 envmap = texCUBE(EnvironmentMapSampler, input.Reflection);
			//OLD: color = lerp(color, envmap, input.Fresnel); 		
			
			color = lerp(color, envmap, inverseDirtAmount * input.Fresnel);					 
		}
			
		if (monochrome)
		{
			// desaturate
			float3 grayXfer = float3(0.3, 0.59, 0.11);
			float grayf = dot(grayXfer, float3(color.x, color.y, color.z));
			color = float3(grayf, grayf, grayf);
			
			//float3 gray = float3(grayf, grayf, grayf);
			 
			 // blue tint
			 // 0.23 0.44 0.68
			//color = float3(0, 0.375 * grayf, 0.63 * grayf);
		}

		float finalAlpha = Alpha * AlphaFactor; // Material alpha - is this working?
			
		
		if (drawAsOverlay)
		{
			
			//Doing that will give you XY coordinates such that X = -1 is the left side of the screen, X = 1 is the right side, Y = 1 is the top, and Y = -1 is the bottom.
			float2 screenPos = input.ScreenPosition.xy / input.ScreenPosition.w;
			
			screenPos.y *= -1;
			screenPos /= 2;
			screenPos += 0.5;
			
			float4 distanceHeightAndBillboardAlpha = tex2D(DistanceHeightAndBillboardAlphaSampler, screenPos); 

			// uses two bytes...
			float pixelDistance = distanceHeightAndBillboardAlpha.x; 
			float billboardAlpha = distanceHeightAndBillboardAlpha.z;
			
			float thisEntitysPixelDistanceFromViewer = input.DistanceFromViewer; 
					    
				
				// desaturate
			float3 grayXfer = float3(0.3, 0.59, 0.11);
			float grayf = dot(grayXfer, float3(color.x, color.y, color.z));
			
			grayf = saturate(grayf * 2.5);
			
			float3 scanlines = tex2D(ScanlinesSampler, input.ScanLinesTexCoords);
			grayf *= scanlines.r;
			
			//color.rgb = color.rgb * 1.2 * scanlines;  
			
			//Now sample the gradient map at the greyscale value
			color.rgb = tex1D(OverlayGradientSampler, grayf);
			
			    
			finalAlpha = finalAlpha * scanlines.r;
			
			if (pixelDistance < thisEntitysPixelDistanceFromViewer)
			{		
				// There is an occluder in front, so we use the occluder's alpha value to determine what we can see:
				
				//output.Color.w = (1 - billboardAlpha); //overlayPixel.w * (1 - billboardAlpha);
		        output.Color = float4(color, finalAlpha * (1 - billboardAlpha));
			}
			else 
			{
				output.Color = float4(color, finalAlpha);
			}
			
			
		}
		else 
		{		
			output.Color = float4(color, finalAlpha); 
		}
	}
	else 
	{
		//return float4(0, 0, 0.1, 1.0);
		output.Color = float4(0, 0, 0.1, 1.0);
	}
	
	return output;
}

void RenderEmitters(in VertexShaderOutput input, out EmitterPixelShaderOutput output)
{
	float finalAlpha = Alpha * AlphaFactor; // Material alpha
		
	float3 posToEye = EyePosition - input.PositionWS.xyz;
		
	float3 N = normalize(input.NormalWS);
	float3 E = normalize(posToEye);

	ColorPair lightResult = ComputePerPixelLights(E, N, false); // try with a bit of shading on the lights, to soften them up...

	output.color = float4(lightResult.Diffuse * EmissiveColor, finalAlpha);

	// we need to store distance information too (in second render target - use MRT):			
	output.DistanceFromViewer = input.DistanceFromViewer.x - 0.1; // place light sources a bit in front of the model part so they don't block themselves!
}

VertexShaderOutput VertexShaderFunctionTrue(VertexShaderInput input)
{
	return VertexShaderFunction(input, true);
}
PixelShaderOutput PixelShaderFunctionTrueTrueFalse(VertexShaderOutput input) //: COLOR0
{
	return PixelShaderFunction(input, true, true, false);
}
technique StandardRenderMonochrome
{
    pass Pass1
    {        
		VertexShader = compile vs_4_0 /*vs_3_0*/ VertexShaderFunctionTrue(); // VertexShaderFunction(true);
		PixelShader = compile ps_4_0 /*ps_3_0*/ PixelShaderFunctionTrueTrueFalse(); // PixelShaderFunction(true, true, false);
    }
}

PixelShaderOutput PixelShaderFunctionTrueFalseFalse(VertexShaderOutput input) //: COLOR0
{
	return PixelShaderFunction(input, true, false, false);
}
technique StandardRender
{
    pass Pass1
    {       
      
		VertexShader = compile vs_4_0 /*vs_3_0*/ VertexShaderFunctionTrue(); // VertexShaderFunction(true);
		PixelShader = compile ps_4_0 /*ps_3_0*/ PixelShaderFunctionTrueFalseFalse(); // PixelShaderFunction(true, false, false);
    }
}

VertexShaderOutput VertexShaderFunctionFalse(VertexShaderInput input)
{
	return VertexShaderFunction(input, false);
}
technique StandardRenderNoReflection
{
    pass Pass1
    {        
		VertexShader = compile vs_4_0 /*vs_3_0*/ VertexShaderFunctionFalse(); // VertexShaderFunction(false);
		PixelShader = compile ps_4_0 /*ps_3_0*/ PixelShaderFunctionTrueFalseFalse(); //PixelShaderFunction(true, false, false);
    }
}

PixelShaderOutput PixelShaderFunctionTrueFalseTrue(VertexShaderOutput input) 
{
	return PixelShaderFunction(input, true, false, true);
}
technique StandardOverlay
{
    pass Pass1
    {        
		VertexShader = compile vs_4_0 /*vs_3_0*/ VertexShaderFunctionTrue(); // VertexShaderFunction(true);
		PixelShader = compile ps_4_0 /*ps_3_0*/ PixelShaderFunctionTrueFalseTrue(); // PixelShaderFunction(true, false, true);
    }
}

PixelShaderOutput PixelShaderFunctionFalseFalseFalse(VertexShaderOutput input)
{
	return PixelShaderFunction(input, false, false, false);
}
technique RenderNoLighting
{
    pass P0
    {
		VertexShader = compile vs_4_0 VertexShaderFunctionFalse(); //VertexShaderFunction(false);
		PixelShader = compile ps_4_0 PixelShaderFunctionFalseFalseFalse(); // PixelShaderFunction(false, false, false);
    }
}

technique DepthHeightBillboardAlpha
{
    pass P0
    {
		VertexShader = compile vs_4_0 VertexShaderFunctionFalse(); //VertexShaderFunction(false);
		PixelShader = compile ps_4_0 PS_DepthHeightBillboardAlpha();
    }
}

technique EmittersOnly
{
	pass P0
    {
		VertexShader = compile vs_4_0 VertexShaderFunctionFalse(); //VertexShaderFunction(false);
		PixelShader = compile ps_4_0 RenderEmitters();
    }
}

// Technique draws the object as normal and depth values for later use in outlining.
technique NormalDepth
{
	pass P0
	{
		VertexShader = compile vs_4_0 NormalDepthVertexShader();
		PixelShader = compile ps_4_0 NormalDepthPixelShader(); // #MONOCHANGE - higher shader model
	}
}