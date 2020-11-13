#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler inputTexture;


float4 MainPS(float2 textureCoordinates: TEXCOORD0) : COLOR0
{


	float4 cell00 = tex2D(inputTexture, textureCoordinates);
	float4 cell01 = tex2D(inputTexture, textureCoordinates + float2(0, 0.5));
	float4 cell10 = tex2D(inputTexture, textureCoordinates + float2(0.5, 0));
	float4 cell11 = tex2D(inputTexture, textureCoordinates + float2(0.5, 0.5));

	float4 color = (cell00 * cell01 * cell10 * cell11)*0.25;

	// does this look weird? more on this later:;
	return color;
}

technique Techninque1
{
	pass Pass1
	{
		AlphaBlendEnable = TRUE;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
		PixelShader = compile ps_3_0 MainPS();
	}
};