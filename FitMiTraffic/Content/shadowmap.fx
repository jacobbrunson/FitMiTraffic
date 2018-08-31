matrix World;
matrix  LightViewProj;

struct CreateShadowMap_VSOut
{
	float4 Position : POSITION;
	float Depth : TEXCOORD0;
};

//  CREATE SHADOW MAP
CreateShadowMap_VSOut CreateShadowMap_VertexShader(float4 Position : SV_POSITION)
{
	CreateShadowMap_VSOut Out;
	Out.Position = mul(Position, LightViewProj);
	Out.Depth = Out.Position.z / Out.Position.w;

	return Out;
}

float4 CreateShadowMap_PixelShader(CreateShadowMap_VSOut input) : COLOR
{
	return float4(input.Depth, 0, 0, 0);
}

// Technique for creating the shadow map
technique CreateShadowMap
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 CreateShadowMap_VertexShader();
		PixelShader = compile ps_5_0 CreateShadowMap_PixelShader();
	}
}