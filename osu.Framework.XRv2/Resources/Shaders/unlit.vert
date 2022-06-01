#version 330 core
in vec3 aPos;

uniform mat4 matrix;
uniform mat4 gProj;

void main()
{
    gl_Position = vec4(aPos, 1.0) * matrix * gProj;
}