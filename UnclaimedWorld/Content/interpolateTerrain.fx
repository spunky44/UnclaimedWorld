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
float xTime;
float xWindForce;
float3 xWindDirection;
float specularIntensity;
//float specularPower;

Texture xReflectionMap;
sampler ReflectionSampler = sampler_state { texture = <xReflectionMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xRefractionMap;
sampler RefractionSampler = sampler_state { texture = <xRefractionMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xWaterBumpMap;
sampler WaterBumpMapSampler = sampler_state { texture = <xWaterBumpMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};


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
    float2 BumpMapSamplingPos        : TEXCOORD2;
    float4 RefractionMapSamplingPos : TEXCOORD3;
    float4 Position3D                : TEXCOORD4;
    float WaterDepth				: TEXCOORD5;
    float4 WaterColor                : TEXCOORD6;
    float4 BottomTint                : TEXCOORD7;
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
    moveVector.y += xTime*xWindForce;    
    Output.BumpMapSamplingPos = moveVector/xWaveLength; //  bumpTex;  //   
	Output.WaterDepth = input.waterDepth;
	Output.WaterColor = input.waterColor;
	Output.BottomTint = input.bottomTint;
	
    return Output;
}

PixelShaderOutput WaterPS(VertexShaderOutput PSIn)
{
    PixelShaderOutput Output = (PixelShaderOutput)0;  
    
    float4 bumpColor = tex2D(WaterBumpMapSampler, PSIn.BumpMapSamplingPos);
    float3 eyeVector = normalize(xCamPos - PSIn.Position3D);

	float3 normalVector = -(bumpColor.rgb-0.5f)*2.0f; //-(bumpColor.rbg-0.5f)*2.0f; //
  

	
	float2 perturbation = xWaveHeight*(bumpColor.rg - 0.5f)*2.0f; //  xWaveHeight*(bumpColor.rb - 0.5f)*2.0f; // 
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
	 float specular = dot(normalize(reflectionVector), normalize(eyeVector));
	 //specular = pow(specular, 256); 
	 // specular = specularIntensity * specular^specularPower
	 specular = specularIntensity * pow(specular, 40); //32);   
	 	 
	 Output.Color.rgb += specular;  

    //Output.Color = reflectiveColor; //combinedColor; //
    
    return Output;
}

technique Water
{
    pass Pass0
    {
        VertexShader = compile vs_1_1 WaterVS();
        PixelShader = compile ps_2_0 WaterPS();
    }
}

