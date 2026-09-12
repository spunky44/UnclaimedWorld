//------- Constants --------
/*
float4x4 xView;
//float4x4 xReflectionView;
float4x4 xProjection;
*/

float4x4 xWorld;

float4x4 reflectWorldViewProjection;
float4x4 refractWorldViewProjection;

float3 xLightDirection;
float xAmbient;
bool xEnableLighting;
float xWaveLength;
float xWaveHeight;
float3 xCamPos;
float3 camPositionSpecularityHack;
float xTime;
float xWindForce;
float3 xWindDirection;
float specularIntensity;
//float specularPower;

Texture xReflectionMap;
sampler ReflectionSampler = sampler_state { texture = <xReflectionMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xRefractionMap;
sampler RefractionSampler = sampler_state { texture = <xRefractionMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};


Texture xClipMap;
sampler ClipSampler = sampler_state { texture = <xClipMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};


Texture xWaterBumpMap;
sampler WaterBumpMapSampler = sampler_state { texture = <xWaterBumpMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

Texture xWaterBumpMapLarge;
sampler WaterBumpMapLargeSampler = sampler_state { texture = <xWaterBumpMapLarge> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

//------- Technique: Water --------
struct VertexShaderInput
{
    float4 inPos		: POSITION;
    float2 inTex		: TEXCOORD0; 
    float2 bumpTex		: TEXCOORD1; 
    float waterDepth	: TEXCOORD2;
	float4 waterColor	: TEXCOORD3;
	float4 bottomTint	: TEXCOORD4;
};

struct VertexShaderOutput
{
    float4 Position                 : POSITION;
    float4 ReflectionMapSamplingPos    : TEXCOORD1;
    //float2 BumpMapSamplingPos        : TEXCOORD2;
	float4 BumpMapSamplingPos        : TEXCOORD2;
    float4 RefractionMapSamplingPos : TEXCOORD3;
    float4 Position3D                : TEXCOORD4;
    float WaterDepth				: TEXCOORD5;
    float4 WaterColor                : TEXCOORD6;
    float4 BottomTint                : TEXCOORD7;
	//float2 BumpMapLargeSamplingPos   : TEXCOORD8;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

VertexShaderOutput WaterVS(VertexShaderInput input) //float4 inPos : POSITION, float2 inTex: TEXCOORD, float2 bumpTex: TEXCOORD1, float waterDepth: TEXCOORD2)
{    
    VertexShaderOutput Output = (VertexShaderOutput)0;

	// for refraction
 /*   float4x4 refractViewProjection = mul (xView, xProjection);
    float4x4 refractWorldViewProjection = mul (xWorld, refractViewProjection);
    */
    
    // for reflection
  /*  float4x4 preReflectionViewProjection = mul (xReflectionView, xProjection);
    float4x4 preWorldReflectionViewProjection = mul (xWorld, preReflectionViewProjection);
*/

    Output.Position = mul(input.inPos, refractWorldViewProjection);   
    Output.ReflectionMapSamplingPos = mul(input.inPos, reflectWorldViewProjection);    
    Output.RefractionMapSamplingPos = mul(input.inPos, refractWorldViewProjection);
    Output.Position3D = mul(input.inPos, xWorld);        
    
    float3 windDir = normalize(xWindDirection);    
    float3 perpDir = cross(xWindDirection, float3(0,1,0));
    
 //   float ydot = dot(inTex, xWindDirection.xz);
	float ydot = dot(input.bumpTex, xWindDirection.xz);
 //   float xdot = dot(inTex, perpDir.xz);
	float xdot = dot(input.bumpTex, perpDir.xz);
	
    float2 moveVector = float2(xdot, ydot);
    moveVector.y += xTime * xWindForce;    

	float2 moveVectorLarge = float2(xdot, ydot);
    moveVectorLarge.y += 0.25 * xTime * xWindForce;    // move the large map slower...

    //Output.BumpMapSamplingPos = moveVector/xWaveLength; 
	
	Output.BumpMapSamplingPos.xy = moveVector/xWaveLength; 
	Output.BumpMapSamplingPos.zw = moveVectorLarge/xWaveLength; 

	Output.WaterDepth = input.waterDepth;
	Output.WaterColor = input.waterColor;
	Output.BottomTint = input.bottomTint;
	
    return Output;
}

PixelShaderOutput WaterPS(VertexShaderOutput PSIn)
{
    PixelShaderOutput Output = (PixelShaderOutput)0;  
    
    float4 bumpColor = tex2D(WaterBumpMapSampler, PSIn.BumpMapSamplingPos.xy);
	//float4 bumpColor = tex2D(WaterBumpMapLargeSampler, PSIn.BumpMapSamplingPos.xy);
	float4 bumpLargeColor = tex2D(WaterBumpMapLargeSampler, PSIn.BumpMapSamplingPos.zw);

	float3 eyeVector = normalize(xCamPos - PSIn.Position3D.xyz); // #MONOCHANGE // normalize(xCamPos - PSIn.Position3D);

	//float3 normalVector = -(bumpColor.rgb-0.5f)*2.0f; 
	float3 normalVector = -(bumpColor.rgb-0.5f)*2.0f + (-(bumpLargeColor.rgb-0.5f)*2.0f); 
	//float3 normalVector = -(bumpColor.rgb + bumpLargeColor.rgb - 0.5f)*2.0f; 
  	
	normalVector = normalize(normalVector); // important!!!

	float2 perturbation = 0.5f * xWaveHeight*(bumpLargeColor.rg - 0.5f)*2.0f + xWaveHeight*(bumpColor.rg - 0.5f)*2.0f;
	//float2 perturbation = 0.5f * xWaveHeight*(bumpColor.rg - 0.5f)*2.0f; 
	//float2 perturbation = xWaveHeight*(bumpColor.rg + bumpLargeColor.rg - 0.5f)*2.0f; 

	float depthFactor = clamp(PSIn.WaterDepth / 36.0, 0.1, 1.0);
	perturbation *= depthFactor; // 0;
    
	float2 ProjectedTexCoords;
	ProjectedTexCoords.x = PSIn.ReflectionMapSamplingPos.x/PSIn.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	ProjectedTexCoords.y = -PSIn.ReflectionMapSamplingPos.y/PSIn.ReflectionMapSamplingPos.w/2.0f + 0.5f;        
	float2 perturbatedTexCoords = ProjectedTexCoords + perturbation;
	float4 reflectiveColor = tex2D(ReflectionSampler, perturbatedTexCoords);
    
	float2 ProjectedRefrTexCoords;
	ProjectedRefrTexCoords.x = PSIn.RefractionMapSamplingPos.x/PSIn.RefractionMapSamplingPos.w/2.0f + 0.5f;
	ProjectedRefrTexCoords.y = -PSIn.RefractionMapSamplingPos.y/PSIn.RefractionMapSamplingPos.w/2.0f + 0.5f;    

	float4 ClipColor = tex2D(ClipSampler, ProjectedRefrTexCoords);
	if(ClipColor.x + ClipColor.y + ClipColor.z == 0) //Clipping black areas, TODO: look for the propper way to do this
	{
		clip(-1);
	}

    float2 perturbatedRefrTexCoords = ProjectedRefrTexCoords + perturbation; // * depthFactor;  
    float4 refractiveColor = tex2D(RefractionSampler, perturbatedRefrTexCoords);
    refractiveColor.rgb *= PSIn.BottomTint.rgb;

	float fresnelTerm = dot(eyeVector, normalVector);    
    
	float4 dullColor = PSIn.WaterColor; // float4(0.3f, 0.3f, 0.5f, 1.0f);
   // float4 deepColor = float4(0.15f, 0.15f, 0.3f, 1.0f);
    
   // refractiveColor = lerp(refractiveColor, deepColor, saturate((PSIn.Position3D.z - 1000.0) / 150.0));
    
	float4 combinedColor = lerp(reflectiveColor, refractiveColor, fresnelTerm); 
    
    // don't start with transparent at the water edge:
    float waterColorContribution = clamp(0.1f + PSIn.WaterDepth / 36.0 * dullColor.a, 0, PSIn.WaterColor.a); //1.0f);
    
    dullColor.a = 1.0f; // we don't use the water color alpha in output. Refraction contribution is how we see the bottom.
    
	// Output.Color = lerp(combinedColor, dullColor, 0.2f);  
	 Output.Color = lerp(combinedColor, dullColor, waterColorContribution);  
     

	 float3 reflectionVector = reflect(xLightDirection, normalVector);
	 float3 specularityEyeVector = normalize(xCamPos - PSIn.Position3D.xyz + camPositionSpecularityHack); //#MONOCHANGE //normalize(xCamPos - PSIn.Position3D + camPositionSpecularityHack);

	// float specular = dot(normalize(reflectionVector), normalize(eyeVector));
	 float specular = dot(normalize(reflectionVector), specularityEyeVector);

	 //specular = specularIntensity * pow(specular, 40); 
	 specular = specularIntensity * pow(abs(specular), 40); // #MONOCHANGE

	 Output.Color.rgb += specular;  

	// combinedColor = lerp(reflectiveColor, refractiveColor, 0.5); 
    
  //  Output.Color = 0.5 * (bumpColor + bumpLargeColor); // combinedColor; // reflectiveColor; //float4(specular, specular, specular, 1); // refractiveColor; // reflectiveColor; //combinedColor; //
   // Output.Color.a = 1;
   
	
    return Output;
}

technique Water
{
    pass Pass0
    {
		VertexShader = compile vs_4_0_level_9_3 /*vs_1_1*/ WaterVS();
		PixelShader = compile ps_4_0_level_9_3 /*ps_2_0*/ WaterPS();
    }
}




