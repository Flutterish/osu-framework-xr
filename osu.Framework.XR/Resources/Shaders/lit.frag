#version 330 core
in vec2 uv;
in vec3 norm;
in vec3 FragPos;

out vec4 FragColor;

uniform sampler2D tex;
uniform vec4 subImage;
uniform vec4 tint;
uniform vec3 viewPos;
uniform vec3 lightPos;
uniform vec3 lightColor;
uniform float specularStr;
uniform float ambientStr;
uniform float specularExp;

void main()
{
    vec3 lightDir = normalize(lightPos - FragPos);
    vec3 diffuse = vec3( abs(dot(norm, lightDir)) ) * lightColor;

    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(abs(dot(viewDir, reflectDir)), specularExp);
    vec3 specular = specularStr * spec * lightColor;

    vec3 ambient = ambientStr * lightColor;
    
    FragColor = vec4(diffuse + ambient + specular, 1) * texture( tex, uv * subImage.zw + subImage.xy ) * tint;
} 