sampler s;
sampler s2 : register(s1);
sampler s3 : register(s2);
bool HasFog = true;
float Alpha = 1;
float ObjectHeight;
float NearDepth;
float FarDepth;
float2 Viewport;
float2 ChunkTex;
float MinDepthDiff;
float4 FogColor = float4(70/255.0,130/255.0,180/255.0, 1); //steelblue	(70, 130, 180)
float4 SourceRectangle;
float DayTime;
float4 AmbientLight;
float4 Overlay = float4(1,1,1,1);
float TileWidth;// = 32.0;
float TileHeight;// = 32;//40.0;
float SpritesheetWidth;// = 256.0;
float SpritesheetHeight;// = 768.0;//1024.0;//768.0;
float TileVertEnsureDraw;
float4x4 Projection;

struct VertexShaderInput
{
    float4 Position : POSITION;
	float4 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 TexCoord : TEXCOORD0;
	float4 ScreenCoord : TEXCOORD1;
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float Depth : TEXCOORD2;
};

VertexShaderOutput VSObj(VertexShaderInput input)
{
	VertexShaderOutput output;
	input.Position.xy -= 0.5f;
	input.Position.xy = input.Position.xy / Viewport;
    input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	//output.Depth = float4(1-input.Position.z/input.Position.w, 0, 0, 1);
	output.Depth.x = input.Position.z/input.Position.w;
	output.Position = input.Position;
	output.ScreenCoord = input.Position;
	//float4 mulpos = mul(float4(0,0,input.Position.z,input.Position.w), Projection);
	//output.Depth.x = 0;// 1-mulpos.z/mulpos.w;;
	return output;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	input.Position.xy -= 0.5f;
	input.Position.xy = input.Position.xy / Viewport;
    input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	//output.Depth = float4(1-input.Position.z/input.Position.w, 0, 0, 1);
	output.Depth.x = 1-input.Position.z/input.Position.w;
	output.Position = input.Position;
	output.ScreenCoord = input.Position;
	return output;
}

VertexShaderOutput VertexShaderToFile(VertexShaderInput input)
{
	VertexShaderOutput output;
	input.Position.xy -= 0.5f;
	input.Position.xy = input.Position.xy / ChunkTex;
    input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	//output.Depth = float4(1-input.Position.z/input.Position.w, 0, 0, 1);
	//output.Depth = 1-input.Position.z/input.Position.w;
	output.Depth.x = 1-input.Position.z/input.Position.w;
	output.Position = input.Position;
	output.ScreenCoord = input.Position;
	return output;
}

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Depth : COLOR1;
	float4 Light : COLOR2;
};

struct PixelShaderOutputFile
{
	float4 Color : COLOR0;
	float4 Depth : COLOR1;
	float4 Light : COLOR2;
	float4 Mouse : COLOR3;
};

PixelShaderOutput PSObj(VertexShaderOutput input, out float DepthBuffer : DEPTH0)
{
	float4 color = tex2D(s, input.TexCoord);
	//float4 light = float4(input.Color.r, input.Color.r, input.Color.r, input.Color.a); //1);
	float l = input.Color.r;
	float a = input.Color.a;
	clip(l==0?-1:1); //clip if dark
	float4 fog = FogColor*color.a;//*c.g
	PixelShaderOutput output;
	
	if(HasFog)
		//output.Color = color*light*(1-input.Color.g) + fog*input.Color.g;
		output.Color = color*l*(1-input.Color.g) + fog*input.Color.g; //-----probably wrong

	if(color.a==0)
	{
		output.Depth = 0;//float4(0,1,0,0);
		//output.Depth = float4(0,0,0,1);
		output.Light = float4(0,0,0,0);
		DepthBuffer = 1; //output.Depth;
		output.Color = 0;//float4(DepthBuffer, DepthBuffer, DepthBuffer, DepthBuffer);//0;
		return output;
	}

	//float d = NearDepth + (1 - input.Depth.x) * (FarDepth - NearDepth);
	//float dhigh = NearDepth + (ObjectHeight) * (FarDepth - NearDepth);
	//float texVertPos = (input.TexCoord.y - SourceRectangle.y) / (SourceRectangle.w);
	//float t = d + (1 - (texVertPos)) * (dhigh - d);

	float final = input.Depth.x; //t;
	output.Depth = final + float4(0,0,0,1);
	output.Light = float4(l,l,l,l);//light;//-----probably wrong

	DepthBuffer = output.Depth;
	//float4 c = color*light*Overlay; //*Alpha
	float4 c = color*(lerp(AmbientLight, float4(1,1,1,1), l))*Overlay*a;//*Alpha;    //-----probably wrong
	output.Color = c;//float4(c.r,c.g,c.b,color.a); //color; //float4(DepthBuffer, DepthBuffer, DepthBuffer, 1);
	return output;
}

PixelShaderOutput PixelShaderObjects(VertexShaderOutput input, out float DepthBuffer : DEPTH0)
{
	float4 color = tex2D(s, input.TexCoord);
//	float4 light = float4(input.Color.r, input.Color.r, input.Color.r, 1);
	float l = input.Color.r;
	float4 fog = FogColor*color.a;//*c.g
	PixelShaderOutput output;
	clip(l==0?-1:1);
	if(HasFog)
		//output.Color = color*light*(1-input.Color.g) + fog*input.Color.g;
		output.Color = color*l*(1-input.Color.g) + fog*input.Color.g;//-----probably wrong

	if(color.a==0)
	{
		output.Depth = 1;//float4(0,1,0,0);
		//output.Depth = float4(0,0,0,1);
		output.Light = float4(0,0,0,0);
		DepthBuffer = 1; //output.Depth;
		output.Color = 0;//float4(DepthBuffer, DepthBuffer, DepthBuffer, DepthBuffer);//0;
		return output;
	}

	float d = NearDepth + (1 - input.Depth.x) * (FarDepth - NearDepth);
	float dhigh = NearDepth + (ObjectHeight) * (FarDepth - NearDepth);
	float texVertPos = (input.TexCoord.y - SourceRectangle.y) / (SourceRectangle.w);
	float t = d + (1 - (texVertPos)) * (dhigh - d);
	float final = t;
	output.Depth = final + float4(0,0,0,1);
	output.Light = float4(l,l,l,l);// light;//-----probably wrong

	DepthBuffer = output.Depth;
	//float4 c = Alpha*color*Overlay*(lerp(AmbientLight, float4(1,1,1,1), l));	//-----probably wrong
	float4 c = Alpha*color*Overlay*AmbientLight;	//-----probably wrong
	output.Color = c;//float4(c.r,c.g,c.b,color.a); //color; //float4(DepthBuffer, DepthBuffer, DepthBuffer, 1);
	return output;
}

PixelShaderOutput PixelShaderScenery(VertexShaderOutput input, out float DepthBuffer : DEPTH0)
{
	float4 color = tex2D(s, input.TexCoord);
	float4 light = float4(input.Color.r, input.Color.r, input.Color.r, 1);
	float4 fog = FogColor*color.a;//*c.g
	PixelShaderOutput output;
	
	if(HasFog)
		output.Color = color*light*(1-input.Color.g) + fog*input.Color.g;

	if(color.a==0)
	{
		output.Depth = 1;//float4(0,1,0,0);
		
		output.Light = float4(0,0,0,0);
		DepthBuffer = output.Depth;
		output.Color = float4(0, 0, 1, 1);//float4(DepthBuffer, DepthBuffer, DepthBuffer, DepthBuffer);//0;
		return output;
	}
	float d = input.Depth.x;//NearDepth + (1 - input.Depth.x) * (FarDepth - NearDepth);

	output.Depth = d + float4(0,0,0,1);
	output.Light = light;

	DepthBuffer = output.Depth;
	output.Color = float4(d,d,d,1);//float4(DepthBuffer, DepthBuffer, DepthBuffer, 1);
	return output;
}

float4 PixelShaderTileHighlight(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord);
	float xx = SourceRectangle.x + input.TexCoord.x * SourceRectangle.z;
	float yy = SourceRectangle.y + input.TexCoord.y * SourceRectangle.w;

	float4 sampledDepth = tex2D(s2, float2(xx, yy));
	float4 derp = input.Depth.x;

	float4 output;

	clip(color.a == 0?-1:1);
	clip(sampledDepth.r == input.Depth.x ? -1 : 1);
	float d = max(input.Depth.x, sampledDepth.r);
	//output = float4(d, d, d, 1);//sampledDepth + float4(0,0,0,1);//float4(sampledDepth.r, input.Depth.x, 0, 1); //Alpha*color;//*AmbientLight;//DONT RENDER WITH LIGHT, RENDER LATER WITH SMOOTH LIGHT SHADER *light;
	//output = float4(derp.r, derp.g, derp.b, 1);
	if(sampledDepth.r < input.Depth.x)
		output = float4(1, 0,0,1);
	else
		output = float4(0,1, 0, 1);
	return output;
}

PixelShaderOutput PixelShaderRealTime(VertexShaderOutput input, out float DepthBuffer : DEPTH0)
{
	float4 color = tex2D(s, input.TexCoord);
	float4 derp = input.Depth.x;
	//float4 light = 0;//float4(input.Color.r, input.Color.g, input.Color.b, 1);
	float4 fog = FogColor*color.a;//*c.g
	float d;
	float l = 0;
	PixelShaderOutput output;

	clip(color.a == 0?-1:1);

	//CHECK WHICH TILE FACE IS BEING DRAWN
	float x = (input.TexCoord.x * SpritesheetWidth) % TileWidth; //32.0;
	float y = (input.TexCoord.y * SpritesheetHeight) % TileHeight; //32.0;32.0;
	float xxx = x / TileWidth;//32.0; //in-tile coords
	float yyy = y / TileHeight;//32.0;


	float4 face = tex2D(s3, float4(xxx, yyy, 0, 0));
	//if(face.r>0)
	if(yyy< TileVertEnsureDraw || face.r>0)
		//light = float4(input.Color.b, input.Color.b, input.Color.b, input.Color.a);//1); //input.Color.a);
		l = input.Color.b;
	else if(face.g>0)
		//light = float4(input.Color.g, input.Color.g, input.Color.g, input.Color.a);//1); //input.Color.a);//1);
		l = input.Color.g;
	else if(face.b>0)
		//light = float4(input.Color.r, input.Color.r, input.Color.r, input.Color.a);//1);//input.Color.a);//1);
		l = input.Color.r;

	//	clip(l==0?-1:1); //clip if light is zero

//	output.Color = Alpha*color*light*AmbientLight;//*input.Color.a;//*AmbientLight;//DONT RENDER WITH LIGHT, RENDER LATER WITH SMOOTH LIGHT SHADER *light;
output.Color = input.Color.a*Alpha*color*(lerp(AmbientLight, float4(1,1,1,1), l));

	if(HasFog)
		output.Color = color*l*(1-input.Color.g) + fog*input.Color.g;//*light

		//d = NearDepth + (derp.r) * (FarDepth - NearDepth);
		d = derp.r;// FarDepth;
		output.Depth = d + float4(0,0,0,1);

		output.Light = float4(l,l,l,l);// light;

	DepthBuffer = output.Depth;

	return output;
}

PixelShaderOutput PixelShaderRealTime2(VertexShaderOutput input, out float DepthBuffer : DEPTH0)
{
	float4 color = tex2D(s, input.TexCoord);
	float4 derp = input.Depth.x;
	float4 light = 0;//float4(input.Color.r, input.Color.g, input.Color.b, 1);
	float4 fog = FogColor*color.a;//*c.g
	float d;
	PixelShaderOutput output;

	clip(color.a == 0?-1:1);

	//CHECK WHICH TILE FACE IS BEING DRAWN
	float x = (input.TexCoord.x * 256.0) % 32.0;
	float y = (input.TexCoord.y * 768.0) % 32.0;
//	float xx = input.TexCoord.x - x;
//	float yy = input.TexCoord.y - y;
	float xxx = x / 32.0; //in-tile coords
	float yyy = y / 32.0;
	float4 face = tex2D(s3, float4(xxx, yyy, 0, 0));
	//if(face.r>0)
	if(yyy<0.5 || face.r>0)
		light = float4(input.Color.b, input.Color.b, input.Color.b, 1);
	else if(face.g>0)
		light = float4(input.Color.g, input.Color.g, input.Color.g, 1);
	else if(face.b>0)
		light = float4(input.Color.r, input.Color.r, input.Color.r, 1);

	output.Color = Alpha*color*light;//*AmbientLight;//DONT RENDER WITH LIGHT, RENDER LATER WITH SMOOTH LIGHT SHADER *light;
	if(HasFog)
		output.Color = color*light*(1-input.Color.g) + fog*input.Color.g;
//	if(color.a>0)
//	{
		//d = NearDepth + (derp.r) * (FarDepth - NearDepth);
		d = derp.r;// FarDepth;
		output.Depth = d + float4(0,0,0,1);
		output.Light = light;
//	}
//	else
//	{
//		output.Depth = 1;//float2(0,0);//,0,1);
//		output.Light = float4(0,0,0,0);
//	}
	DepthBuffer = output.Depth;

	return output;
}

PixelShaderOutput PixelShaderThumbnail(VertexShaderOutput input, out float DepthBuffer : DEPTH0)
{
	float4 color = tex2D(s, 0);//input.TexCoord);
	float4 derp = input.Depth.x;
	float4 light = float4(input.Color.r, input.Color.r, input.Color.r, 1);
	float4 fog = FogColor*color.a;//*c.g
	float d;
	PixelShaderOutput output;

	output.Color = Alpha*color*light;
	if(HasFog)
		output.Color = color*light*(1-input.Color.g) + fog*input.Color.g;
	if(color.a>0)
	{
		//d = NearDepth + (derp.r) * (FarDepth - NearDepth);
		d = derp.r;// FarDepth;
		output.Depth = d + float4(0,0,0,1);
		output.Light = light;
	}
	else
	{
		output.Depth = 1;//float2(0,0);//,0,1);
		output.Light = float4(0,0,0,0);
	}
	DepthBuffer = output.Depth;

	return output;
}

PixelShaderOutput PixelShaderMap(VertexShaderOutput input, out float DepthBuffer : DEPTH0)
{
	float4 color = tex2D(s, input.TexCoord);
	float4 derp = tex2D(s2, input.TexCoord);
	float4 light = float4(input.Color.r, input.Color.r, input.Color.r, 1);
	float4 fog = FogColor*color.a;//*c.g
	float d;
	PixelShaderOutput output;

	output.Color = color*light;
	if(HasFog)
		output.Color = color*light*(1-input.Color.g) + fog*input.Color.g;
	if(color.a>0)
	{
		//output.Depth = input.Depth.r + float4(0,0,0,1);
		//float d = input.Depth.r * (FarDepth - NearDepth);
		//output.Depth = derp + float4(0,0,0,1);
		d = NearDepth + (derp.r) * (FarDepth - NearDepth);
		output.Depth = d + float4(0,0,0,1);
		output.Light = light;
	}
	else
	{
		output.Depth = 1;//float2(0,0);//,0,1);
		output.Light = float4(0,0,0,0);
	}
	DepthBuffer = output.Depth;
	//output.Color = float4(DepthBuffer, DepthBuffer, DepthBuffer, color.a);

	return output;
}

PixelShaderOutputFile PixelShaderToFile(VertexShaderOutput input)
{
	float4 color = tex2D(s, input.TexCoord);
	float4 light = float4(input.Color.r, input.Color.r, input.Color.r, 1);
	float4 fog = FogColor*color.a;//*c.g
	PixelShaderOutputFile output;

	output.Color = color;//*light;
	//output.Mouse = float4(input.Color.r, input.Color.g, input.Color.b, color.a);
	//output.Mouse = float4(input.Color.r, input.Color.g, input.Color.b, 1);
	//if(HasFog)
	//	output.Color = color*light*(1-input.Color.g) + fog*input.Color.g;
	if(color.a>0)
	{
		//output.Depth = 1 - input.Depth.r + float4(0,0,0,1);
		output.Depth = 1 - input.Depth + float4(0,0,0,1);
		output.Light = light;
		output.Mouse = input.Color;
	}
	else
	{
		output.Depth = 0;//float4(0,1,0,0);
		output.Light = float4(0,0,0,0);
		output.Mouse = 0;//color;
	}
	return output;
}

PixelShaderOutput PixelShaderFunction2(float2 uv : TEXCOORD0, float4 c : COLOR0)
{
    // TODO: add your pixel shader code here.
	float4 color = tex2D(s, uv); 

	float4 fogColor = float4(70/255.0,130/255.0,180/255.0, 1); //steelblue	(70, 130, 180)
	float4 fog = fogColor*color.a;//*c.g
	float4 light = float4(c.r, c.r, c.r, 1);

	//float l = c.r/16.0;
	//float l2 = float4(l, l, l, 1);

	PixelShaderOutput output;
	output.Color = color*light;
	if(HasFog)
		output.Color = color*light*(1-c.g) + fog*c.g;
		
	if(color.a>0)
	{
		output.Depth = 1-c.b + float4(0,0,0,1);
		output.Light = light;
	}
	else
	{
		output.Depth = float4(0,0,0,0);//tex2D(s2, uv);
		output.Light = float4(0,0,0,0);
	}
	return output;
}



float4 PixelShaderFunction(float2 uv : TEXCOORD0, float4 c : COLOR0) : COLOR0
{
    // TODO: add your pixel shader code here.
	float4 color = tex2D(s, uv); 
    //return color;// + c.a*float4(0,0,c.r,0);
	//float4 fog = float4(0,0,c.g,0)*color.a;
	//float4 fogColor = float4(135.0/255, 206.0/255, 235.0/255, 1); //skyblue
	//float4 fogColor = float4(100.0/255, 149.0/255, 237.0/255, 1); //cornflowerblue
	//float4 fogColor = float4(0,0,0, 1); //black
	//float4 fogColor = float4(75/255.0,0,130/255.0, 1); //indigo	(75, 0, 130)
	float4 fogColor = float4(70/255.0,130/255.0,180/255.0, 1); //steelblue	(70, 130, 180)
	float4 fog = fogColor*color.a;//*c.g
	float4 light = float4(c.r, c.r, c.r, 1);
	//float4 light = float4(c.r * fogColor.r, c.r * fogColor.g, c.r * fogColor.b, 1);
	float l = c.r/16.0;
	float l2 = float4(l, l, l, 1);

	if(HasFog)
		return color*light*(1-c.g) + fog*c.g;

	return color*light;
	//return color*(1-c.g) + fog*c.g;
	//return color;

	//color = color*(1-c.g) + fog*c.g;
	//return color*(c.r) + fog*(1-c.r);
}

float4 BorderShader2(float2 uv : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(s, uv);
    float4 current = tex2D(s2, uv);
	float4 below = tex2D(s2, uv + float2(0, 3/Viewport.y)); // 0.005));
	float4 above = tex2D(s2, uv - float2(0, 3/Viewport.y)); //0.005));
	float bdif = current.r - below.r;// - current.r;
	float adif = current.r - above.r;// - current.r;
	if(bdif > MinDepthDiff/2.0) //0.008)
		return float4(0,0,0,1);//color - bdif*15;
	if(adif > MinDepthDiff/2.0) //0.008)
		return float4(0,0,0,1);//color - adif*15;
	return color;
}

float4 BorderShader(float2 uv : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(s, uv);
    float4 current = tex2D(s2, uv);
	float4 below = tex2D(s2, uv + float2(0, 3/Viewport.y)); // 0.005));
	float4 above = tex2D(s2, uv - float2(0, 3/Viewport.y)); //0.005));
	float bdif = below.b - current.b;
	float adif = above.b - current.b;

	return float4(0,0,1,1);

	//if(below.b > current.b + 0.0001) //0.01)
	if(bdif > 0.008)
		return color - bdif*5;
	if(adif > 0.008)
	//if(above.b > current.b + 0.0001) //0.01)
		return color - adif*5;

	return color;
}

float4 PixelShaderSmoothLight(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord);
	//return color;
	float4 light = tex2D(s3, input.TexCoord);
	//return color*light;
	float f = 16;
	float4 px1 = float4(-f / Viewport.x, f / Viewport.y, 0, 0);
	float4 px2 = float4(f / Viewport.x, f / Viewport.y, 0, 0);
	float4 px3 = float4(-f / Viewport.x, -f / Viewport.y, 0, 0);
	float4 px4 = float4(f / Viewport.x, -f / Viewport.y, 0, 0);

	float4 lightTopLeft = tex2D(s3, input.TexCoord + px1);
	float4 lightTopRight = tex2D(s3, input.TexCoord + px2);
	float4 lightBottomLeft = tex2D(s3, input.TexCoord + px3);
	float4 lightBottomRight = tex2D(s3, input.TexCoord + px4);
	//return color*lightTopLeft;
	return color*light*lightTopRight*lightTopLeft*lightBottomLeft*lightBottomRight;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderMap();
    }
}

technique Technique2
{
    pass Pass1
    {
        // TODO: set renderstates here.
		VertexShader = compile vs_2_0 VertexShaderToFile();
        PixelShader = compile ps_2_0 PixelShaderToFile();
    }
}

technique Technique3
{
    pass Pass1
    {
        // TODO: set renderstates here.
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderToFile();
    }
}

technique RealTime
{
    pass Pass1
    {
        // TODO: set renderstates here.
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderRealTime();
    }
}

technique TileHighlight
{
    pass Pass1
    {
        // TODO: set renderstates here.
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderTileHighlight();
    }
}

technique TechniqueObjects
{
    pass Pass1
    {
        // TODO: set renderstates here.
		//AlphaBlendEnable = TRUE;
        //DestBlend = INVSRCALPHA;
        //SrcBlend = SRCALPHA;
		VertexShader = compile vs_2_0 VSObj();//VertexShaderFunction();
        PixelShader = compile ps_2_0 PSObj();//PixelShaderObjects();
    }
}

technique TechniqueScenery
{
    pass Pass1
    {
        // TODO: set renderstates here.
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderScenery();
    }
}

technique BorderShading
{
	pass Pass1
    {
        // TODO: set renderstates here.
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 BorderShader2();
    }
}


technique Borders
{
    pass Pass1
    {
        // TODO: set renderstates here.

        PixelShader = compile ps_2_0 BorderShader();
    }
}

technique Thumbnail
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderThumbnail();
	}
}

technique SmoothLight
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderSmoothLight();
	}
}