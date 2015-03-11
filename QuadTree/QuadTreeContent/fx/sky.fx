float4x4 W;
float4x4 VP;

float3 camPos;

Texture tex;
samplerCUBE texSampler = sampler_state
{
	texture = <tex>;
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter = LINEAR; 
	AddressU = Mirror; 
	AddressV = Mirror; 
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 pw = mul(input.Position, W );
    output.Position = mul(pw, VP);
	
	output.TextureCoordinate = pw - camPos;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return texCUBE(texSampler, normalize(input.TextureCoordinate));;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
