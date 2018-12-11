#define VS_VERSION vs_3_0
#define PS_VERSION ps_3_0

float4 AmbientColor;
float4 DiffuseColor;

float4 ChromaKeyReplace;

float3 LightPosition;
float4x4 LightMatrix;
Texture2D ShadowMap;
sampler2D shadow_sampler = sampler_state {
	Texture = (ShadowMap);
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};



texture ModelTexture;
sampler2D tex_sampler = sampler_state {
    Texture = (ModelTexture);
    MagFilter = Anisotropic;
    MinFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
};

sampler2D car_tex_sampler = sampler_state {
	Texture = (ModelTexture);
	MagFilter = Point;
	MinFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 NormalMatrix;

struct VertexShaderInput
{
    float4 Position : POSITION;
	float4 Normal : NORMAL;
	float2 TexCoords : TEXCOORD;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
	float4 Color : COLOR0;
	float4 RealColor : COLOR1;
	float3 Normal : NORMAL;
	float2 TexCoords : TEXCOORD;
	float4 PositionLight : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
};

struct PixelShaderOutput
{
	float4 Color : COLOR0;
};

float ComputeShadow(float4 positionLight)
{

	float2 shadow_map_coords = mad(0.5f , positionLight.xy / positionLight.w , float2(0.5f, 0.5f));
    shadow_map_coords.y = 1.0f - shadow_map_coords.y;

    float real_depth = (positionLight.z / positionLight.w);
	float closest_depth = tex2D(shadow_sampler, shadow_map_coords).x;

	return real_depth <= closest_depth + 0.0005 ? 1.0 : 0.0;
}

VertexShaderOutput ComputeDiffuse(float4 position, float4 normal, float2 tex_coords, float4 color)
{
	VertexShaderOutput output;

    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	float3 transformed_normal = normalize(mul(normal, NormalMatrix).rgb);
	float light_intensity = dot(LightPosition.rgb, transformed_normal);
	output.Color = saturate(DiffuseColor * light_intensity);
	output.RealColor = color;

	output.PositionLight = mul(position, LightMatrix);
	output.WorldPosition = worldPosition;
	output.Normal = normalize(mul(normal, (float3x3)World));

	output.TexCoords = tex_coords;

    return output;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    return ComputeDiffuse(input.Position, input.Normal, input.TexCoords, float4(1, 1, 1, 1));
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
    
	float4 tex_color;
	if (ChromaKeyReplace.a == -1) {
		tex_color = tex2D(tex_sampler, input.TexCoords);
	} else {
		tex_color = tex2D(car_tex_sampler, input.TexCoords);
	}

	tex_color.a = 1;

	if (tex_color.r <= 0.01 && tex_color.g >= 0.99 && tex_color.b <= 0.01) {
		tex_color = ChromaKeyReplace;
	}

	float shadow = ComputeShadow(input.PositionLight);
	output.Color = saturate(tex_color * (input.Color * shadow + AmbientColor));

	return output;
}


technique ShadowedScene
{
    pass Pass1
    {
        VertexShader = compile VS_VERSION VertexShaderFunction();
        PixelShader = compile PS_VERSION PixelShaderFunction();
    }
}

struct TerrainVertexShaderInput
{
    float4 Position : POSITION;
	float4 Normal : NORMAL;
	float4 Color : COLOR0;
};

VertexShaderOutput TerrainVertexShaderFunction(TerrainVertexShaderInput input)
{
    return ComputeDiffuse(input.Position, input.Normal, float2(0, 0), input.Color);
}

PixelShaderOutput TerrainPixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
    
	float shadow = ComputeShadow(input.PositionLight);
	output.Color = saturate(input.RealColor * (input.Color * shadow + AmbientColor));

	return output;
}

technique ShadowedTerrain
{
    pass Pass1
    {
        VertexShader = compile VS_VERSION TerrainVertexShaderFunction();
        PixelShader = compile PS_VERSION TerrainPixelShaderFunction();
    }
}


struct ShadowMapVertexShaderOutput
{
	float4 Position : POSITION;
	float4 Position2D : TEXCOORD0;
};

struct ShadowMapPixelShaderOutput
{
	float4 Color : COLOR0;
};


ShadowMapVertexShaderOutput ShadowMapVertexShader(float4 inPos : POSITION)
{
	ShadowMapVertexShaderOutput output;
	output.Position = mul(inPos, LightMatrix);
	output.Position2D = output.Position;
	return output;
}

ShadowMapPixelShaderOutput ShadowMapPixelShader(ShadowMapVertexShaderOutput input)
{
	ShadowMapPixelShaderOutput output;
	output.Color = input.Position2D.z / input.Position2D.w;
	return output;
}


technique ShadowMap
{
	pass Pass0
	{
		VertexShader = compile VS_VERSION ShadowMapVertexShader();
		PixelShader = compile PS_VERSION ShadowMapPixelShader();
	}
}