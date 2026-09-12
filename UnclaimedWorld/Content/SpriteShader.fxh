Texture NormalMap;
sampler NormalMapSampler = sampler_state { texture = <NormalMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

//********** Normal map lighting: ***********
// Lighting parameters.
uniform const float ShadowFactor;
uniform const float3 LightPosition;
uniform const float3 LightColor = float3(1.15, 1.15, 1.0); 
uniform const float3 AmbientColorForNormalMapping = float3(0.15, 0.15, 0.35);
// ******************************************


float4 ApplyLighting(float4 colorToBlend, float2 TextureCoords, uniform const bool debugLighting, float alpha)
{
	float4 normal = tex2D(NormalMapSampler, TextureCoords);  //input.NormalTextureCoordinate);   
  
	float lightAmount = max(dot(normal.xyz, LightPosition), 0);				
	lightAmount = saturate(lightAmount);

	// keep the light/darkness within reasonable levels:
	// NOTE: Different than for billboards - we want the result a bit less extreme...
	lightAmount = 0.5 * lightAmount + 0.5;
			
	// apply depth map alpha (alpha = 0 means we ignore lighting):
	float lightingEffect = ShadowFactor * normal.a; //  include alpha!!!
		
	float3 color = AmbientColorForNormalMapping + lightAmount * LightColor;
	
	if (debugLighting)
    {   // only light:    		
		
		lightAmount = lerp(1.0, lightAmount, lightingEffect); 
		colorToBlend = float4(lightAmount, lightAmount, lightAmount, alpha);								
    }
    else 
    {
		//the real one:
		colorToBlend = float4(lerp(colorToBlend.xyz, colorToBlend.xyz * color, lightingEffect), alpha);			
    }

	
	return colorToBlend;
}	