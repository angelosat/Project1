sampler s;
sampler s1 : register(s1);
sampler s2 : register(s2);
sampler s3 : register(s3);
sampler s4 : register(s4);
float4x4 World;
float4x4 View;
float4x4 Projection;
float2 Viewport;
float2 ViewportW;

float BlockWidth;
float BlockHeight;
float AtlasWidth;
float AtlasHeight;

float NearDepth;
float FarDepth;

float DepthResolution;
float BorderThicknessPx = 1;
float BorderThickness = 0.0005;
float2 BorderResolution;
float OutlineThreshold = 0.02; // if depth difference between target and surrounding pixels is larger than this, shade the pixel as part of the sprite's outline
float Zoom = 1;
float2 OutlineTolerance;
float2 GetBorderThickness()
{
	return float2(BorderThicknessPx / Viewport.x, BorderThickness / Viewport.y) * Zoom;
}
bool PlayerOcclusion;
float3 PlayerGlobal;
float4 PlayerBoundingBox;
float PlayerDepth;
float PlayerRotXY;
float2 PlayerCenterOffset;
float RotCos;
float RotSin;
bool HideWalls;
float OcclusionRadius;
float MaxDrawLevel;
bool FogEnabled;
float FogLevel;
float4 AmbientLight;
float4 FogColor;
float FogZ;
int FogDistance;
float2 FogOffset;
float2 FogTextureSize;
float2 WaterTextureSize;
float2 WaterOffset;
float2 WaterOffset2;

float TileVertEnsureDraw;//for irregular blocks that extend upwards outside the cube and into the transparent area
// TODO: add effect parameters here.

float4 GetFogColor(float z)
{
	if (!FogEnabled)
		return float4(0, 0, 0, 0);
	if (FogZ > 1)
		if (z < FogZ + FogLevel)
		{
			float d = abs(z - FogZ + 1 - FogLevel); // the problem with that glitchy fog layer when jumping seems to be here, but i floor the player z now as a workaround anyway
			d = clamp(d, 0, FogDistance) / FogDistance;
			float4 finalFogColor = float4(d, d, d, d);
			return finalFogColor;
		}
	return float4(0, 0, 0, 0);
}
float4 GetFogColorFromPlayer(float z)
{
	if (!FogEnabled)
		return float4(0, 0, 0, 0);
	float playerz = floor(PlayerGlobal.z);
	if (playerz > 1)
		if (z < playerz + FogLevel)
		{
			float d = abs(z - playerz + 1 - FogLevel); // the problem with that glitchy fog layer when jumping seems to be here, but i floor the player z now as a workaround anyway
			float fogDistance = 8;
			d = clamp(d, 0, fogDistance) / fogDistance;
			float4 finalFogColor = float4(d, d, d, d);
			return finalFogColor;
		}
	return float4(0, 0, 0, 0);
}

struct MyVertexInput
{
	float4 Position : SV_Position0;
	float4 TexCoord : TEXCOORD0;
	float4 Fog : COLOR0;
	float4 Color : COLOR1;
	float4 Light : COLOR2;
	float4 BlockLight : COLOR3;// COLOR2;
	float4 Water : COLOR4;
	float4 Material : COLOR5;
	float3 BlockCoords : TEXCOORD3;
	//float2 BlockRotXY : TEXCOORD4;
};
struct MyVertexOutput
{
	float4 TexCoord : TEXCOORD0;
	float4 Position : SV_Position0; // POSITION0;
	float4 Fog : TEXCOORD4;
	float4 Color : COLOR0;
	float4 Light : COLOR1;
	float4 BlockLight : TEXCOORD2;// COLOR2;
	float Depth : TEXCOORD1;
	float4 ScreenPosition : TEXCOORD3;
	float4 Water : TEXCOORD5;
	float4 Material : TEXCOORD6;
	float3 BlockCoords : TEXCOORD7;
	//float2 BlockRotXY : TEXCOORD8;

	// TODO: add vertex shader outputs such as colors and texture
	// coordinates here. These values will automatically be interpolated
	// over the triangle, and provided as input to your pixel shader.
};
struct VertexShaderInput
{
	float4 Position : SV_Position0;
	float4 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};
struct VertexShaderOutput
{
	float4 TexCoord : TEXCOORD0;
	float4 Position : SV_Position0; // POSITION0;
	float4 Color : COLOR0;
	float Depth : TEXCOORD1;
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};
struct MyPixelOutput
{
	float4 Color : COLOR0;
	float4 Light : COLOR1;
	float4 Depth : COLOR2;
	float4 Fog : COLOR3;
	float DepthBuffer : DEPTH0;
};
MyVertexOutput MyChunkVertexShader(MyVertexInput input)
{
	MyVertexOutput output;
	//output.ScreenPosition = float4((input.Position.xy - 0.5f) / Viewport, 0, 0); //-0.5f created problems in monogame
	output.ScreenPosition = float4(input.Position.xy / Viewport, 0, 0);

	bool visible = input.BlockCoords.z <= MaxDrawLevel;

	if (visible)
	{
		float4 worldPosition = mul(input.Position, World);
		float4 viewPosition = mul(worldPosition, View);
		output.Position = mul(viewPosition, Projection);
	}
	else
		output.Position = float4(0, 0, 0, 0);

	output.TexCoord = input.TexCoord;
	
	output.Fog = (FogEnabled && visible) ? GetFogColor(input.BlockCoords.z) : float4(0, 0, 0, 0);
	if (output.Fog.r == 1)
		output.Position = float4(0, 0, 0, 0); // no difference in performance???

	output.Color = input.Color;
	output.Light = input.Light;
	output.BlockLight = input.BlockLight;
	output.Depth.x = 1 - output.Position.z / output.Position.w; // w is 1 anyway
	output.BlockCoords = input.BlockCoords;
	output.Water = input.Water;
	output.Material = input.Material;
	output.ScreenPosition = output.Position;
	return output;
}
MyVertexOutput MyVertexShader(MyVertexInput input)
{
	MyVertexOutput output;
	//output.ScreenPosition = float4((input.Position.xy - 0.5f) / Viewport, 0, 0); //-0.5f created problems in monogame
	output.ScreenPosition = float4(input.Position.xy / Viewport, 0, 0);

	// these 4 lines is basically a projection matrix multiplication
	//input.Position.xy -= 0.5f;//this created problems in monogame
	input.Position.xy /= Viewport;
	input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);
	
	output.Position = input.Position;
	output.Position.z = (output.Position.z - FarDepth) / (NearDepth - FarDepth);
	output.TexCoord = input.TexCoord;
	output.Fog = input.Fog;
	output.Color = input.Color;
	output.Light = input.Light;
	output.BlockLight = input.BlockLight;
	output.Depth.x = 1 - output.Position.z / output.Position.w;
	output.BlockCoords = input.BlockCoords;
	output.Water = input.Water;
	output.Material = input.Material;
	return output;
}
MyVertexOutput EntityMouseoverVertexShaderFunction(MyVertexInput input)
{
	MyVertexOutput output;
	//output.ScreenPosition = float4((input.Position.xy - 0.5f) / Viewport, 0, 0);//-0.5f created problems in monogame
	output.ScreenPosition = float4(input.Position.xy / Viewport, 0, 0);
	//input.Position.xy -= 0.5f;//this created problems in monogame
	input.Position.xy /= Viewport;
	input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);

	output.Position = input.Position;
	output.TexCoord = input.TexCoord;
	output.Fog = input.Fog;
	output.Color = input.Color;
	output.Light = input.Light;
	output.BlockLight = input.BlockLight;
	output.Depth.x = output.Position.z / output.Position.w;
	output.BlockCoords = input.BlockCoords;
	output.Water = input.Water;
	output.Material = input.Material;
	return output;
}
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	//input.Position.xy -= 0.5f; //this created problems in monogame
	input.Position.xy /= Viewport;
	input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);
	output.Position = input.Position;// mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	output.Depth.x = input.Position.z / input.Position.w;
    return output;
}


float4 PixelShaderFunction(VertexShaderOutput input, out float depth : DEPTH0) : COLOR0
{
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	depth = input.Depth.r;
	float a = input.Color.a;
	return (1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a);
}

float4 BlurMore(VertexShaderOutput input) : COLOR0
{
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	color += tex2D(s, input.TexCoord.xy + float2(0.02, 0));
	color += tex2D(s, input.TexCoord.xy - float2(0.02, 0));
	color += tex2D(s, input.TexCoord.xy + float2(0, 0.02));
	color += tex2D(s, input.TexCoord.xy - float2(0, 0.02));
	color /= 5;
	return float4(input.Color.r, input.Color.g, input.Color.b, color.a);
}

float4 SolidBorder(VertexShaderOutput input) : COLOR0
{
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	return float4(1, 0, 0, color.a);
}

float4 ColorKey(VertexShaderOutput input) : COLOR0
{
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
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
	return float4(l, l, l, 1);
}
float GetDepth(float depthMap, float inputDepth)
{
	float adjustedDepth = (depthMap - 0.5f);
	return adjustedDepth * DepthResolution + inputDepth;
}

float4 EntityShaderWithDepth(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color, depth;
	color = tex2D(s, input.TexCoord.xy);
	float4 d = tex2D(s1, input.TexCoord.xy); // sample depth map at uv coords
	clip(color.a == 0 ? -1 : 1);

	float depthToWrite = GetDepth(d.r, input.Depth.x);
	depthBuffer = depthToWrite;
	return color * AmbientLight * input.Color;
}
float4 EntityShaderWithDepthColored(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0 //, out float depth : DEPTH0
{
	float4 grayscalecolor, depth;

	// we're either sampling grayscale textures of sprites, and tint them accordingly via the input color
	// , or samping colored textures and tint them via the input color which should generally be white
	grayscalecolor = tex2D(s, input.TexCoord.xy); 
	float4 d = tex2D(s1, input.TexCoord.xy); // sample depth map at uv coords
		clip(grayscalecolor.a == 0 ? -1 : 1);

	float depthToWrite = GetDepth(d.r, input.Depth.x);
	depthBuffer = depthToWrite;

	float4 outcol = grayscalecolor * input.Color;

	// add lighting
	float4 maxlight = float4(max(input.Light.r, input.BlockLight.r), max(input.Light.g, input.BlockLight.g), max(input.Light.b, input.BlockLight.b), 1);
	outcol *= maxlight;

	// TODO: draw entities before compositing final scene so i can sample the fog texture at the correct screen coordinates
	float fogValue = input.Fog.a;
	float4 fog = FogColor * fogValue;
	outcol += fog;

	return outcol;
}
float4 ParticleShader(MyVertexOutput input, out float depthBuffer : DEPTH0, out float4 fog : COLOR1, out float4 depthmap : COLOR2) : COLOR0 //, out float depth : DEPTH0
{
	float4 grayscalecolor, depth;

	// we're either sampling grayscale textures of sprites, and tint them accordingly via the input color
	// , or samping colored textures and tint them via the input color which should generally be white
	grayscalecolor = tex2D(s, input.TexCoord.xy);
	float4 d = tex2D(s1, input.TexCoord.xy); // sample entity's depth map at uv coords
		clip(grayscalecolor.a == 0 ? -1 : 1);

	float depthToWrite = input.Depth.x;
	depthBuffer = depthToWrite;
	depthmap = float4(depthToWrite, depthToWrite, depthToWrite, 1);

	// TODO: pass shininess to shader
	float shininess = input.Color.a;
	float shine = grayscalecolor.r * shininess;
	shine = pow(shine, 3);
	float4 outcol = grayscalecolor * shine + (grayscalecolor * input.Color) * (1 - shine); 
	outcol.a = 1; // make output alpha 1 again because i'm passing shininess factor through the input color's alpha channel
	outcol = input.Color * grayscalecolor;

	// add lighting
	float4 maxlight = float4(max(input.Light.r, input.BlockLight.r), max(input.Light.g, input.BlockLight.g), max(input.Light.b, input.BlockLight.b), 1);
		outcol *= maxlight;

	// TODO: draw entities before compositing final scene so i can sample the fog texture at the correct screen coordinates
	float fogValue = input.Fog.a;
	fog = float4(fogValue, 0, 0, 1);
	return outcol;
}

float4 mixWithMaterial(float4 color, float4 material, float shininess)
{
	float grayscale = (color.r + color.g + color.b) / 3.0f;
	float shine = grayscale * shininess;
	shine = pow(shine, 3);
	//float4 outcol = shininess; // COOL EFFECT FOR GHOSTS (because of the alpha)
	float4 outcol = color * shine + (color * material) * (1 - shine);
		outcol.a = 1;
	return outcol;
}
float4 mixWithMaterial(float4 color, float4 material)
{
	float shininess = material.a;
	float grayscale = (color.r + color.g + color.b) / 3.0f;
	float pixelshine = grayscale * shininess;
	pixelshine = pow(pixelshine, 3);
	float blend = 1;
	float4 targetcolor = color *(1 - blend) + blend*color * float4(material.rgb, 1);
	float4 outcol = color * pixelshine + (targetcolor)* (1 - pixelshine);
	outcol.a = 1;
	return outcol;
}
float4 EntitiesFogShader(MyVertexOutput input, out float depthBuffer : DEPTH0, out float4 fog : COLOR1, out float4 depthmap : COLOR2) : COLOR0 //, out float depth : DEPTH0
{
	float4 grayscalecolor, depth;

	// we're either sampling grayscale textures of sprites, and tint them accordingly via the input color
	// , or samping colored textures and tint them via the input color which should generally be white
	grayscalecolor = tex2D(s, input.TexCoord.xy);
	float4 d = tex2D(s1, input.TexCoord.xy); // sample entity's depth map at uv coords
		clip(grayscalecolor.a == 0 ? -1 : 1);

	float depthToWrite = GetDepth(d.r, input.Depth.x);
	depthBuffer = depthToWrite;
	depthmap = float4(depthToWrite, depthToWrite, depthToWrite, 1);
	
	float4 outcol = mixWithMaterial(grayscalecolor, input.Material);
	outcol *= input.Color;

	// add lighting
	float4 maxlight = float4(max(input.Light.r, input.BlockLight.r), max(input.Light.g, input.BlockLight.g), max(input.Light.b, input.BlockLight.b), 1);
	outcol *= maxlight;

	// TODO: draw entities before compositing final scene so i can sample the fog texture at the correct screen coordinates
	float fogValue = input.Fog.a;
	fog = float4(fogValue, 0, 0, 1);
	return outcol;
}
float4 EntitiesUIShader(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	depthBuffer = 0;
	return float4(1, 0, 0, 1);
}
float4 EntityShader(VertexShaderOutput input, out float depth : DEPTH0) : COLOR0
{
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	depth = input.Depth.x;
	return color * AmbientLight;
}

float4 FinalOutsideBorders(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 light = tex2D(s1, input.TexCoord.xy);
	float depth = tex2D(s2, input.TexCoord.xy);
	depthBuffer = depth;
	float borderThreshold = 0.02;
	float dtop = tex2D(s2, input.TexCoord.xy + float2(0, -BorderThickness));
	float dbottom = tex2D(s2, input.TexCoord.xy + float2(0, BorderThickness));
	float dleft = tex2D(s2, input.TexCoord.xy + float2(-BorderThickness, 0));
	float dright = tex2D(s2, input.TexCoord.xy + float2(BorderThickness, 0));
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
		return ((1 - b) * float4(0, 0, 0, 1) + (b) * color * light);
	}
	return color.a * color * light * AmbientLight;

	if (depth.r > dtop.r + BorderThickness ||
		depth.r > dbottom.r + BorderThickness ||
		depth.r > dleft.r + BorderThickness ||
		depth.r > dright.r + BorderThickness)
	{
		depthBuffer = depth - borderThreshold;
		return float4(0, 0, 0, 1);

		depthBuffer = depth - borderThreshold;
		float factor = dright.r - borderThreshold;
		border = (1 - factor) * color + factor * float4(0, 0, 0, 1);
		return color.a * border;
	}

	return color.a * color * light * AmbientLight;
}

float4 FogWaterShader(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 fog = tex2D(s1, input.TexCoord.xy);

	//sample depth from depth texture and write it to render target
	float4 depth = tex2D(s3, input.TexCoord.xy);
	float d = depth.r;
	depthBuffer = d;

	//water here
	float2 wateruv = Viewport / (WaterTextureSize * Zoom);
	wateruv *= (0.5f - input.TexCoord.xy);
	float4 water = tex2D(s4, float2(wateruv.x, wateruv.y) + WaterOffset);
	float wateramount = fog.b * 0.5f;
	color = color * (1 - wateramount) + water * wateramount;
	float2 foguv = Viewport / (FogTextureSize * Zoom);

	foguv *= (0.5f - input.TexCoord.xy);
	float4 fogTex = tex2D(s2, float2(foguv.x, foguv.y) + FogOffset);

	float value = fog.r;
	value = max(fog.r, 1 - color.a);
	float4 fogFinal = fogTex * (1 - value); // +FogColor * fog.r; // comment to make fog color white
	float4 afterFog = color * (1 - value) + fogFinal * value;

	// effect for reducing color depth
	/*	float r = round(afterFog.r * 8) / 8;
		float g = round(afterFog.g * 8) / 8;
		float b = round(afterFog.b * 4) / 4;
		afterFog = float4(r, g, b, afterFog.a);*/

	return afterFog;
}

float4 MapDump(VertexShaderOutput input, out float depthBuffer : DEPTH0, out float4 fogoutput : COLOR1) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 fog = tex2D(s1, input.TexCoord.xy);
	//sample depth from depth texture and write it to render target
	float4 depth = tex2D(s3, input.TexCoord.xy);
	float d = depth.r;
	depthBuffer = d;
	fogoutput = float4(fog.r, 0, 0, 1);
	return color;
}
float4 ApplyFogShader(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 fog = tex2D(s1, input.TexCoord.xy);
	
	//sample depth from depth texture and write it to render target
	float4 depth = tex2D(s3, input.TexCoord.xy);
	float d = depth.r;
	depthBuffer = d;

	float2 foguv = Viewport / (FogTextureSize * Zoom);

	foguv *= (0.5f - input.TexCoord.xy);
	float4 fogTex = tex2D(s2, float2(foguv.x, foguv.y) + FogOffset);

	float value = fog.r;
	value = max(fog.r, 1 - color.a);
	float4 fogFinal = fogTex * (1 - value); // +FogColor * fog.r; // comment to make fog color white
	float4 afterFog = color * (1 - value) + fogFinal * value;

	//	 //effect for reducing color depth
	//	 float r = round(afterFog.r * 16) / 16;// 8) / 8;
	//float g = round(afterFog.g * 16) / 16;//8) / 8;
	//float b = round(afterFog.b * 16) / 16;//8) / 8; //4) / 4;
	//	afterFog = float4(r, g, b, afterFog.a);

	return afterFog;
}
float4 FogShader(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 fog = tex2D(s1, input.TexCoord.xy);

	//sample depth from depth texture and write it to render target
	float4 depth = tex2D(s3, input.TexCoord.xy);
	float d = depth.r;
	depthBuffer = d;
	float2 foguv = Viewport / (FogTextureSize * Zoom);
	foguv *= (0.5f - input.TexCoord.xy);
	float4 fogTex = tex2D(s2, float2(foguv.x, foguv.y) + FogOffset);

	float value = fog.r;
	value = max(fog.r, 1 - color.a);
	float4 fogFinal = fogTex * (1 - value); // +FogColor * fog.r; // comment to make fog color white
	float4 afterFog = color * (1 - value) + fogFinal * value;

		// effect for reducing color depth
		//	float r = round(afterFog.r * 8) / 8;
		//float g = round(afterFog.g * 8) / 8;
		//float b = round(afterFog.b * 4) / 4;
		//afterFog = float4(r, g, b, afterFog.a);

	return afterFog;
}

float4 FinalWaterShader(VertexShaderOutput input, out float depthBuffer : DEPTH0, out float4 fogoutput : COLOR1) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 fog = tex2D(s1, input.TexCoord.xy);

	//sample depth from depth texture and write it to render target
	float4 depth = tex2D(s3, input.TexCoord.xy);
	float d = depth.r;
	depthBuffer = d;
	clip(fog.r == 1 ? -1 : 1);
	fogoutput = float4(fog.r, 0, 0, 1);
	return color;// *0.5f; // i set the alpha in the compositewater technique because its value depends on block face

	float fogvalue = fog.r;
	fogvalue = max(fog.r, 1 - color.a);
	color *= (1-fogvalue);
	color *= 0.5f; // uncomment to make water transparent

	return color;
}


float4 FinalInsideBordersFunction(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 light = tex2D(s1, input.TexCoord.xy);
	float depth = tex2D(s2, input.TexCoord.xy);
	float4 fog = tex2D(s3, input.TexCoord.xy);
	float4 fogTex = tex2D(s4, float2(input.TexCoord.x, input.TexCoord.y) + FogOffset);

	depthBuffer = depth;
	float dtop = tex2D(s2, input.TexCoord.xy + float2(0, -BorderResolution.y));
	float dbottom = tex2D(s2, input.TexCoord.xy + float2(0, BorderResolution.y));
	float dleft = tex2D(s2, input.TexCoord.xy + float2(-BorderResolution.x, 0));
	float dright = tex2D(s2, input.TexCoord.xy + float2(BorderResolution.x, 0));
	float diff;

	diff = dtop.r - depth.r;
	diff = max(diff, dbottom.r - depth.r);
	diff = max(diff, dleft.r - depth.r);
	diff = max(diff, dright.r - depth.r);

	if (diff > OutlineThreshold)
	{
		float b = 0.5f;
		float4 outline = ((1 - b) * float4(0, 0, 0, 1) + (b) * color * light) * AmbientLight;
		return outline * (1 - fog.r) + FogColor * fog.r;
		return ((1 - b) * float4(0, 0, 0, 1) + (b) * color * light) * AmbientLight;
	}

	float4 beforeFog = color.a * color * light;
	// DONT ADD FOG HERE, ADD LATER AFTER DRAWING ENTITIES
	float4 fogFinal = fogTex * (1 - fog.r); // +FogColor * fog.r; // comment to make fog color white
	float4 afterFog = beforeFog * (1 - fog.r) + fogFinal * fog.r;
	return afterFog;

	return color.a * color * light;
}
float4 FinalInsideBordersFunctionNoFog(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 light = tex2D(s1, input.TexCoord.xy);
	float depth = tex2D(s2, input.TexCoord.xy);
	depthBuffer = depth;

	float dtop = tex2D(s2, input.TexCoord.xy + float2(0, -BorderResolution.y));
	float dbottom = tex2D(s2, input.TexCoord.xy + float2(0, BorderResolution.y));
	float dleft = tex2D(s2, input.TexCoord.xy + float2(-BorderResolution.x, 0));
	float dright = tex2D(s2, input.TexCoord.xy + float2(BorderResolution.x, 0));
	float diff;

	diff = dtop.r - depth.r;
	diff = max(diff, dbottom.r - depth.r);
	diff = max(diff, dleft.r - depth.r);
	diff = max(diff, dright.r - depth.r);

	if (diff > OutlineThreshold)
	{
		float b = 0; // 0.5f; //outline transparency
		float4 outline = ((1 - b) * float4(0, 0, 0, 1) + (b) * color * light) * AmbientLight;
		return outline;
	}

	float4 beforeFog = color.a * color * light;

	//	//effect for reducing color depth
	//	float r = round(beforeFog.r * 8) / 8;
	//float g = round(beforeFog.g * 8) / 8;
	//float b = round(beforeFog.b * 8) / 8; //4) / 4;
	//beforeFog = float4(r, g, b, beforeFog.a);

	return beforeFog;
}

float4 CompositeWaterShader(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 light = tex2D(s1, input.TexCoord.xy);
	float depth = tex2D(s2, input.TexCoord.xy);
	depthBuffer = depth;
	// DONT DRAW WATER HERE, DRAW IT INSIDE FOG SHADER INSTEAD
	float4 watermap = tex2D(s3, input.TexCoord.xy);
	float2 wateruv = Viewport / (WaterTextureSize * Zoom); // TODO: cache this as a parameter!
	wateruv *= (0.5f - input.TexCoord.xy);
		
	float4 water = tex2D(s4, float2(wateruv.x, wateruv.y) + WaterOffset);
	water += tex2D(s4, float2(wateruv.x, wateruv.y) + WaterOffset2);

	float4 outcolor;
	if (watermap.b == 1) // if the blue component of the watermaptexture is 1, it means we are drawing water
	{
		outcolor = water;

		outcolor.a = .5f;
		if (color.g == 1)
		{
			outcolor.rgb *= .5f;
			outcolor.a = .75f;
		}
		if (color.b == 1)
		{
			outcolor.rgb *= .25f;
			outcolor.a = .9f;
		}
	}
	else //otherwise, we are drawing a regular block, so make it transparent (here? or pass the alpha value through the vertex?)
	{
		outcolor = float4(color.rgb, .5f);
	}
	float dtop = tex2D(s2, input.TexCoord.xy + float2(0, -BorderResolution.y));
	float dbottom = tex2D(s2, input.TexCoord.xy + float2(0, BorderResolution.y));
	float dleft = tex2D(s2, input.TexCoord.xy + float2(-BorderResolution.x, 0));
	float dright = tex2D(s2, input.TexCoord.xy + float2(BorderResolution.x, 0));
	float diff;

	diff = dtop.r - depth.r;
	diff = max(diff, dbottom.r - depth.r);
	diff = max(diff, dleft.r - depth.r);
	diff = max(diff, dright.r - depth.r);

	if (diff > OutlineThreshold)
	{
		float b = 0.5f;
		float4 outline = ((1 - b) * float4(0, 0, 0, 1) + (b) * outcolor * light) * AmbientLight;
		return outline;
	}

	float4 beforeFog = outcolor.a * outcolor * light;

	return beforeFog;
}

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
	float4 face = tex2D(s2, uv);
	float sunlight = 0;
	float blocklight;
	if (v < TileVertEnsureDraw || face.r > 0)
	{
		sunlight = input.Light.b;
		blocklight = input.BlockLight.b;
	}
	else if (face.g > 0)
	{
		sunlight = input.Light.g;
		blocklight = input.BlockLight.g;
	}
	else if (face.b > 0)
	{
		sunlight = input.Light.r;
		blocklight = input.BlockLight.r;
	}
	float a = input.Color.a;
	float4 d = tex2D(s3, uv);
	float depthToWrite = GetDepth(d.r, input.Depth.x);
	float fog = input.Fog.a;

	output.Fog = float4(fog, 0, input.Water.r, 1);

	if (input.Water.r > 0)
		output.Color = 0;
	else
		output.Color = d.a * ((1 - a) * color + float4(a * color.rgb * input.Color.rgb, color.a));

	output.Depth = depthToWrite + float4(0, 0, 0, 1);
	output.DepthBuffer = depthToWrite;
	float4 lightcolor = sunlight * AmbientLight + float4(blocklight, blocklight, blocklight, 1);

	output.Light = lightcolor;

	return output;
}

MyPixelOutput TransparentBlockShader(MyVertexOutput input)
{
	MyPixelOutput output;
	float4 color = tex2D(s, input.TexCoord.xy);
		clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)

	float4 face = tex2D(s2, input.TexCoord.xy);
		float sunlight = 0;
	float blocklight;
	if (face.r > 0)
	{
		sunlight = input.Light.b;
		blocklight = input.BlockLight.b;
	}
	else if (face.g > 0)
	{
		sunlight = input.Light.g;
		blocklight = input.BlockLight.g;
	}
	else if (face.b > 0)
	{
		sunlight = input.Light.r;
		blocklight = input.BlockLight.r;
	}
	float a = input.Color.a;
	float4 d = tex2D(s3, input.TexCoord.xy);
		float depthToWrite = GetDepth(d.r, input.Depth.x);
	float fog = input.Fog.a;

	output.Fog = float4(fog, 0, input.Water.r, 1);

	output.Color = mixWithMaterial(color, input.Material);
	output.Color *= input.Color;
	output.Color *= color.a;
	output.Depth = depthToWrite + float4(0, 0, 0, 1);
	output.DepthBuffer = depthToWrite;
	float4 lightcolor = sunlight * AmbientLight + float4(blocklight, blocklight, blocklight, 1);
	output.Light = lightcolor;

	return output;
}

MyPixelOutput MyChunkPixelShader(MyVertexOutput input)
{
	MyPixelOutput output;
	float4 color = tex2D(s, input.TexCoord.xy);
		clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);
	float fog = input.Fog.a;
	clip(fog == 1 ? -1 : 1); // no difference in performance???!?!?!?

	if (HideWalls && PlayerOcclusion)
		if (input.BlockCoords.z >= PlayerGlobal.z)
		{
			if (input.BlockCoords.x + input.BlockCoords.y > PlayerRotXY) //the good one
			{
				float2 positionNormalised = input.ScreenPosition.xy / ViewportW;
				 positionNormalised += float2(-PlayerCenterOffset.x,PlayerCenterOffset.y);

					float l = pow(positionNormalised.x, 2) + pow(positionNormalised.y, 2);
				if (l < OcclusionRadius)
					clip(-1);
			}
		}
	// TODO: pack flags instead of color in vertex for lit face (FASTER!)

	float4 face = tex2D(s2, input.TexCoord.xy);
		float sunlight = 0;
	float blocklight;
	
	// which is faster???
	sunlight = input.Light.b * face.r + input.Light.g * face.g + input.Light.r * face.b;
	blocklight = input.BlockLight.b * face.r + input.BlockLight.g * face.g + input.BlockLight.r* face.b;

	float a = input.Color.a;
	float4 d = tex2D(s3, input.TexCoord.xy);
		float depthToWrite = GetDepth(d.r, input.Depth.x);
	output.Fog = float4(fog, 0, input.Water.r, 1);

	output.Color = input.Color * color.a * mixWithMaterial(color, input.Material);
	output.Depth = depthToWrite + float4(0, 0, 0, 1);
	output.DepthBuffer = depthToWrite;

	float4 lightcolor = sunlight * AmbientLight + float4(blocklight, blocklight, blocklight, 1);
		output.Light = lightcolor;

	return output;
}

MyPixelOutput MyPixelShaderWithNormals(MyVertexOutput input)
{
	MyPixelOutput output;
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)

	float4 face = tex2D(s2, input.TexCoord.xy);
	float sunlight = 0;
	float blocklight;
	if (face.r > 0)
	{
		sunlight = input.Light.b;
		blocklight = input.BlockLight.b;
	}
	else if (face.g > 0)
	{
		sunlight = input.Light.g;
		blocklight = input.BlockLight.g;
	}
	else if (face.b > 0)
	{
		sunlight = input.Light.r;
		blocklight = input.BlockLight.r;
	}
	float a = input.Color.a;
	float4 d = tex2D(s3, input.TexCoord.xy);
	float fog = input.Fog.a;

	output.Fog = float4(fog, 0, input.Water.r, 1);

	output.Color = mixWithMaterial(color, input.Material);
	output.Color *= input.Color;
	output.Color *= color.a;
	float depthToWrite = GetDepth(d.r, input.Depth.x);
	output.Depth = depthToWrite + float4(0, 0, 0, 1);
	output.DepthBuffer = depthToWrite;
	float4 lightcolor = sunlight * AmbientLight + float4(blocklight, blocklight, blocklight, 1);
	output.Light = lightcolor;
	return output;
}

MyPixelOutput MyWaterShader(MyVertexOutput input)
{
	MyPixelOutput output;
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)

	float4 face = tex2D(s2, input.TexCoord.xy);
	float sunlight = 0;
	float blocklight;
	float waterfx = 0; // draw water texture later only on the top face
	if (face.r > 0)
	{
		sunlight = input.Light.b;
		blocklight = input.BlockLight.b;
		waterfx = 1; // draw water texture later only on the top face
	}
	else if (face.g > 0)
	{
		sunlight = input.Light.g;
		blocklight = input.BlockLight.g;
	}
	else if (face.b > 0)
	{
		sunlight = input.Light.r;
		blocklight = input.BlockLight.r;
	}
	float a = input.Color.a;
	float4 d = tex2D(s3, input.TexCoord.xy);
	
	float fog = input.Fog.a;
	output.Fog = float4(fog, 0, input.Water.r, 1); // draw water texture later only on the top face

	if (input.Water.r == 1)
		color.rgb = face.rgb; // i added this so i can draw individual water faces differently later in the water texture shader
	else // if we aren't drawing water, mix color with input material
		color = mixWithMaterial(color, input.Material);

	output.Color = d.a * ((1 - a) * color + float4(a * color.rgb * input.Color.rgb, color.a));
	output.Color *= input.Color.a;

	float depthToWrite = GetDepth(d.r, input.Depth.x);
	output.Depth = depthToWrite + float4(0, 0, 0, 1);
	output.DepthBuffer = depthToWrite;

	float4 lightcolor = sunlight * AmbientLight + float4(blocklight, blocklight, blocklight, 1);
	output.Light = lightcolor;

	return output;
}

float4 EntityShadowShader(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)
	// get local UV
	float v = ((input.TexCoord.y * AtlasHeight) % 16) / 16;

	float adjustedD = (1 - v - 0.5f); // bring sampled depth within range of (-0.4f,0.4) to account for depth origin being at the center of the block //(-0.5f,0.5f)

	depthBuffer = adjustedD * DepthResolution + input.Depth.x - DepthResolution / 6; // to bring it a bit out from the ground plane
	
	///NEW
	///draw shadow transparency according to the passed color's alpha
	color *= input.Color.a;
	
	return color;
}
float4 SolidColor(VertexShaderOutput input) : COLOR0 //(MyVertexOutput input) : COLOR0
{
	clip(tex2D(s, input.TexCoord.xy).a == 0 ? -1 : 1);
	return input.Color;
}
float4 SpriteDepthTextureShader(MyVertexOutput input) : COLOR0
{
	clip(tex2D(s, input.TexCoord.xy).a == 0 ? -1 : 1);
	return tex2D(s1, input.TexCoord.xy);
}
float4 DefaultShader(MyVertexOutput input) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	return color * input.Color;
}
float4 EntityMouseoverPixelShaderFunction(MyVertexOutput input, out float zBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float d = 1 - input.Depth.x;
	clip(color.a == 0 ? -1 : 1);
	zBuffer = d;
	float4 mix = mixWithMaterial(color, input.Material);
	return mix * input.Color;
}
float4 DefaultShaderWithDepth(VertexShaderOutput input, out float zBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float d = 1 - input.Depth.x;
	clip(color.a == 0 ? -1 : 1);
	zBuffer = d;
	return color * input.Color;
}
float4 BlockHighlightShader(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	float4 d = tex2D(s1, input.TexCoord.xy);
	//float adjustedD = d.r * 0.9 - 0.45f;
	//float dd = adjustedD * DepthResolution + input.Depth.x - DepthResolution / 10; // GetDepth(d.r, input.Depth.x) - DepthResolution / 12;
	float dd = GetDepth(d.r, input.Depth.x); // replaced above two lines with this and fixed the small depth glitch that block highlights had
	depthBuffer = dd;
	float a = input.Color.a;
	
	color = mixWithMaterial(color, input.Material);
	color *= input.Color;
	return color;
}
//float4 BlockHighlightShaderLastWorking(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
//{
//	float4 color = tex2D(s, input.TexCoord.xy);
//	clip(color.a == 0 ? -1 : 1);
//	float4 d = tex2D(s1, input.TexCoord.xy);
//	float adjustedD = d.r*0.9 - 0.45f;
//	float dd = adjustedD * DepthResolution + input.Depth.x - DepthResolution / 10;// GetDepth(d.r, input.Depth.x) - DepthResolution / 12;
//	depthBuffer = dd;// dd;
//	return color * input.Color;
//}
//float4 BlockHighlightShader2(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
//{
//	float4 color = tex2D(s, input.TexCoord.xy);
//	clip(color.a == 0 ? -1 : 1);
//	float4 d = tex2D(s1, input.TexCoord.xy);
//	float adjustedD = d.r*0.8 - 0.4f;// (1 - v - 0.5f);
//	float dd = adjustedD * DepthResolution + input.Depth.x -DepthResolution / 12; // to bring it a bit out from the ground plane
//	depthBuffer = dd;
//	return color * input.Color;
//}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 SolidBorder();
    }
}

technique Technique2
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 ColorKey();
	}
}
technique Solid
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 SolidColor();
	}
}
technique Normal
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}

technique Entities
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 EntityShaderWithDepthColored();
	}
}

technique Light
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 LightFunction();
	}
}

technique Final
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 FinalOutsideBorders();
	}
}

technique CompositeWater
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 CompositeWaterShader();
	}
};

technique FinalInsideBorders
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 FinalInsideBordersFunctionNoFog();

	}
}
technique Chunks{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyChunkVertexShader();
		PixelShader = compile ps_4_0 MyChunkPixelShader();
	}
};
technique Combined{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MyPixelShaderWithNormals();
	}
};
technique CombinedWater{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyChunkVertexShader();
		PixelShader = compile ps_4_0 MyWaterShader();
	}
};

technique EntityShadows{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 EntityShadowShader();
	}
};

technique BlockHighlight
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 BlockHighlightShader();
	}
};

technique SpriteDepthTexture
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 SpriteDepthTextureShader();
	}
};

technique Default
{
	pass Pass1
	{
		ZEnable = true;
		ZWriteEnable = true;
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 DefaultShaderWithDepth();
	}
};

technique EntityMouseover
{
	pass Pass1
	{
		ZEnable = true;
		ZWriteEnable = true;
		VertexShader = compile vs_4_0 EntityMouseoverVertexShaderFunction();
		PixelShader = compile ps_4_0 EntityMouseoverPixelShaderFunction();
	}
};

technique RenderMapWithoutFog
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MapDump();
	}
};
technique ApplyFog
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 ApplyFogShader();
	}
};
technique Fog
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 FogShader();
	}
};
technique Water
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 FinalWaterShader();
	}
};
technique FogWater
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 FogWaterShader();
	}
};
technique Particles
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 ParticleShader();
	}
};
technique EntitiesFog
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 EntitiesFogShader();
	}
};

technique EntitiesUI
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 EntitiesUIShader();
	}
};