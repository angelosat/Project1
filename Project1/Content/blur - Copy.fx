sampler s;
sampler s1 : register(s1);
sampler s2 : register(s2);
sampler s3 : register(s3);
float4x4 World;
float4x4 View;
float4x4 Projection;
float2 Viewport;

float BlockWidth;
float BlockHeight;
float AtlasWidth;
float AtlasHeight;

float NearDepth;
float FarDepth;
float DepthRange;

float DepthResolution;
float BorderThicknessPx = 1;
float BorderThickness = 0.0005; //0.001;
float2 BorderResolution;
float OutlineThreshold = 0.02; // if depth difference between target and surrounding pixels is larger than this, shade the pixel as part of the sprite's outline
float Zoom = 1;
float2 OutlineTolerance;
float2 GetBorderThickness()
{
	return float2(BorderThicknessPx / Viewport.x, BorderThickness / Viewport.y) * Zoom;
}

float4 AmbientLight;

float TileVertEnsureDraw;//for irregular blocks that extend upwards outside the cube and into the transparent area
// TODO: add effect parameters here.

struct MyVertexInput
{
	float4 Position : POSITION;
	float4 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
	float4 Light : COLOR1;
	/*float Depth : TEXCOORD1;*/
};
struct MyVertexOutput
{
	float4 TexCoord : TEXCOORD0;
	float4 Position : POSITION;// POSITION0;
	float4 Color : COLOR0;
	float4 Light : COLOR1;
	float Depth : TEXCOORD1;
	float2 ScreenPosition : TEXCOORD2;
	// TODO: add vertex shader outputs such as colors and texture
	// coordinates here. These values will automatically be interpolated
	// over the triangle, and provided as input to your pixel shader.
};
struct VertexShaderInput
{
    float4 Position : POSITION;
	float4 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};
struct VertexShaderOutput
{
	float4 TexCoord : TEXCOORD0;
	float4 Position : POSITION;// POSITION0;
	float4 Color : COLOR0;
	float Depth : TEXCOORD1;
	float2 ScreenPosition : TEXCOORD2;
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};
struct MyPixelOutput
{
	float4 Color : COLOR0;
	float4 Light : COLOR1;
	float4 Depth : COLOR2;
	float DepthBuffer : DEPTH0;
};

MyVertexOutput MyVertexShader(MyVertexInput input)
{
	MyVertexOutput output;

	input.Position.xy -= 0.5f;
	input.Position.xy /= Viewport;
	input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);

	//float4 worldPosition = mul(input.Position, World);
	//float4 viewPosition = mul(worldPosition, View);
	output.Position = input.Position;// mul(viewPosition, Projection);
	//old output.Position.z /= (NearDepth - FarDepth);// ((FarDepth - NearDepth));
	//old output.Position.z = (FarDepth + output.Position.z) / (FarDepth + NearDepth);

	output.Position.z = (output.Position.z - FarDepth) / (NearDepth - FarDepth);
	//output.Position.w /= (output.Position.z*2);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	output.Light = input.Light;
	//output.Depth.x = 1 - input.Position.z / input.Position.w;
	//output.Depth.x = input.Position.z / input.Position.w;
	output.Depth.x = 1 - output.Position.z / output.Position.w;
	output.ScreenPosition = input.Position.xy;
	// TODO: add your vertex shader code here.

	return output;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	input.Position.xy -= 0.5f;
	input.Position.xy /= Viewport;
	input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);

	//float4 worldPosition = mul(input.Position, World);
	//float4 viewPosition = mul(worldPosition, View);
	output.Position = input.Position;// mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	//output.Depth.x = 1 - input.Position.z / input.Position.w;
	output.Depth.x = input.Position.z / input.Position.w;
    // TODO: add your vertex shader code here.
	output.ScreenPosition = input.Position.xy;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input, out float depth : DEPTH0) : COLOR0
{
	// TODO: add your pixel shader code here.
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);
	depth = input.Depth.r;

	// get local UV
	float u = ((input.TexCoord.x * AtlasWidth) % BlockWidth) / BlockWidth;
	float v = ((input.TexCoord.y * AtlasHeight) % BlockHeight) / BlockHeight;
	float2 uv = float2(u, v);
	// find current block face
	float4 face = tex2D(s2, uv);

	float l = 0;
	if (face.r > 0)
		l = input.Color.b;
	else if (face.g > 0)
		l = input.Color.g;
	else if (face.b > 0)
		l = input.Color.r;

	float a = input.Color.a;
	return (1-a)*color + float4(a*color.rgb*input.Color.rgb, color.a);
	//color = float4(depth, depth, depth, color.a);
	
	/*float3 rgb = color.rgb * input.Color.rgb * color.a;*/
	/*float3 rgb = (1 - a) * color.rgb + a * input.Color.rgb;*/
	float r = (1 - a) * color.r + a * input.Color.r;
	float g = (1 - a) * color.g + a * input.Color.g;
	float b = (1 - a) * color.b + a * input.Color.b;
	return float4(r, g, b, color.a);// float4(input.Color.rgb, 1);// float4(1, 0, 0, 1);
}

float4 BlurMore(VertexShaderOutput input) : COLOR0
{
	// TODO: add your pixel shader code here.
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	color += tex2D(s, input.TexCoord.xy + float2(0.02, 0));
	color += tex2D(s, input.TexCoord.xy - float2(0.02, 0));
	color += tex2D(s, input.TexCoord.xy + float2(0, 0.02));
	color += tex2D(s, input.TexCoord.xy - float2(0, 0.02));
	color /= 5;
	//return float4(input.TexCoord.x, input.TexCoord.y, 0, 1);// color;
	//return color;
	return float4(input.Color.r, input.Color.g, input.Color.b, color.a);
}

float4 SolidBorder(VertexShaderOutput input) : COLOR0
{
	// TODO: add your pixel shader code here.
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	return float4(1, 0, 0, color.a);
}

float4 ColorKey(VertexShaderOutput input) : COLOR0
{
	// TODO: add your pixel shader code here.
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	//if (color == float4(0, 1, 0, 1))
	if (color.g == 1 && !any(color.rba))
		return float4(0, 0, 0, 0);
	clip(color.a == 0 ? -1 : 1);
	return color;
}

float4 LightFunction(VertexShaderOutput input, out float depth : DEPTH0) : COLOR0
{
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);
	depth = input.Depth.x;

	// get local UV
	float u = ((input.TexCoord.x * AtlasWidth) % BlockWidth) / BlockWidth;
	float v = ((input.TexCoord.y * AtlasHeight) % BlockHeight) / BlockHeight;
	float2 uv = float2(u, v);

		// find current block face
		float4 face = tex2D(s2, uv);
		float l = 0;

	if (v < TileVertEnsureDraw || face.r > 0)
		l = input.Color.b;
	else if (face.g > 0)
		l = input.Color.g;
	else if (face.b > 0)
		l = input.Color.r;
	//clip(l == 0 ? -1 : 1); //clip if light is zero
	return float4(l, l, l, 1);
}
float GetDepth(float depthMap, float inputDepth)
{
	//return depthMap*DepthResolution + inputDepth;
	float adjustedDepth = (depthMap - 0.5f);// d.r *0.8f - 0.4f;// (d.r - 0.5f); // bring sampled depth within range of (-0.4f,0.4) to account for depth origin being at the center of the block //(-0.5f,0.5f)
	return adjustedDepth * DepthResolution + inputDepth;//(d.r)*DepthResolution + 
}
float4 EntityShaderWithDepth(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0 //, out float depth : DEPTH0
{
	// TODO: add your pixel shader code here.
	float4 color, depth;
	color = tex2D(s, input.TexCoord.xy);
	float4 d = tex2D(s1, input.TexCoord.xy); // sample depth map at uv coords
	clip(color.a == 0 ? -1 : 1);

	//float adjustedDepth = (d.r - 0.5f);// d.r *0.8f - 0.4f;// (d.r - 0.5f); // bring sampled depth within range of (-0.4f,0.4) to account for depth origin being at the center of the block //(-0.5f,0.5f)
	//float depthToWrite = adjustedDepth * DepthResolution + input.Depth.x;//(d.r)*DepthResolution + 
	float depthToWrite = GetDepth(d.r, input.Depth.x);// -2 / DepthResolution;
	depthBuffer = depthToWrite;// input.Depth.x;// -DepthResolution / 2;
//	return float4(depthBuffer, depthBuffer, depthBuffer, 1);
	return color * AmbientLight;
}
float4 EntityShader(VertexShaderOutput input, out float depth : DEPTH0) : COLOR0 //, out float depth : DEPTH0
{
	// TODO: add your pixel shader code here.
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	//clip(color.a == 0 ? -1 : 1);
	//clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);
	//depth = 0;// input.Depth.r;
	//depth = input.Depth.x;
	//return input.Depth.x;
	depth = input.Depth.x;// -DepthResolution / 2;
	//return float4(depth, depth, depth, 1);
	return color * AmbientLight;
	

	float a = input.Color.a;
	return (1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a);
}

float4 FinalOutsideBorders(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 light = tex2D(s1, input.TexCoord.xy);
	float depth = tex2D(s2, input.TexCoord.xy);
	depthBuffer = depth;
	//clip(color.a == 0 ? -1 : 1);

	//float borderThickness = 0.003;
	float borderThreshold = 0.02;
	float dtop = tex2D(s2, input.TexCoord.xy + float2(0, -BorderThickness));
	float dbottom = tex2D(s2, input.TexCoord.xy + float2(0, BorderThickness));
	float dleft = tex2D(s2, input.TexCoord.xy + float2(-BorderThickness, 0));
	float dright = tex2D(s2, input.TexCoord.xy + float2(BorderThickness, 0));
	//float4 border = float4(1, 1, 1, 1);
	float4 border;
	float diff;

	diff = depth.r - dtop.r;
	diff = max(diff, depth.r - dbottom.r);
	diff = max(diff, depth.r - dleft.r);
	diff = max(diff, depth.r - dright.r);

	if (diff > borderThreshold)
	{
		float distance = diff - borderThreshold;
		float b = 1 - distance / DepthResolution;
		return ((1 - b) * float4(0, 0, 0, 1) + (b)* color*light);
		//return float4(b, b, b, 1) * float4(color.rgb, 1);// *light * AmbientLight;
	}
		//return float4(0, 0, 0, 1);
		return color.a * color * light * AmbientLight; //float4(depth, depth, depth, 1);//  border * 

		if (
			depth.r > dtop.r + BorderThickness ||
			depth.r > dbottom.r + BorderThickness ||
			depth.r > dleft.r + BorderThickness ||
			depth.r > dright.r + BorderThickness)
			/*depth.r < dtop.r - borderThreshold ||
			depth.r < dbottom.r - borderThreshold ||
			depth.r < dleft.r - borderThreshold ||
			depth.r < dright.r - borderThreshold)*/
		{
		depthBuffer = depth - borderThreshold;
		return float4(0, 0, 0, 1);

		depthBuffer = depth - borderThreshold;
		//border = float4(0, 0, 0, 1)*  (dright.r - borderThreshold);
		float factor = dright.r - borderThreshold;
		border = (1-factor)*color + factor*float4(0, 0, 0, 1);
		return color.a *border;
		}

	//return float4(depth, depth, depth, 1);// 
	return color.a * color * light * AmbientLight; //float4(depth, depth, depth, 1);//  border * 
	
}

float4 FinalInsideBordersFunction(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 light = tex2D(s1, input.TexCoord.xy);
	float depth = tex2D(s2, input.TexCoord.xy);
	depthBuffer = depth;
	//clip(light == 0 ? -1 : 1);
	float dtop = tex2D(s2, input.TexCoord.xy + float2(0, -BorderResolution.y));
	float dbottom = tex2D(s2, input.TexCoord.xy + float2(0, BorderResolution.y));
	float dleft = tex2D(s2, input.TexCoord.xy + float2(-BorderResolution.x, 0));
	float dright = tex2D(s2, input.TexCoord.xy + float2(BorderResolution.x, 0));
	float diff;

	diff = dtop.r - depth.r;
	diff = max(diff, dbottom.r - depth.r);
	diff = max(diff, dleft.r - depth.r);
	diff = max(diff, dright.r - depth.r);

	if (diff > OutlineThreshold)// OutlineThreshold)
	{
		float b = 0.5f;
		return ((1 - b) * float4(0, 0, 0, 1) + (b)* color*light);
		//return float4(b, b, b, 1) * float4(color.rgb, 1);// *light * AmbientLight;
	}

	//float dist = 1 - distance(float2(0,0), input.ScreenPosition);

	return color.a * color * light * AmbientLight; //float4(depth, depth, depth, 1);//  border * 
}
//float4 FinalFunction(VertexShaderOutput input) : COLOR0
//{
//	float4 color = tex2D(s, input.TexCoord.xy);
//	float4 light = tex2D(s1, input.TexCoord.xy);
//	//depth = input.Depth.x;
//	return color * light * AmbientLight;
//}


MyPixelOutput MyPixelShader(MyVertexOutput input)
{
	MyPixelOutput output;
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)
	// get local UV
	float u = ((input.TexCoord.x * AtlasWidth) % BlockWidth) / BlockWidth;
	float v = ((input.TexCoord.y * AtlasHeight) % BlockHeight) / BlockHeight;
	float2 uv = float2(u, v);

	// find current block face
	float4 face = tex2D(s2, uv);
	float l = 0;

	if (v < TileVertEnsureDraw || face.r > 0)
		l = input.Light.b;
	else if (face.g > 0)
		l = input.Light.g;
	else if (face.b > 0)
		l = input.Light.r;
	//clip(l == 0 ? -1 : 1);
	// blend sampled color with source color
	float a = input.Color.a;

	// sample depth map from blockdepthmap at uv coords
	float4 d = tex2D(s3, uv);
		//float adjustedD = (d.r - 0.5f) * DepthResolution;// d.r *0.8f - 0.4f;// (d.r - 0.5f); // bring sampled depth within range of (-0.4f,0.4) to account for depth origin being at the center of the block //(-0.5f,0.5f)
		//float depthToWrite = input.Depth.x +adjustedD;//(d.r)*DepthResolution + 
		float depthToWrite = GetDepth(d.r, input.Depth.x);
	//output.Color = depthToWrite + float4(0, 0, 0, 1); // to draw opaque depth
	//float dist = 1 - distance(float2(0.5, 0.5), input.ScreenPosition);
	output.Color = d.a*((1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a));
	//output.Color *= dist;
	//output.Color = color * input.Color;
	// blend sampled depth with input depth
	// add interpolated depth resolution (by factor from sampled depth) to input depth
	output.Depth = depthToWrite + float4(0, 0, 0, 1); // to draw opaque depth //d.r*DepthResolution + 
	output.DepthBuffer = depthToWrite; //d.r*DepthResolution + input.Depth.x; //
	output.Light = float4(l, l, l, 1);

	

	return output;
}

float4 EntityShadowShader(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)
	// get local UV
	//float u = ((input.TexCoord.x * AtlasWidth) % 32) / 32;
	float v = ((input.TexCoord.y * AtlasHeight) % 16) / 16;
	//float2 uv = float2(u, v);

		float adjustedD = (1 - v - 0.5f);	// bring sampled depth within range of (-0.4f,0.4) to account for depth origin being at the center of the block //(-0.5f,0.5f)

	depthBuffer = adjustedD * DepthResolution + input.Depth.x - DepthResolution/6; // to bring it a bit out from the ground plane
	return color;// float4(depthBuffer, 0, 0, 1);
}

float4 SolidColor(MyVertexOutput input) : COLOR0
{
	clip(tex2D(s, input.TexCoord.xy).a == 0 ? -1 : 1);
	return input.Color;
}
float4 SpriteDepthTextureShader(MyVertexOutput input) : COLOR0
{
	clip(tex2D(s, input.TexCoord.xy).a == 0 ? -1 : 1);
	return tex2D(s1, input.TexCoord. xy);
}
float4 DefaultShader(MyVertexOutput input) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	return color;
}
float4 BlockHighlightShader(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	//float u = ((input.TexCoord.x * AtlasWidth) % BlockWidth) / BlockWidth;
	//float v = ((input.TexCoord.y * AtlasHeight) % BlockHeight) / BlockHeight;
	//float2 uv = float2(u, v);
		float4 d = tex2D(s1, input.TexCoord.xy);
	//float adjustedD = d.r - 0.5f;// (1 - v - 0.5f);
	float adjustedD = d.r*0.8 - 0.4f;// (1 - v - 0.5f);
		float dd = adjustedD * DepthResolution + input.Depth.x -DepthResolution / 12; // to bring it a bit out from the ground plane
	depthBuffer = dd;
	//return d;// float4(d, d, d, 1);

	return color * input.Color;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 SolidBorder();
    }
}

technique Technique2
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 ColorKey();
	}
}
technique Solid
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 SolidColor();
	}
}
technique Normal
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}

technique Entities
{
	pass Pass1
	{
		// TODO: set renderstates here.

		//VertexShader = compile vs_2_0 VertexShaderFunction();
		VertexShader = compile vs_2_0 MyVertexShader();
		//PixelShader = compile ps_2_0 EntityShader();
		PixelShader = compile ps_2_0 EntityShaderWithDepth();
	}
}

technique Light
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 LightFunction();
	}
}

technique Final
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 FinalOutsideBorders();
	}
}
technique FinalInsideBorders
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 FinalInsideBordersFunction();
	}
}
technique Combined{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 MyPixelShader();
	}
};

technique EntityShadows{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 EntityShadowShader();
	}
};

technique BlockHighlight
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 BlockHighlightShader();
	}
};

technique SpriteDepthTexture
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 SpriteDepthTextureShader();
	}
};
