#version 330 core
in vec3 aPos;

uniform mat4 matrix;

void main()
{
    gl_Position = vec4(aPos, 1.0) * matrix;
}