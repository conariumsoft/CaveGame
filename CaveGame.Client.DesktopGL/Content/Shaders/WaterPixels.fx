#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Our texture sampler

float xSize;
float ySize;
float xDraw;
float yDraw;

float4 filterColor;


texture Texture;
sampler TextureSampler = sampler_state
{
	Texture = <Texture>;
};

// This data comes from the sprite batch vertex shader
struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCordinate : TEXCOORD0;
};


// Our pixel shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR
{
	float4 texColor = tex2D(TextureSampler, input.TextureCordinate);

	float vertPixSize = 4.0f / ySize;
	float horPixSize = 4.0f / xSize;

	float xPixelSize = 1.0f / xSize;
	float yPixelSize = 1.0f / ySize;

	float4 color;
	if (xDraw / xSize < 50.0f || yDraw / ySize < 50.0f)
	{
	// pass 1 (extreme antialias blur)

		float4 aboveColor = tex2D(TextureSampler, (input.TextureCordinate) + float2(0, -vertPixSize));
		float4 belowColor = tex2D(TextureSampler, (input.TextureCordinate) + float2(0, vertPixSize));
		float4 leftColor = tex2D(TextureSampler, (input.TextureCordinate) + float2(-horPixSize, 0));
		float4 rightColor = tex2D(TextureSampler, (input.TextureCordinate) + float2(horPixSize, 0));


		 float4 smallscalecolor = float4((texColor.r + aboveColor.r + belowColor.r + leftColor.r + rightColor.r) / 5,
		 (texColor.g + aboveColor.g + belowColor.g + leftColor.g + rightColor.g) / 5,
		 (texColor.b + aboveColor.b + belowColor.b + leftColor.b + rightColor.b) / 5,
		 (texColor.a + aboveColor.a + belowColor.a + leftColor.a + rightColor.a) / 5);

		 // pass 2 (4x4 cubing)
		 float cx = (floor(((input.TextureCordinate.x * xSize))/4)*4)/xSize;
		 float cy = (floor(((input.TextureCordinate.y * ySize))/4)*4)/ySize;

		float2 coordinates = float2(cx, cy);
		float4 belowPixelColor = tex2D(TextureSampler, (coordinates)+float2(0, yPixelSize));
		float4 bottomRightPixelColor = tex2D(TextureSampler, (coordinates)+float2(xPixelSize, yPixelSize));
		float4 rightPixelColor = tex2D(TextureSampler, (coordinates)+float2(xPixelSize, 0));
		float4 thisPixelColor = tex2D(TextureSampler, coordinates);

		color = float4((belowPixelColor.r + bottomRightPixelColor.r + rightPixelColor.r + thisPixelColor.r) / 4,
			(belowPixelColor.g + bottomRightPixelColor.g + rightPixelColor.g + thisPixelColor.g) / 4,
			(belowPixelColor.b + bottomRightPixelColor.b + rightPixelColor.b + thisPixelColor.b) / 4,
			(belowPixelColor.a + bottomRightPixelColor.a + rightPixelColor.a + thisPixelColor.a) / 4);

		color.rgb *= 0.7f;
		smallscalecolor.rgb *= 0.3f;
		smallscalecolor.a =0;
		color += smallscalecolor;
	} else {
		color = float4(texColor.r, texColor.g, texColor.b, texColor.a);
	}
	return color;
}

// Compile our shader
technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
