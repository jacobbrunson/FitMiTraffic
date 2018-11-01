#define VS_VERSION ps_3_0
#define PS_VERSION ps_3_0

float4x4 World;
float4x4 View;
float4x4 Projection;

float4 AmbientColor;
float AmbientIntensity;

float4x4 WorldInverseTranspose;
float4x4 xLightsWorldViewProjection;

float3 xLightPos;
float4 DiffuseColor = float4(1, 0.8, 0.8, 1);
float DiffuseIntensity;

float2 resolution;

float4 CarColor;

matrix  LightViewProj;
float ShadowMapSize = float2(1024, 1024);

texture ModelTexture;
Texture2D ShadowMap;


//SAMPLERS
sampler2D textureSampler = sampler_state { Texture = (ModelTexture); MagFilter = Anisotropic; MinFilter = Anisotropic; AddressU = Clamp; AddressV = Clamp; };
sampler2D carTextureSampler = sampler_state { Texture = (ModelTexture); MagFilter = Point; MinFilter = Point; AddressU = Clamp; AddressV = Clamp; };
sampler ShadowMapSampler = sampler_state { texture = <ShadowMap>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = clamp; AddressV = clamp; };

//TODO: delete
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};
//TODO: delete
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 WorldPos : POSITION1;
	float4 Color : COLOR0;
	float3 Normal : TEXCOORD0;
	float2 TextureCoordinate : TEXCOORD1;
};

//SHADOW MAP
struct ShadowMapVSOut
{
	float4 Position     : POSITION;
	float4 Position2D    : TEXCOORD0;
};

struct ShadowMapPSOut
{
	float4 Color : COLOR0;
};


ShadowMapVSOut ShadowMapVertexShader(float4 inPos : POSITION)
{
	ShadowMapVSOut Output = (ShadowMapVSOut)0;
	Output.Position = mul(inPos, xLightsWorldViewProjection);
	Output.Position2D = Output.Position;
	return Output;
}

ShadowMapPSOut ShadowMapPixelShader(ShadowMapVSOut PSIn)
{
	ShadowMapPSOut Output = (ShadowMapPSOut)0;
	Output.Color = PSIn.Position2D.z / PSIn.Position2D.w;
	return Output;
}

technique ShadowMap
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 ShadowMapVertexShader();
		PixelShader = compile ps_3_0 ShadowMapPixelShader();
	}
}

//Common functions
float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
	float3 lightDir = normalize(lightPos - pos3D);
	return dot(-lightDir, normal);
}

float DiffuseIntensityWithShadows(float4 PosFromLight, float4 Position3D, float3 Normal) {
	float2 ProjectedTexCoords;
	ProjectedTexCoords[0] = PosFromLight.x / PosFromLight.w / 2.0f + 0.5f;
	ProjectedTexCoords[1] = -PosFromLight.y / PosFromLight.w / 2.0f + 0.5f;

	float diffuseLightingFactor = 0;
	if ((saturate(ProjectedTexCoords).x == ProjectedTexCoords.x) && (saturate(ProjectedTexCoords).y == ProjectedTexCoords.y))
	{
		float depthStoredInShadowMap = tex2D(ShadowMapSampler, ProjectedTexCoords).r;
		float realDistance = PosFromLight.z / PosFromLight.w;
		if ((realDistance - 1.0f / 100.0f) <= depthStoredInShadowMap)
		{
			diffuseLightingFactor = DotProduct(xLightPos, Position3D, Normal);
			diffuseLightingFactor = saturate(diffuseLightingFactor);
			diffuseLightingFactor *= DiffuseIntensity;
		}
	}
	else {
		diffuseLightingFactor = DotProduct(xLightPos, Position3D, Normal);
		diffuseLightingFactor = saturate(diffuseLightingFactor);
		diffuseLightingFactor *= DiffuseIntensity;
	}
	return diffuseLightingFactor;
}

float Vignette(float4 Position) {
	return 1; //Disable vignette
	float2 uv = (Position / resolution);
	uv *= 1.0f - uv.yx;
	return pow(uv.x * uv.y * 15.0f, 0.1f);
}

//SHADOWED SCENE

struct SSceneVertexToPixel
{
	float4 Position             : SV_POSITION;
	float4 Pos2DAsSeenByLight    : TEXCOORD0;

	float2 TexCoords            : TEXCOORD1;
	float3 Normal                : TEXCOORD2;
	float4 Position3D            : TEXCOORD3;
};

struct SScenePixelToFrame
{
	float4 Color : COLOR0;
};

SSceneVertexToPixel ShadowedSceneVertexShader(float4 inPos : POSITION, float2 inTexCoords : TEXCOORD0, float3 inNormal : NORMAL)
{
	SSceneVertexToPixel Output = (SSceneVertexToPixel)0;

	float4 worldPosition = mul(inPos, World);
	float4 viewPosition = mul(worldPosition, View);
	Output.Position = mul(viewPosition, Projection);
	Output.Pos2DAsSeenByLight = mul(inPos, xLightsWorldViewProjection);
	Output.Normal = normalize(mul(inNormal, (float3x3)World));
	Output.Position3D = mul(inPos, World);
	Output.TexCoords = inTexCoords;

	return Output;
}

SScenePixelToFrame ShadowedScenePixelShader(SSceneVertexToPixel PSIn)
{
	SScenePixelToFrame Output = (SScenePixelToFrame)0;

	float diffuseLightingFactor = DiffuseIntensityWithShadows(PSIn.Pos2DAsSeenByLight, PSIn.Position3D, PSIn.Normal);
	//diffuseLightingFactor  = 2;
	float vignette = Vignette(PSIn.Position);
	vignette = 1;
	float4 baseColor = tex2D(textureSampler, PSIn.TexCoords);
	Output.Color = baseColor * (DiffuseColor * diffuseLightingFactor + AmbientColor * AmbientIntensity) * vignette;

	return Output;
}

technique ShadowedScene
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 ShadowedSceneVertexShader();
		PixelShader = compile ps_3_0 ShadowedScenePixelShader();
	}
}

//SHADOWED TERRAIN
struct STerrainVertexToPixel
{
	float4 Position             : SV_POSITION;
	float4 Pos2DAsSeenByLight    : TEXCOORD0;

	float3 Normal                : TEXCOORD2;
	float4 Position3D            : TEXCOORD3;
	float4 Color : TEXCOORD1;
};

STerrainVertexToPixel ShadowedTerrainVertexShader(float4 inPos : SV_POSITION, float4 inColor : TEXCOORD1, float3 inNormal : NORMAL)
{
	STerrainVertexToPixel Output = (STerrainVertexToPixel)0;

	float4 worldPosition = mul(inPos, World);
	float4 viewPosition = mul(worldPosition, View);
	Output.Position = mul(viewPosition, Projection);
	Output.Pos2DAsSeenByLight = mul(inPos, xLightsWorldViewProjection);
	Output.Normal = mul(inNormal, WorldInverseTranspose);
	Output.Position3D = mul(inPos, World);
	Output.Color = inColor;

	return Output;
}

SScenePixelToFrame ShadowedTerrainPixelShader(STerrainVertexToPixel PSIn)
{
	SScenePixelToFrame Output = (SScenePixelToFrame)0;

	//float diffuseLightingFactor = DiffuseIntensityWithShadows(PSIn.Pos2DAsSeenByLight, PSIn.Position3D, PSIn.Normal);

	//float vignette = Vignette(PSIn.Position);

	//Output.Color = PSIn.Color * (DiffuseColor * diffuseLightingFactor + AmbientColor * AmbientIntensity) * vignette;

	Output.Color = float4(1, 1, 1, 1);

	return Output;
}

technique ShadowedTerrain
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 ShadowedTerrainVertexShader();
		PixelShader = compile ps_3_0 ShadowedTerrainPixelShader();
	}
}


//SHADOWED CAR
SScenePixelToFrame ShadowedCarPixelShader(SSceneVertexToPixel PSIn)
{
	SScenePixelToFrame Output = (SScenePixelToFrame)0;

	float diffuseLightingFactor = DiffuseIntensityWithShadows(PSIn.Pos2DAsSeenByLight, PSIn.Position3D, PSIn.Normal);

	float vignette = Vignette(PSIn.Position);

	float4 baseColor = tex2D(carTextureSampler, PSIn.TexCoords);
	if (baseColor.r <= 0.05 && baseColor.g >= 0.95 && baseColor.b <= 0.05) {
		//baseColor = CarColor;
	}

	Output.Color = baseColor * (DiffuseColor * diffuseLightingFactor + AmbientColor * AmbientIntensity) * vignette;

	return Output;
}

technique ShadowedCar
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 ShadowedSceneVertexShader();
		PixelShader = compile ps_3_0 ShadowedCarPixelShader();
	}
}
