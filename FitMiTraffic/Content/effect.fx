float4x4 World;
float4x4 View;
float4x4 Projection;

float4 AmbientColor = float4(1, 0.8, 0.8, 1);//float4(0.9, 0.9, 1, 1);
float AmbientIntensity = 0.4f;//0.8f;

float4x4 WorldInverseTranspose;

float3 xLightPos;
float3 DiffuseLightDirection;
float4 DiffuseColor = float4(1, 0.8, 0.8, 1);
float DiffuseIntensity = 0.8f;//1.0f;

float2 resolution;

float4 CarColor;

matrix  LightViewProj;
float ShadowMapSize = float2(2048, 2048);

texture ModelTexture;
sampler2D textureSampler = sampler_state {
	Texture = (ModelTexture);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	AddressU = Clamp;
	AddressV = Clamp;
};


sampler2D carTextureSampler = sampler_state {
	Texture = (ModelTexture);
	MagFilter = Point;
	MinFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

Texture2D ShadowMap;
sampler ShadowMapSampler = sampler_state { texture = <ShadowMap>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = clamp; AddressV = clamp; };

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 WorldPos : POSITION1;
	float4 Color : COLOR0;
	float3 Normal : TEXCOORD0;
	float2 TextureCoordinate : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPos = worldPosition;

	float4 normal = mul(input.Normal, WorldInverseTranspose);
	float lightIntensity = dot(normal, DiffuseLightDirection);
	output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);

	output.Normal = normal;
	output.TextureCoordinate = input.TextureCoordinate;

	return output;
}


/*float CalcShadowTermPCF(float light_space_depth, float ndotl, float2 shadow_coord)
{
	float shadow_term = 0;
	float DepthBias = 0.02;

	//float2 v_lerps = frac(ShadowMapSize * shadow_coord);

	float variableBias = clamp(0.001 * tan(acos(ndotl)), 0, DepthBias);

	//safe to assume it's a square
	float size = 1 / ShadowMapSize.x;

	float samples[4];
	samples[0] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord).r);
	samples[1] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord + float2(size, 0)).r);
	samples[2] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord + float2(0, size)).r);
	samples[3] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord + float2(size, size)).r);

	shadow_term = (samples[0] + samples[1] + samples[2] + samples[3]) / 4.0;
	//shadow_term = lerp(lerp(samples[0],samples[1],v_lerps.x),lerp(samples[2],samples[3],v_lerps.x),v_lerps.y);

	return shadow_term;
}*/





float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
	textureColor.a = 1;



	float4 lightingPosition = mul(input.WorldPos, LightViewProj);
	// Find the position in the shadow map for this pixel
	float2 ShadowTexCoord = mad(0.5f, lightingPosition.xy / lightingPosition.w, float2(0.5f, 0.5f));
	ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

	// Get the current depth stored in the shadow map
	float ourdepth = (lightingPosition.z / lightingPosition.w);
	float NdotL = dot(input.Normal, DiffuseLightDirection);
	//float shadowContribution = CalcShadowTermPCF(ourdepth, NdotL, ShadowTexCoord);




	return saturate(textureColor + AmbientColor * AmbientIntensity);
}

technique Textured
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}


//SHADOW MAP

float4x4 xLightsWorldViewProjection;

struct SMapVertexToPixel
{
	float4 Position     : POSITION;
	float4 Position2D    : TEXCOORD0;
};

struct SMapPixelToFrame
{
	float4 Color : COLOR0;
};


SMapVertexToPixel ShadowMapVertexShader(float4 inPos : POSITION)
{
	SMapVertexToPixel Output = (SMapVertexToPixel)0;

	Output.Position = mul(inPos, xLightsWorldViewProjection);
	Output.Position2D = Output.Position;

	return Output;
}

SMapPixelToFrame ShadowMapPixelShader(SMapVertexToPixel PSIn)
{
	SMapPixelToFrame Output = (SMapPixelToFrame)0;

	Output.Color = PSIn.Position2D.z / PSIn.Position2D.w;

	return Output;
}


technique ShadowMap
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 ShadowMapVertexShader();
		PixelShader = compile ps_4_0 ShadowMapPixelShader();
	}
}



//SHADOW SCENE

struct SSceneVertexToPixel
{
	float4 Position             : POSITION;
	float4 Pos2DAsSeenByLight    : TEXCOORD0;

	float2 TexCoords            : TEXCOORD1;
	float3 Normal                : TEXCOORD2;
	float4 Position3D            : TEXCOORD3;
};


struct STerrainVertexToPixel
{
	float4 Position             : POSITION;
	float4 Pos2DAsSeenByLight    : TEXCOORD0;

	float3 Normal                : TEXCOORD2;
	float4 Position3D            : TEXCOORD3;
	float4 Color : COLOR0;
};

struct SScenePixelToFrame
{
	float4 Color : COLOR0;
};

float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
	float3 lightDir = normalize(lightPos - pos3D);
	return dot(-lightDir, normal);
}

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


STerrainVertexToPixel ShadowedTerrainVertexShader(float4 inPos : POSITION0, float4 inColor : COLOR0, float3 inNormal : NORMAL0)
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

SScenePixelToFrame ShadowedScenePixelShader(SSceneVertexToPixel PSIn)
{
	SScenePixelToFrame Output = (SScenePixelToFrame)0;

	float2 ProjectedTexCoords;
	ProjectedTexCoords[0] = PSIn.Pos2DAsSeenByLight.x / PSIn.Pos2DAsSeenByLight.w / 2.0f + 0.5f;
	ProjectedTexCoords[1] = -PSIn.Pos2DAsSeenByLight.y / PSIn.Pos2DAsSeenByLight.w / 2.0f + 0.5f;

	float diffuseLightingFactor = 0;
	if ((saturate(ProjectedTexCoords).x == ProjectedTexCoords.x) && (saturate(ProjectedTexCoords).y == ProjectedTexCoords.y))
	{
		float depthStoredInShadowMap = tex2D(ShadowMapSampler, ProjectedTexCoords).r;
		float realDistance = PSIn.Pos2DAsSeenByLight.z / PSIn.Pos2DAsSeenByLight.w;
		if ((realDistance - 1.0f / 100.0f) <= depthStoredInShadowMap)
		{
			diffuseLightingFactor = DotProduct(xLightPos, PSIn.Position3D, PSIn.Normal);
			diffuseLightingFactor = saturate(diffuseLightingFactor);
			diffuseLightingFactor *= DiffuseIntensity;//xLightPower;
		}
	}
	else {
		diffuseLightingFactor = DotProduct(xLightPos, PSIn.Position3D, PSIn.Normal);
		diffuseLightingFactor = saturate(diffuseLightingFactor);
		diffuseLightingFactor *= DiffuseIntensity;//xLightPower;
	}

	float2 uv = (PSIn.Position / resolution);
	uv *= 1.0f - uv.yx;

	float len = length(uv);
	float vignette = pow(uv.x * uv.y * 15.0f, 0.1f);

	//vignette = 1;

	float4 baseColor = tex2D(textureSampler, PSIn.TexCoords);
	Output.Color = baseColor * (DiffuseColor * diffuseLightingFactor + AmbientColor * AmbientIntensity) * vignette;

	return Output;
}

SScenePixelToFrame ShadowedTerrainPixelShader(STerrainVertexToPixel PSIn)
{
	SScenePixelToFrame Output = (SScenePixelToFrame)0;

	float2 ProjectedTexCoords;
	ProjectedTexCoords[0] = PSIn.Pos2DAsSeenByLight.x / PSIn.Pos2DAsSeenByLight.w / 2.0f + 0.5f;
	ProjectedTexCoords[1] = -PSIn.Pos2DAsSeenByLight.y / PSIn.Pos2DAsSeenByLight.w / 2.0f + 0.5f;

	float diffuseLightingFactor = 0;
	if ((saturate(ProjectedTexCoords).x == ProjectedTexCoords.x) && (saturate(ProjectedTexCoords).y == ProjectedTexCoords.y))
	{
		float depthStoredInShadowMap = tex2D(ShadowMapSampler, ProjectedTexCoords).r;
		float realDistance = PSIn.Pos2DAsSeenByLight.z / PSIn.Pos2DAsSeenByLight.w;
		if ((realDistance - 1.0f / 100.0f) <= depthStoredInShadowMap)
		{
			diffuseLightingFactor = DotProduct(xLightPos, PSIn.Position3D, PSIn.Normal);
			diffuseLightingFactor = saturate(diffuseLightingFactor);
			diffuseLightingFactor *= DiffuseIntensity;//xLightPower;
		}
	}
	else {
		diffuseLightingFactor = DotProduct(xLightPos, PSIn.Position3D, PSIn.Normal);
		diffuseLightingFactor = saturate(diffuseLightingFactor);
		diffuseLightingFactor *= DiffuseIntensity;//xLightPower;
	}
	float2 uv = (PSIn.Position / resolution);
	uv *= 1.0f - uv.yx;

	float len = length(uv);
	float vignette = pow(uv.x * uv.y * 15.0f, 0.1f);

	//vignette = 1;

	Output.Color = PSIn.Color * (DiffuseColor * diffuseLightingFactor + AmbientColor * AmbientIntensity) * vignette;

	return Output;
}

SScenePixelToFrame ShadowedCarPixelShader(SSceneVertexToPixel PSIn)
{
	SScenePixelToFrame Output = (SScenePixelToFrame)0;

	float2 ProjectedTexCoords;
	ProjectedTexCoords[0] = PSIn.Pos2DAsSeenByLight.x / PSIn.Pos2DAsSeenByLight.w / 2.0f + 0.5f;
	ProjectedTexCoords[1] = -PSIn.Pos2DAsSeenByLight.y / PSIn.Pos2DAsSeenByLight.w / 2.0f + 0.5f;

	float diffuseLightingFactor = 0;
	if ((saturate(ProjectedTexCoords).x == ProjectedTexCoords.x) && (saturate(ProjectedTexCoords).y == ProjectedTexCoords.y))
	{
		float depthStoredInShadowMap = tex2D(ShadowMapSampler, ProjectedTexCoords).r;
		float realDistance = PSIn.Pos2DAsSeenByLight.z / PSIn.Pos2DAsSeenByLight.w;
		if ((realDistance - 1.0f / 100.0f) <= depthStoredInShadowMap)
		{
			diffuseLightingFactor = DotProduct(xLightPos, PSIn.Position3D, PSIn.Normal);
			diffuseLightingFactor = saturate(diffuseLightingFactor);
			diffuseLightingFactor *= DiffuseIntensity;//xLightPower;
		}
	}

	float2 uv = (PSIn.Position / resolution);
	uv *= 1.0f - uv.yx;

	float len = length(uv);
	float vignette = pow(uv.x * uv.y * 15.0f, 0.1f);

	//vignette = 1;

	float4 baseColor = tex2D(carTextureSampler, PSIn.TexCoords);
	if (baseColor.r <= 0.05 && baseColor.g >= 0.95 && baseColor.b <= 0.05) {
		baseColor = CarColor;
	}

	Output.Color = baseColor * (DiffuseColor * diffuseLightingFactor + AmbientColor * AmbientIntensity) * vignette;

	return Output;
}


technique ShadowedScene
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 ShadowedSceneVertexShader();
		PixelShader = compile ps_4_0 ShadowedScenePixelShader();
	}
}


technique ShadowedTerrain
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 ShadowedTerrainVertexShader();
		PixelShader = compile ps_4_0 ShadowedTerrainPixelShader();
	}
}

technique ShadowedCar
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 ShadowedSceneVertexShader();
		PixelShader = compile ps_4_0 ShadowedCarPixelShader();
	}
}
