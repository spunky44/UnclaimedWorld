

// ******************************************


float4 GetOutlineColor(sampler texSampler,float2 texCoord, float3 tintingColor)
{
	



	float4 Color;
	float adjustNum = 0.001f;
	Color =  tex2D(texSampler, texCoord);
	Color += tex2D(texSampler, float2(texCoord.x+adjustNum,texCoord.y))* 0.5;
	Color += tex2D(texSampler, float2(texCoord.x,texCoord.y+adjustNum))* 0.5;
	Color += tex2D(texSampler, float2(texCoord.x-adjustNum,texCoord.y))* 0.5;
	Color += tex2D(texSampler, float2(texCoord.x,texCoord.y-adjustNum))* 0.5;
	Color += tex2D(texSampler, float2(texCoord.x-adjustNum,texCoord.y-adjustNum))* 0.25;//corrner
	Color += tex2D(texSampler, float2(texCoord.x+adjustNum,texCoord.y+adjustNum))* 0.25;//corrner
	Color += tex2D(texSampler, float2(texCoord.x-adjustNum,texCoord.y+adjustNum))* 0.25;//corrner
	Color += tex2D(texSampler, float2(texCoord.x+adjustNum,texCoord.y-adjustNum))* 0.25;//corrner

	float4 returnColor = float4(0,0,0,0);
	if(Color.a > 0 && Color.a < 1)
	{
		
		if(Color.a > 0.5)
		{
			returnColor.a = 1-Color.a;
		}
		else
		{
			returnColor.a = Color.a;
		}

		returnColor.r = tintingColor.r;
		returnColor.b = tintingColor.b;
		returnColor.g = tintingColor.g;
		returnColor.rgb *= returnColor.a;
	}

	return returnColor;
}
