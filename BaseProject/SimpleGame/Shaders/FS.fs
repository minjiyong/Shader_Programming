#version 330 core
layout(location = 0) out vec4 FragColor;
layout(location = 1) out vec4 FragColor1;

in vec2 v_UV;

uniform sampler2D u_RGBTexture;
uniform sampler2D u_DigitTexture;
uniform sampler2D u_NumTexture;

uniform float u_Time;   // 시간(초 단위)

const float PI = 3.141592;

vec4 Test()
{
	vec2 newUV = v_UV;
    float dx = 0.1 * sin(v_UV.y * 2 * PI * 2 + (4 * u_Time));
    float dy = 0.1 * sin(v_UV.x * 2 * PI * 2 + (4 * u_Time));
    newUV += vec2(dx, dy);

    vec4 sampledColor = texture(u_RGBTexture, newUV);

    return sampledColor;
}

vec4 Circles()
{
    vec2 newUV = v_UV;  //0~1, left top (0, 0)
    vec2 center = vec2(0.5, 0.5);
    vec4 newColor = vec4(0);

    float d = distance(newUV, center);

    float value = sin(d * 4 * PI * 4 - (u_Time * 4));
    newColor = vec4(value);

    return newColor;
}

vec4 Flag()
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
        //discard;
    }

    return newColor;
}

vec4 Q1()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  //0~1, left top (0, 0)
    float x = newUV.x;  // 0~1
    float y = 1-abs(2*(newUV.y - 0.5));    // 절댓값 1~0~1 -> 1- 하면 0~1~0

    // 그림 그려가면서 생각. 원래 텍스처상의 좌표에서 (0, 0), (1, 1) 찍어가면서 분석
    vec4 newColor = texture(u_RGBTexture, vec2(x, y));

    return newColor;
}

vec4 Q2()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  //0~1, left top (0, 0)
    float x = (3*newUV.x);  
    float y = (2-floor(newUV.x * 3))/3 + (newUV.y/3);

    vec4 newColor = texture(u_RGBTexture, vec2(x, y));

    return newColor;
}


// 그림 그려가면서 생각. 원래 텍스처상의 좌표랑 비교, r setion, g section... 나눠가면서 찍기
vec4 Q3()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  //0~1, left top (0, 0)
    float x = (3*newUV.x);  
    float y = floor(newUV.x * 3)/3 + (newUV.y/3);

    vec4 newColor = texture(u_RGBTexture, vec2(x, y));

   return newColor;
}

vec4 HorizontalBrick()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  //0~1, left top (0, 0)
    float rCount = 3;
    float sAmount = 0.5;
    float x = fract(newUV.x*rCount) + floor(newUV.y*rCount + 1) * sAmount; //0~1, 0~1
    float y = fract(newUV.y*rCount); //0~1, 0~1

    vec4 newColor = texture(u_RGBTexture, vec2(x, y));

    return newColor;
}

vec4 VerticalBrick()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  //0~1, left top (0, 0)
    float x = fract(newUV.x*2);  //0~1, 0~1
    float y = fract(newUV.y*2) + floor(newUV.x*2) * 0.5;  //0~1, 0~1

    vec4 newColor = texture(u_RGBTexture, vec2(x, y));

    return newColor;
}

vec4 Digit()
{
    return texture(u_DigitTexture, v_UV);
}

vec4 Digit_Num()
{
    int digit = int(u_Time)%10;

    int tileIndex = (digit + 9)%10;

    // 내가 짠거
    float offX = tileIndex * 0.2;
    float offY = round(tileIndex * 0.1 + 0.1) * 0.5;   // 12345 = 0. 67890 = 0.5

    // 교수님 ver
    //float offX = float(tileIndex % 5) * 0.2;
    //float offY = floor(float(tileIndex) * 0.2) * 0.5;

    float tx = v_UV.x * 0.2 + offX;
    float ty = v_UV.y * 0.5 + offY;

    return texture(u_NumTexture, vec2(tx, ty));
}

void main()
{
   // Test();
   // Circles();
   // Flag();
   // Q1();
   // Q2();
   //Q3();
   //HorizontalBrick();
   //VerticalBrick();
   //Digit();
   //Digit_Num();

   FragColor = Circles();
   FragColor1 = Flag();
}