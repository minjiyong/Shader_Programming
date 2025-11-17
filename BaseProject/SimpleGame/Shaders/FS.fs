#version 330 core
layout(location = 0) out vec4 FragColor;

in vec2 v_UV;

uniform sampler2D u_RGBTexture;
uniform float u_Time;   // 시간(초 단위)

const float PI = 3.141592;

void main(){
    vec2 newUV = v_UV;
    float dx = 0.1 * sin(v_UV.y * 2 * PI * 2 + (4 * u_Time));
    float dy = 0.1 * sin(v_UV.x * 2 * PI * 2 + (4 * u_Time));
    newUV += vec2(dx, dy);

    vec4 sampledColor = texture(u_RGBTexture, newUV);

    FragColor = sampledColor;
}