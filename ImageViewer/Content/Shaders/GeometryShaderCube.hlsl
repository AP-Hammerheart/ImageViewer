struct GeometryShaderInput
{
	min16float4 pos     : SV_POSITION;
	min16float3 coord   : TEXCOORD0;
	uint        instId  : TEXCOORD1;
};

struct GeometryShaderOutput
{
	min16float4 pos     : SV_POSITION;
	min16float3 coord   : TEXCOORD0;
	uint        rtvId   : SV_RenderTargetArrayIndex;
};

[maxvertexcount(3)]
void main(triangle GeometryShaderInput input[3], inout TriangleStream<GeometryShaderOutput> outStream)
{
	GeometryShaderOutput output;
	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		output.pos = input[i].pos;
		output.coord = input[i].coord;
		output.rtvId = input[i].instId;
		outStream.Append(output);
	}
}
