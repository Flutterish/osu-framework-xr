#version 330 core
in vec2 uv;

out vec4 FragColor;

uniform sampler2D tex;
uniform vec4 subImage;
uniform vec4 tint;

// unlit is an opaque shader, therefore we use dithering for transparency
const float dither[16] = float[](
	0.0625, 0.5625, 0.1875,  0.6875,
	0.8125, 0.3125, 0.9375,  0.4375,
	0.25, 0.75, 0.125, 0.625,
	1.0, 0.5, 0.875,  0.375
);

float ditherLimit ( vec4 color ) {
	if ( color.a == 1 )
		return 1;

	return dither[ ((int(gl_FragCoord.x) % 4) * 4 + int(gl_FragCoord.y) % 4 + int(color.r * 1229 + color.g * 1231 + color.b * 1237)) % 16 ];
}

void main ()
{
	vec4 color = texture( tex, uv * subImage.zw + subImage.xy ) * tint;

	if ( color.a < ditherLimit( color ) )
		discard;
	FragColor = vec4(color.xyz, 1);
} 