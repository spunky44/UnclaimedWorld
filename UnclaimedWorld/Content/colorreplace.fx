float FromHue;
float ToHue;

sampler TextureSampler : register(s0);
float4 RGB_to_HSV (float4 color)
{
	float r, g, b, delta;
	float colorMax, colorMin;
	float h=0, s=0, v=0;
	float4 hsv=0;
	r = color[0];
	g = color[1];
	b = color[2];
	colorMax = max (r,g);
	colorMax = max (colorMax,b);
	colorMin = min (r,g);
	colorMin = min (colorMin,b);
	v = colorMax; // this is value
	
	if (colorMax != 0)
	{
		s = (colorMax - colorMin) / colorMax;
	}
	if (s != 0) // if not achromatic
	{
		delta = colorMax - colorMin;
		if (r == colorMax)
		{
			h = (g-b)/delta;
		}
		else if (g == colorMax)
		{
			h = 2.0 + (b-r) / delta;
		}
		else // b is max
		{
			h = 4.0 + (r-g)/delta;
		}
		h *= 60;
		if( h < 0)
		{
			h +=360;
		}
		hsv[0] = h / 360.0; // moving h to be between 0 and 1.
		hsv[1] = s;
		hsv[2] = v;
	}
	return hsv;
}



float4 HSV_to_RGB (float4 hsv)
{
	float4 color=0;
	float f,p,q,t;
	float h,s,v;
	float r=0,g=0,b=0;
	float i;
	if (hsv[1] == 0)
	{
		if (hsv[2] != 0)
		{
			color = hsv[2];
		}
	}
	else
	{
		h = hsv.x * 360.0;
		s = hsv.y;
		v = hsv.z;
		if (h == 360.0)
		{
			h=0;
		}
		h /=60;
		i = floor (h);
		f = h-i;
		p = v * (1.0 - s);

		q = v * (1.0 - (s * f));
		t = v * (1.0 - (s * (1.0 -f)));
		
		if (i == 0)
		{
			r = v;
			g = t;
			b = p;
		}
		else if (i == 1)
		{
			r = q;
			g = v;
			b = p;
		}
		else if (i == 2)
		{
			r = p;
			g = v;
			b = t;
		}
		else if (i == 3)
		{
			r = p;
			g = q;
			b = v;
		}
		else if (i == 4)
		{
			r = t;
			g = p;
			b = v;
		}
		else if (i == 5)
		{
			r = v;
			g = p;
			b = q;
		}
		color.r = r;
		color.g = g;
		color.b = b;
	}
	return color;
}

float4 PixelShader(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the texture color.
    float4 tex = tex2D(TextureSampler, texCoord);
    
    float4 hsv = RGB_to_HSV(tex);
    
    // Convert it to greyscale. The constants 0.3, 0.59, and 0.11 are because
    // the human eye is more sensitive to green light, and less to blue.
    //float greyscale = dot(tex.rgb, float3(0.3, 0.59, 0.11));
    
    // The input color alpha controls saturation level.
    //tex.rgb = lerp(greyscale, tex.rgb, color.a * 4);
    
    float delta = hsv[0] - FromHue;
   
    if (abs(delta) <= 0.1f)
    {
		hsv[0] = (ToHue + delta) % 1.0;
    }
    
    tex.rgb = HSV_to_RGB(hsv);
    
    return tex;
}



technique ReplaceColor
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShader(); // set to version 3!!!
    }
}


