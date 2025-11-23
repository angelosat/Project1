//------------------------------------------------------------------------------
// File: BasicRender.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
// Global variables
//------------------------------------------------------------------------------
float4 g_MaterialAmbientColor;      // Material's ambient color
float4 g_MaterialDiffuseColor;      // Material's diffuse color

float3 g_LightPos;					// Position of light
float3 g_LightDir;					// Direction of light (temp)
float4x4 g_mLightView;				// View matrix of light
float4x4 g_mLightProj;				// Projection matrix of light

float4 g_LightDiffuse;				// Light's diffuse color
float4 g_LightAmbient;              // Light's ambient color

texture g_MeshTexture;              // Color texture for mesh
texture g_ShadowMapTexture;			// Shadow map texture for lighting

float4x4 g_mWorld;                  // World matrix for object
float3 g_CameraPos;				    // Camera position for scene View 
float4x4 g_mCameraView;				// Camera's view matrix
float4x4 g_mCameraProj;				// Projection matrix


//------------------------------------------------------------------------------
// Texture samplers
//------------------------------------------------------------------------------
sampler MeshTextureSampler = 
sampler_state
{
    Texture = <g_MeshTexture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};
sampler ShadowMapSampler =
sampler_state
{
	Texture = <g_ShadowMapTexture>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

//------------------------------------------------------------------------------
// Vertex shader output structure
//------------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position   : POSITION0;   // vertex position 
    float2 TextureUV  : TEXCOORD0;  // vertex texture coords   
    float3 vNormal	  : TEXCOORD1;
    float4 vPos       : TEXCOORD2;  
};
struct PS_INPUT
{
    float2 TextureUV  : TEXCOORD0;  // vertex texture coords   
    float3 vNormal	  : TEXCOORD1;
    float4 vPos       : TEXCOORD2;  
};

//reuse//code//VS_OutputStruct//
struct VS_SHADOW_OUTPUT
{
	float4 Position : POSITION;
	float Depth : TEXCOORD0;
};
//reuse//code//VS_OutputStruct//

//------------------------------------------------------------------------------
// Utility function(s)
//------------------------------------------------------------------------------
float4x4 CreateLookAt(float3 cameraPos, float3 target, float3 up)
{
	float3 zaxis = normalize(cameraPos - target);
	float3 xaxis = normalize(cross(up, zaxis));
	float3 yaxis = cross(zaxis, xaxis);
	
	float4x4 view = { xaxis.x, yaxis.x, zaxis.x, 0,
		xaxis.y, yaxis.y, zaxis.y, 0,
		xaxis.z, yaxis.z, zaxis.z, 0,
		-dot(xaxis, cameraPos), -dot(yaxis, cameraPos),
		-dot(zaxis, cameraPos), 1
	};

	return view;
}

//reuse//code//GetPositionFromLight//
float4 GetPositionFromLight(float4 position)
{
	float4x4 WorldViewProjection =
	 mul(mul(g_mWorld, g_mLightView), g_mLightProj);
	return mul(position, WorldViewProjection);  
}
//reuse//code//GetPositionFromLight//

//------------------------------------------------------------------------------
// This shader computes rudimentary transform and lighting.
// The XNA VertexDeclaration of our models is PositionNormalTexture.
//------------------------------------------------------------------------------

//reuse//code//RenderShadowsVS//
VS_OUTPUT RenderShadowsVS(
     float3 position : POSITION,
     float3 normal : NORMAL,
     float2 vTexCoord0 : TEXCOORD0 )
{
     VS_OUTPUT Output;

     //generate the world-view-projection matrix
     float4x4 wvp = mul(mul(g_mWorld, g_mCameraView), g_mCameraProj);
     
     //transform the input position to the output
     Output.Position = mul(float4(position, 1.0), wvp);

	 //transform the normal to world space
     Output.vNormal =  mul(normal, g_mWorld);
     
     //do not transform the position needed for the
     //shadow map determination
     Output.vPos = float4(position,1.0);
     
     //pass the texture coordinate as-is
	 Output.TextureUV = vTexCoord0;
    
     //return the output structure
     return Output;
}
//reuse//code//RenderShadowsVS//

//reuse//code//RenderShadowMapVS//
VS_SHADOW_OUTPUT RenderShadowMapVS(float4 vPos: POSITION)
{
	VS_SHADOW_OUTPUT Out;
	Out.Position = GetPositionFromLight(vPos); 
	// Depth is Z/W.  This is returned by the pixel shader.
	// Subtracting from 1 gives us more precision in floating point.
	Out.Depth.x = 1-(Out.Position.z/Out.Position.w);	
	return Out;
}
//reuse//code//RenderShadowMapVS//

//------------------------------------------------------------------------------
// Pixel shader output structure
//------------------------------------------------------------------------------
struct PS_OUTPUT
{
    float4 RGBColor : COLOR0;  // Pixel color    
};


//------------------------------------------------------------------------------
// This shader outputs the pixel's color by modulating the texture's
//       color with diffuse material color
//------------------------------------------------------------------------------

//reuse//code//RenderShadowsPS1//
PS_OUTPUT RenderShadowsPS( PS_INPUT In ) 
{ 
    PS_OUTPUT Output;
    
    // Standard lighting equation
    float4 vTotalLightDiffuse = float4(0,0,0,1);
    float3 lightDir = normalize(g_LightPos-In.vPos);  // direction of light
    vTotalLightDiffuse += g_LightDiffuse * max(0,dot(In.vNormal, lightDir)); 
    vTotalLightDiffuse.a = 1.0f;
//reuse//code//RenderShadowsPS1//
//reuse//code//RenderShadowsPS2//
	// Now, consult the ShadowMap to see if we're in shadow
    float4 lightingPosition 
		= GetPositionFromLight(In.vPos);// Get our position on the shadow map
   
    // Get the shadow map depth value for this pixel   
    float2 ShadowTexC = 
		0.5 * lightingPosition.xy / lightingPosition.w + float2( 0.5, 0.5 );
    ShadowTexC.y = 1.0f - ShadowTexC.y;
//reuse//code//RenderShadowsPS2//    

//reuse//code//RenderShadowsPS3//
    float shadowdepth = tex2D(ShadowMapSampler, ShadowTexC).r;    
    
    // Check our value against the depth value
    float ourdepth = 1 - (lightingPosition.z / lightingPosition.w);
//reuse//code//RenderShadowsPS3//
    
//reuse//code//RenderShadowsPS4//    
    // Check the shadowdepth against the depth of this pixel
    // a fudge factor is added to account for floating-point error
	if (shadowdepth-0.03 > ourdepth)
	{
	    // we're in shadow, cut the light
		vTotalLightDiffuse = float4(0,0,0,1);
	};
//reuse//code//RenderShadowsPS4//

//reuse//code//RenderShadowsPS5//
    Output.RGBColor = tex2D(MeshTextureSampler, In.TextureUV) * 
		(vTotalLightDiffuse + g_LightAmbient);
        
    return Output;
    
}
//reuse//code//RenderShadowsPS5//
PS_OUTPUT DiffuseOnlyPS(VS_OUTPUT In) : COLOR
{
	 PS_OUTPUT Output;
     //calculate per-pixel diffuse
     float3 directionToLight = normalize(g_LightPos - In.vPos);
     float diffuseIntensity = saturate( dot(directionToLight, In.vNormal));
     float4 diffuse = g_LightDiffuse * diffuseIntensity;
     
     float4 color = diffuse + g_LightAmbient;
     color.a = 1.0;
     
     Output.RGBColor = tex2D(MeshTextureSampler, In.TextureUV) * color;
     
     return Output;
}
PS_OUTPUT TextureOnlyPS(float2 TextureUV  : TEXCOORD0) : COLOR
{
     PS_OUTPUT Output;
     Output.RGBColor = tex2D(MeshTextureSampler, TextureUV);
     
     return Output;
}

//reuse//code//RenderShadowMapPS//
float4 RenderShadowMapPS( VS_SHADOW_OUTPUT In ) : COLOR
{ 
	// The depth is Z divided by W. We return
	// this value entirely in a 32-bit red channel
	// using SurfaceFormat.Single.  This preserves the
	// floating-point data for finer detail.
    return float4(In.Depth.x,0,0,1);
}
//reuse//code//RenderShadowMapPS//

//------------------------------------------------------------------------------
// Renders scene to render target
//------------------------------------------------------------------------------
technique TextureRender
{
	pass P0
	{
        VertexShader = compile vs_4_0 RenderShadowsVS();
        PixelShader  = compile ps_4_0 TextureOnlyPS(); 
	}
}
technique ShadowRender
{
	pass P0
	{
        VertexShader = compile vs_4_0 RenderShadowsVS();
        PixelShader  = compile ps_4_0 RenderShadowsPS(); 
	}
}

//reuse//code//DepthTextureEffect//
technique ShadowMapRender
{
	pass P0
	{
		CullMode = NONE;
		ZEnable = TRUE;
		ZWriteEnable = TRUE;
		AlphaBlendEnable = FALSE;
		
        VertexShader = compile vs_4_0 RenderShadowMapVS();
        PixelShader  = compile ps_4_0 RenderShadowMapPS();
	}
}
//reuse//code//DepthTextureEffect//
