float4x4 View;
float4x4 Proj;
float4x4 World;

float4 ambientColor = float4(0.5, 0.5, 0.5, 1);
float3 camPos;

float3 diffuseDirection = float3(1, 0, 0);
float4 diffuseColor = float4(0, 0, 0, 1);
float diffuseIntensity = 0.2;

Texture tex0;
Texture tex1;
Texture tex2;
Texture tex3;
sampler sampler0 = sampler_state{texture = <tex0>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
sampler sampler1 = sampler_state{texture = <tex1>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
sampler sampler2 = sampler_state{texture = <tex2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
sampler sampler3 = sampler_state{texture = <tex3>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

const static int MAXPLIGHTS = 24;
uint numplights;
float4 plights[MAXPLIGHTS];
float4 plightscol[MAXPLIGHTS];
bool enableLighting = true;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
	float4 texWeights : TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;    
    float4 Color : COLOR0;
    float2 texCoord : TEXCOORD0;
    float4 lightFactor : TEXCOORD1;
	float depth : TEXCOORD2;
	float3 worldPos : TEXCOORD3;
	float3 norm : TEXCOORD4;
	float4 texWeights : TEXCOORD5;
};

VertexShaderOutput VertShader(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 wpos = mul(input.Position, World);
	float4 pos = mul(wpos, View);
    output.Position = mul(pos, Proj);
	output.Color = float4(0,0,0,0);
	output.texCoord = input.TextureCoordinate;
	output.lightFactor = dot(input.Normal, -diffuseDirection) * 2;
	output.depth = length(input.Position - camPos);
	output.worldPos = float3(wpos.x, wpos.y, wpos.z);
	output.norm = mul(input.Normal, World);
	output.texWeights = input.texWeights;

    return output;
}

float4 PixShader(VertexShaderOutput input) : COLOR0
{
	float4 col = float4(0,0,0,1);
	if (input.texWeights.x != 0)
		col += tex2D(sampler0, input.texCoord) * input.texWeights.x;
	if (input.texWeights.y != 0)
		col += tex2D(sampler1, input.texCoord) * input.texWeights.y;
	if (input.texWeights.z != 0)
		col += tex2D(sampler2, input.texCoord) * input.texWeights.z;
	if (input.texWeights.w != 0)
		col += tex2D(sampler3, input.texCoord) * input.texWeights.w;

	float d = clamp(input.depth / 50, 0, 1);

	col.rgb *= saturate(input.lightFactor) + ambientColor;
	
	if (enableLighting)
	{
		float4 orig = col;
		col.rgb *= 0.25f;
		for (uint i = 0; i < numplights; i++)
			if (plights[i].w > 0)
			{
				float3 lightDir = normalize(plights[i] - input.worldPos);
				float d = distance(input.worldPos, plights[i]);
				float att = 1 - pow(clamp(d/plights[i].w, 0, 1), 2);
				
				col += att * plightscol[i];
			}
	}
	
    return col;
}

technique Technique1
{
    pass Pass1
    {
        AlphaBlendEnable = FALSE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        VertexShader = compile vs_3_0 VertShader();
        PixelShader = compile ps_3_0 PixShader();
    }
}
