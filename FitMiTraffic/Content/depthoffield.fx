#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0

Texture2D SpriteTexture;
float BlurDistance = 0.00f;

float2 resolution = float2(800, 600);

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};


struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 Tex : TEXCOORD0;
};


float4 MainPS(VertexShaderOutput input) : COLOR
{
	//float4 color = tex2D(SpriteTextureSampler, input.Tex);

	float4 real = tex2D(SpriteTextureSampler, input.Tex);

	float2 uv = input.Tex / resolution;

	float4 color = float4(0, 0, 0, 0);
	float2 off1 = float2(1.411764705882353, 1.411764705882353);
	float2 off2 = float2(3.2941176470588234, 3.2941176470588234);
	float2 off3 = float2(5.176470588235294, 5.176470588235294);
	color += tex2D(SpriteTextureSampler, input.Tex) * 0.1964825501511404;
	color += tex2D(SpriteTextureSampler, input.Tex + (off1 / resolution)) * 0.2969069646728344;
	color += tex2D(SpriteTextureSampler, input.Tex - (off1 / resolution)) * 0.2969069646728344;
	color += tex2D(SpriteTextureSampler, input.Tex + (off2 / resolution)) * 0.09447039785044732;
	color += tex2D(SpriteTextureSampler, input.Tex - (off2 / resolution)) * 0.09447039785044732;
	color += tex2D(SpriteTextureSampler, input.Tex + (off3 / resolution)) * 0.010381362401148057;
	color += tex2D(SpriteTextureSampler, input.Tex - (off3 / resolution)) * 0.010381362401148057;

	return real;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};