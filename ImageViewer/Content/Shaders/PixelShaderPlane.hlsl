Texture2D ColorTexture;
SamplerState ColorSampler;

struct PixelShaderInput
{
	min16float4 pos		: SV_POSITION;
	min16float2 coord	: TEXCOORD0;
};

min16float4 main(PixelShaderInput input) : SV_TARGET
{ 
	return ColorTexture.Sample(ColorSampler, input.coord);
}
