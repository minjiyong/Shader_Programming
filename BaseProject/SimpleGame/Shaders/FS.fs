#version 330 core
layout(location = 0) out vec4 FragColor;

in vec2 v_UV;

uniform sampler2D u_RGBTexture;
uniform float u_Time;   // 시간(초 단위)

const float PI = 3.141592;

void Test()
{
	vec2 newUV = v_UV;
    float dx = 0.1 * sin(v_UV.y * 2 * PI * 2 + (4 * u_Time));
    float dy = 0.1 * sin(v_UV.x * 2 * PI * 2 + (4 * u_Time));
    newUV += vec2(dx, dy);

    vec4 sampledColor = texture(u_RGBTexture, newUV);

    FragColor = sampledColor;
}

void Circles()
{
    vec2 newUV = v_UV;  //0~1, left top (0, 0)
    vec2 center = vec2(0.5, 0.5);
    vec4 newColor = vec4(0);

    float d = distance(newUV, center);

    float value = sin(d * 4 * PI * 4 - (u_Time * 4));
    newColor = vec4(value);

    FragColor = newColor;
}

void Flag()
{
    vec2 newUV = vec2(v_UV.x, 1-v_UV.y-0.5);  //0~1, left bottom (0, 0)
    vec4 newColor = vec4(0);

    float width = 0.2 * (1 - newUV.x);  // 끝을 뾰족하게
    float sinValue = v_UV.x * 0.2 * sin(newUV.x * 2 * PI + u_Time);

    if(newUV.y < sinValue + width && newUV.y > sinValue - width)
    {
        newColor = vec4(1);
    }
    else 
    {
        discard;
    }

    FragColor = newColor;
}

void Q1()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  //0~1, left top (0, 0)
    float x = newUV.x;  // 0~1
    float y = 1-abs(2*(newUV.y - 0.5));    // 절댓값 1~0~1 -> 1- 하면 0~1~0

    // 그림 그려가면서 생각. 원래 텍스처상의 좌표에서 (0, 0), (1, 1) 찍어가면서 분석
    vec4 newColor = texture(u_RGBTexture, vec2(x, y));
    FragColor = newColor;
}

void Q2()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  //0~1, left top (0, 0)
    float x = (3*newUV.x);  
    float y = (2-floor(newUV.x * 3))/3 + (newUV.y/3);

    vec4 newColor = texture(u_RGBTexture, vec2(x, y));
    FragColor = newColor;
}


// 그림 그려가면서 생각. 원래 텍스처상의 좌표랑 비교, r setion, g section... 나눠가면서 찍기
void Q3()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  //0~1, left top (0, 0)
    float x = (3*newUV.x);  
    float y = floor(newUV.x * 3)/3 + (newUV.y/3);

    vec4 newColor = texture(u_RGBTexture, vec2(x, y));
    FragColor = newColor;
}

void main()
{
   // Test();
   // Circles();
   // Flag();
   // Q1();
   // Q2();
   Q3();
}