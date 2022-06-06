#version 330 core
#define GAMMA 2.4
in vec2 uv;

out vec4 FragColor;

uniform sampler2D tex;

lowp float toSRGB ( lowp float color ) {
    return color < 0.0031308 ? ( 12.92 * color ) : ( 1.055 * pow( color, 1.0 / GAMMA ) - 0.055 );
}

lowp vec4 toSRGB(lowp vec4 colour)
{
    return vec4( toSRGB( colour.r ), toSRGB( colour.g ), toSRGB( colour.b ), colour.a );
}

void main()
{
    FragColor = toSRGB( texture( tex, vec2( uv.x, 1.0 - uv.y /* once again, o!f and openGL disagree where up is */ ) ) );
} 