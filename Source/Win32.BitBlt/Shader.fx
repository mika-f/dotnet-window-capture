struct VS_INPUT {
	float4 position: POSITION;
	float2 uv : TEXCOORD;
};

struct PS_INPUT {
	float4 position : SV_POSITION;
	float2 uv : TEXCOORD;
};

Texture2D g_texture : register(t0);
SamplerState g_sampler : register(s0);

PS_INPUT VS(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;
	output.position = input.position;
	output.uv = input.uv;

	return output;
}

float4 PS(PS_INPUT input) : SV_TARGET
{
	return g_texture.Sample(g_sampler, input.uv);
}