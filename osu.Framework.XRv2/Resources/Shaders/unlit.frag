﻿#version 330 core
in vec2 uv;

out vec4 FragColor;

uniform sampler2D tex;
uniform vec4 subImage;

void main()
{
    FragColor = texture( tex, uv * subImage.zw + subImage.xy );
} 