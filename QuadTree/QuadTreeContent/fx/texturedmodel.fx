float4x4 View;
float4x4 Proj;
float4x4 World;

float4 ambientColor = float4(0.5, 0.5, 0.5, 1);

float3 diffuseDirection = float3(1, 0, 0);
float4 diffuseColor = float4(0, 0, 0, 1);
float diffuseIntensity = 0.2;

const static int MAXPLIGHTS = 24;
uint numplights;
float4 plights[MAXPLIGHTS];
float4 plightscol[MAXPLIGHTS];
bool enableLighting = true;

Texture tex;
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
	float3 Normal : NORMAL0;
    float4 Color : COLOR0;
	float2 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;    
    float4 Color : COLOR0;
    float4 lightFactor : TEXCOORD1;
	float3 worldPos : TEXCOORD2;
	float2 TexCoords : TEXCOORD3;
	float3 norm : TEXCOORD4;
};

VertexShaderOutput VertShader(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 wpos = mul(input.Position, World);
	float4 pos = mul(wpos, View);
    output.Position = mul(pos, Proj);
	output.Color = input.Color;
	float3 norm = mul(input.Normal, World);
	output.lightFactor = dot(norm, -diffuseDirection) * 2;
	output.worldPos = wpos;
	output.TexCoords = input.TexCoords;
	output.norm = norm;

    return output;
}

float4 PixShader(VertexShaderOutput input) : COLOR0
{
	float4 col = tex2D(texSampler, input.TexCoords);
	col.rgb *= saturate(input.lightFactor) + ambientColor;
	
	if (enableLighting)
	{
		float4 orig = col;
		col.rgb *= 0.25f;
		for (uint i = 0; i < numplights; i++)
			if (plights[i].w > 0)
			{
				float3 lightDir = normalize(plights[i] - input.worldPos);
				float diffuse = saturate(dot(normalize(input.norm), lightDir));
				float d = distance(input.worldPos, plights[i]);
				float att = 1 - pow(clamp(d/plights[i].w, 0, 1), 2);
				
				col += diffuse * att * plightscol[i];
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
