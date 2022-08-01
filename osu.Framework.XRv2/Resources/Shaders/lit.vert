#version 330 core
in vec3 aPos;
in vec2 aUv;
in vec3 aNorm;

out vec2 uv;
out vec3 norm;
out vec3 FragPos;

uniform mat4 mMatrix;
uniform mat3 mNormal;
uniform mat4 gProj;

void main()
{
    uv = aUv;
    gl_Position = vec4(aPos, 1.0) * mMatrix * gProj;
    FragPos = (vec4(aPos, 1.0) * mMatrix).xyz;
    norm = normalize(aNorm * mNormal);
}