TextureCube ColorTexture;
SamplerState ColorSampler;

struct PixelShaderInput
{
	min16float4 pos		: SV_POSITION;
	min16float3 coord	: TEXCOORD0;
};

min16float4 main(PixelShaderInput input) : SV_TARGET
{
	return ColorTexture.Sample(ColorSampler, input.coord);
}
