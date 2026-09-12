    
	
	// These are required by the Animation library
	uniform const float4x4 MatrixPalette[56]; 

	
    float4x4 World;
 
    // These are not
    float4x4 View;
    float4x4 Projection;
    
	// used by depth/distance renderer instead of World
	float4x4 UncorrectedWorld;

    texture BasicTexture; 
    sampler TextureSampler = sampler_state
    {
       Texture = (BasicTexture);
    };
 
	float3		DiffuseColor	: register(c5) = 1;
	float		Alpha			: register(c6) = 1;
	float3		EmissiveColor	: register(c7) = 0;
	float3		SpecularColor	: register(c8) = 0;
	float		SpecularPower	: register(c9) = 0;   
	    
	uniform const float3	EyePosition		: register(c4);		// in world space
	
	uniform const float2 ScanlinesTextureDimensions;

	//-----------------------------------------------------------------------------
	// Lights
	// All directions and positions are in world space and must be unit vectors
	//-----------------------------------------------------------------------------

	//uniform const float3	AmbientLightColor; //		: register(c10);

	uniform const float3	DirLight0Direction; //		: register(c11);
	uniform const float3	DirLight0DiffuseColor; //	: register(c12);
	uniform const float3	DirLight0SpecularColor; //	: register(c13);

	uniform const float3	DirLight1Direction; //		: register(c14);
	uniform const float3	DirLight1DiffuseColor; //	: register(c15);
	uniform const float3	DirLight1SpecularColor; //	: register(c16);

	uniform const float3	DirLight2Direction; //		: register(c17);
	uniform const float3	DirLight2DiffuseColor; //	: register(c18);
	// drop this. Not enough instructions in Pixel Shader 2.0:
	//uniform const float3	DirLight2SpecularColor; //	: register(c19);

	float3	ReplaceColor0;
	float3	ReplaceColor1;
	float3	ReplaceColor2;
	float3	ReplaceColor3;
	
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

	// overlay / lights:
	uniform const float		AlphaFactor = 1;

	// needed by the emitting light shader:
	float2   ViewportSize; 
	float2   WindowPosition;

	struct ColorPair
	{
		float3 Diffuse;
		float3 Specular;		
	};
    
    // This is passed into our vertex shader from Xna
    struct VS_INPUT
    {
			// This is the position of the vertex in the model file
		//float4 position : POSITION;
		float4 position : SV_Position;

			// The vertex normal
		float3 normal : NORMAL0;

		// This is the texture coordinate for the vertex in the model file
		float2 texcoord : TEXCOORD0;

		// These are the indices (4 of them) that index the bones that affect
		// this vertex.  The indices refer to the MatrixPalette.
		int4 indices: BLENDINDICES0;
		//half4 indices : BLENDINDICES0;
		//float4 indices: BLENDINDICES0;

		// These are the weights (4 of them) that determine how much each bone
		// affects this vertex.
		float4 weights : BLENDWEIGHT0;
    };
    
          
        // This is the output from our skinning method
    struct SKIN_OUTPUT
    {
        float4 position;
        float4 normal;
    };
    
	struct SKIN_OUTPUT_DEBUG
	{
		float4 position;
		
		/*float index0;
		float index1;
		float index2;
		float index3;*/
	};

        // This method takes in a vertex and applies the bone transforms to it.
	SKIN_OUTPUT_DEBUG Skin4Debug(const VS_INPUT input)
    {
		SKIN_OUTPUT_DEBUG output = (SKIN_OUTPUT_DEBUG)0;
        // Since the weights need to add up to one, store 1.0 - (sum of the weights)
        float lastWeight = 1.0;
        float weight = 0;
        
		output.position = mul(input.position, MatrixPalette[input.indices[0]]);

		/*
		output.index0 = input.indices[0];
		output.index1 = input.indices[1];
		output.index2 = input.indices[2];
		output.index3 = input.indices[3];
		*/
		// Apply the transforms for the first 3 weights
		/*for (int i = 0; i < 3; ++i)
		{
			weight = input.weights[i];
			lastWeight -= weight;
			output.position += mul(input.position, MatrixPalette[input.indices[i]]) * weight; 
			output.normal += mul(input.normal, MatrixPalette[input.indices[i]]) * weight;
		}
		// Apply the transform for the last weight
		output.position += mul(input.position, MatrixPalette[input.indices[3]]) * lastWeight;
		output.normal += mul(input.normal, MatrixPalette[input.indices[3]]) * lastWeight;
		*/

        return output;
    };
    
	SKIN_OUTPUT Skin4(const VS_INPUT input)
	{
		SKIN_OUTPUT output = (SKIN_OUTPUT)0;
		// Since the weights need to add up to one, store 1.0 - (sum of the weights)
		float lastWeight = 1.0;
		float weight = 0;
		// Apply the transforms for the first 3 weights
		for (int i = 0; i < 3; ++i)
		{
			weight = input.weights[i];
			lastWeight -= weight;
			output.position += mul(input.position, MatrixPalette[input.indices[i]]) * weight;
			output.normal += mul(input.normal, MatrixPalette[input.indices[i]]) * weight;
		}
		// Apply the transform for the last weight
		output.position += mul(input.position, MatrixPalette[input.indices[3]]) * lastWeight;
		output.normal += mul(input.normal, MatrixPalette[input.indices[3]]) * lastWeight;

		return output;
	};

	// This is passed into our pixel shader
    struct VertexShaderOutput
    {
		float4	PositionPS	: POSITION;		// Position in projection space
        float2	TexCoord	: TEXCOORD0;
        
        float4	PositionWS	: TEXCOORD1;
		float3	NormalWS	: TEXCOORD2;
		
		float2 DistanceFromViewer : TEXCOORD3; // NEW: for correct light occlusion!

		float4 ScreenPosition : TEXCOORD4; 
		float2 ScanLinesTexCoords	: TEXCOORD5;

		//float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
    };
    
	struct VertexShaderDebugOutput
	{
		float4	PositionPS	: POSITION;		
		float2	TexCoord	: TEXCOORD0;
		
		float4 indices: COLOR0;
		float4 weights: COLOR1;

		//float4 indexes: COLOR0;
		/*
		float index0;
		float index1;
		float index2;
		float index3;*/
	};



	#include "ModelShader.fxh"
	
	VertexShaderOutput TransformVertex(in VS_INPUT input)
    {
		VertexShaderOutput output = (VertexShaderOutput)0;

        // Calculate the skinned position
        SKIN_OUTPUT skin = Skin4(input); 
        	
      
        float4x4 WorldViewProjection = mul(World, mul(View, Projection));
        						
		// This is the final position of the vertex, and where it will be drawn on the screen
		output.PositionPS = mul(skin.position, WorldViewProjection);               
        float4 worldNormal = mul(skin.normal, World);
        output.TexCoord = input.texcoord;        
      
        output.NormalWS = worldNormal;
        output.PositionWS = mul(skin.position, World); 
		
		// no skin transform:
		/*output.PositionPS = mul(input.position, WorldViewProjection);
		float4 worldNormal = mul(input.normal, World);
		output.TexCoord = input.texcoord;

		output.NormalWS = worldNormal;
		output.PositionWS = mul(input.position, World);
		*/

		output.ScreenPosition = output.PositionPS; //pos_ps; /* for overlay rendering */

		output.ScanLinesTexCoords.x = output.ScreenPosition.x * ScanlinesTextureDimensions.x; // ViewportSize.x / 640; 
		output.ScanLinesTexCoords.y = output.ScreenPosition.y * ScanlinesTextureDimensions.y; // ViewportSize.y / 496; 
	
		float4	uncorrectedPositionWS = mul(skin.position, UncorrectedWorld);
		//float4	uncorrectedPositionWS = mul(input.position, UncorrectedWorld);

		// use the uncorrected world matrix when computing the distance from the viewer. verify by comparing with billboards on the same line near the top/bottom parts of the screen.
		float distanceFromViewer = saturate(1 - (uncorrectedPositionWS.y - WindowPosition.y) / ViewportSize.y);
				
    
		output.DistanceFromViewer.x = distanceFromViewer;

		return output;
    }
    
	VertexShaderDebugOutput TransformVertexDebug(in VS_INPUT input)
	{
		VertexShaderDebugOutput output = (VertexShaderDebugOutput)0;

		// Calculate the skinned position
		SKIN_OUTPUT skin = Skin4(input);
	
		float4x4 WorldViewProjection = mul(World, mul(View, Projection));
		
		// This is the final position of the vertex, and where it will be drawn on the screen
		output.PositionPS = mul(skin.position, WorldViewProjection);

		output.TexCoord = input.texcoord;
	
		output.indices = input.indices; //  float4(skin.index0, skin.index1, skin.index2, skin.index3);
		output.weights = input.weights;
		/*
		output.index0 = skin.index0;
		output.index1 = skin.index1;
		output.index2 = skin.index2;
		output.index3 = skin.index3;*/
		

		return output;
	}

	PixelShaderOutput TransformPixelDebug(in VertexShaderDebugOutput input)
	{
		PixelShaderOutput output = (PixelShaderOutput)0;

		//output.Color = float4(32, 1, 0, 1.0);
		output.Color = float4(input.indices.rgb, 1.0);
		//output.Color = float4(input.weights.rgb, 1.0);

		//output.Color = float4(input.index0 / 50, input.index1 / 50, input.index2 / 50, 1.0);
		
		//output.Color = float4(0, 1, 0, 1.0); 

		return output;
	}
       
		

    //-----------------------------------------------------------------------------
	// Compute per-pixel lighting.
	// When compiling for pixel shader 2.0, the lit intrinsic uses more slots
	// than doing this directly ourselves, so we don't use the intrinsic.
	// E: Eye-Vector
	// N: Unit vector normal in world space
	//-----------------------------------------------------------------------------
	ColorPair ComputePerPixelLights(float3 E, float3 N, uniform bool includeSpecular)
	{
		ColorPair result;
		
		result.Diffuse = 0; // AmbientLightColor;
		result.Specular = 0;
	
		// Light0
		float3 L = -DirLight0Direction;
		float3 H = normalize(E + L);
		float dt = max(0,dot(L,N));
		result.Diffuse += DirLight0DiffuseColor * dt;

		if (includeSpecular)
		{
			if (dt != 0)
				result.Specular += DirLight0SpecularColor * pow(max(0.00001f,dot(H,N)), SpecularPower);
		}

		// Light1
		L = -DirLight1Direction;
		H = normalize(E + L);
		dt = max(0,dot(L,N));
		result.Diffuse += DirLight1DiffuseColor * dt;
		
		if (includeSpecular)
		{
			if (dt != 0)
				result.Specular += DirLight1SpecularColor * pow(max(0.00001f,dot(H,N)), SpecularPower);
	    }

		// Light2
		L = -DirLight2Direction;
		H = normalize(E + L);
		dt = max(0,dot(L,N));
		result.Diffuse += DirLight2DiffuseColor * dt;

	//	if (dt != 0)				// N.B!!! Not enough instructions.. skip last specular light. 
		//	result.Specular += DirLight2SpecularColor * pow(max(0.00001f,dot(H,N)), SpecularPower);
	    
		result.Diffuse += float3(0.4, 0.4, 0.55); // ambient light

		result.Diffuse *= DiffuseColor;

		/*result.Diffuse += EmissiveColor;*/

		if (includeSpecular)
		{
			result.Specular *= SpecularColor;
		}

		return result;
	}
    
	PixelShaderOutput TransformPixel(in VertexShaderOutput input, /*out PixelShaderOutput output,*/ uniform bool computeLighting, uniform bool monochrome, uniform bool drawAsOverlay)
    {		
		PixelShaderOutput output;

		float finalAlpha = Alpha * AlphaFactor; // Material alpha - is this working?
		
		if (computeLighting)
		{
			float3 posToEye = EyePosition - input.PositionWS.xyz;
		
			float3 N = normalize(input.NormalWS);
			float3 E = normalize(posToEye);
			
			ColorPair lightResult = ComputePerPixelLights(E, N, true);
			
			float4 diffuseTex = tex2D(TextureSampler, input.TexCoord);  // float4(lightResult.Diffuse * input.Diffuse.rgb, input.Diffuse.a);
			float3 diffuse;
			
			if (diffuseTex.a < 0.9) 
			{
				if (diffuseTex.a > 0.75)
				{
					diffuse = ReplaceColor0;
				}
				else if (diffuseTex.a > 0.5)
				{
					diffuse = ReplaceColor1;
				}
				else if (diffuseTex.a > 0.25)
				{
					diffuse = ReplaceColor2;
				}
				else 
				{
					diffuse = ReplaceColor3;
				}
			}
			else 
			{
				diffuse.rgb = diffuseTex.rgb; 
			}
			
			float3 color = diffuse * lightResult.Diffuse + lightResult.Specular; // + EmissiveColor; //emissive color parts are rendered separately
		    
			if (monochrome)
			{
				// desaturate
				float3 grayXfer = float3(0.3, 0.59, 0.11);
				float grayf = dot(grayXfer, float3(color.x, color.y, color.z));
				color = float3(grayf, grayf, grayf);	
			}


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
			output.Color = float4(0, 0, 0.1, 1.0); //1; // what is this for...
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

		//output.color = float4(0, 0, 0, finalAlpha);
		//output.color = float4(EmissiveColor, finalAlpha);
		output.color = float4(lightResult.Diffuse * EmissiveColor, finalAlpha);

		// we need to store distance information too (in second render target - use MRT):	
		
		output.DistanceFromViewer = input.DistanceFromViewer.x - 0.1; // place light sources a bit in front of the model part so they don't block themselves!
	}
    
   
    
    // ---------ooooo EDGE DETECT ooooooo------------------
    // OUTPUT Normal AND DEPTH INFO FOR EDGE DETECT
    
   

	// Alternative vertex shader outputs normal and depth values, which are then
	// used as an input for the edge detection filter in PostprocessEffect.fx.
	NormalDepthVertexShaderOutput NormalDepthVertexShader(VS_INPUT input)
	{
		NormalDepthVertexShaderOutput output;
		
		// Calculate the skinned position
        SKIN_OUTPUT skin = Skin4(input);
        
		// Apply camera matrices to the input position.
		output.Position = mul(mul(mul(skin.position, World), View), Projection);
	    
		float3 worldNormal = mul(skin.normal, World);


		//output.Color = float4(1, 1, 1, 1);

		// The output color holds the normal, scaled to fit into a 0 to 1 range.
		output.Color.rgb = (worldNormal + 1) / 2;

		// The output alpha holds the depth, scaled to fit into a 0 to 1 range.
		output.Color.a = output.Position.z / output.Position.w;
	    

		return output;    
	}

	PixelShaderOutput TransformPixelTrueTrueFalse(in VertexShaderOutput input)
	{
		return TransformPixel(input, true, true, false);
	}
	technique StandardRenderMonochrome
	{
		pass Pass1
		{        
			VertexShader = compile vs_4_0 /*vs_2_0*/ TransformVertex(); // #MONOCHANGE was: vs_4_0_level_9_1
			PixelShader = compile ps_4_0 /*ps_2_0*/ TransformPixelTrueTrueFalse(); // TransformPixel(true, true, false);
		}
	}
    
	PixelShaderOutput TransformPixelTrueFalseFalse(in VertexShaderOutput input)
	{
		return TransformPixel(input, true, false, false);
	}
    technique SkinnedRender
    {
        pass P0
        {
			VertexShader = compile vs_4_0 TransformVertex();
			PixelShader = compile ps_4_0 TransformPixelTrueFalseFalse();
        }
    }
	/*technique SkinnedRender
	{
		pass P0
		{
			VertexShader = compile vs_4_0 TransformVertexDebug();
			PixelShader = compile ps_4_0 TransformPixelDebug();
		}
	}*/

	PixelShaderOutput TransformPixelFalseFalseFalse(in VertexShaderOutput input)
	{
		return TransformPixel(input, false, false, false);
	}
    technique SkinnedRenderNoLighting
    {
        pass P0
        {
			VertexShader = compile vs_4_0 /*vs_2_0*/ TransformVertex();
			PixelShader = compile ps_4_0 /*ps_2_0*/ TransformPixelFalseFalseFalse(); // TransformPixel(false, false, false);
        }
    }
    
	PixelShaderOutput TransformPixelTrueFalseTrue(in VertexShaderOutput input)
	{
		return TransformPixel(input, true, false, true);
	}
	technique StandardOverlay
	{
		pass P0
		{        
			VertexShader = compile vs_4_0 /*vs_3_0*/ TransformVertex();
			PixelShader = compile ps_4_0 /*ps_3_0*/ TransformPixelTrueFalseTrue(); // TransformPixel(true, false, true);
		}
	}

    technique DepthHeightBillboardAlpha
    {
        pass P0
        {
			VertexShader = compile vs_4_0 /*vs_2_0*/ TransformVertex();
			PixelShader = compile ps_4_0 /*ps_2_0*/ PS_DepthHeightBillboardAlpha();
        }
    }

	technique EmittersOnly
	{
		pass P0
        {
			VertexShader = compile vs_4_0 /*vs_2_0*/ TransformVertex();
			PixelShader = compile ps_4_0 /*ps_2_0*/ RenderEmitters();
        }
	}
    
    
    // Technique draws the object as normal and depth values for later use in outlining.
	technique NormalDepth
	{
		pass P0
		{

			VertexShader = compile vs_4_0 /*vs_2_0*/ NormalDepthVertexShader();
			PixelShader = compile ps_4_0 /*ps_2_0*/ NormalDepthPixelShader();
						
		}
	}