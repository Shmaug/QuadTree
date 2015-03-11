float4x4 rot;
float4x4 W;
float4x4 VP;
Texture tex;

const static int MAXPLIGHTS = 24;
uint numplights;
float4 plights[MAXPLIGHTS];
float4 plightscol[MAXPLIGHTS];
bool enableLighting = true;

sampler texSampler = sampler_state
{
	texture = <tex>;
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter = LINEAR; 
	AddressU = Clamp; 
	AddressV = Clamp; 
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TextureCoordinate : TEXCOORD0;
	float3 worldPos : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4x4 wr = mul(rot, W);
	float4x4 WVP = mul(wr, VP);
    output.Position = mul(input.Position, WVP);
	output.worldPos = mul(input.Position, wr);
	
	output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 col = tex2D(texSampler, input.TextureCoordinate);
	if (col.a <= 0)
		clip(-1);
	else
	{
		if (enableLighting)
		{
			float4 orig = col;
			col.rgb *= 0.25f;
			for (uint i = 0; i < numplights; i++)
				if (plights[i].w > 0)
				{
					float3 lightDir = normalize(plights[i] - input.worldPos);
					float diffuse = .5f;
					float d = distance(input.worldPos, plights[i]);
					float att = 1 - pow(clamp(d/plights[i].w, 0, 1), 2);
					
					col += diffuse * att * plightscol[i];
				}
		}
	}
    return col;
}

technique Technique1
{
    pass Pass1
    {
        AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
