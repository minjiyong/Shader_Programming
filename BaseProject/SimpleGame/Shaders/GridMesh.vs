#version 330

in vec3 a_Position;

out vec4 v_Color;
out vec2 v_UV;

uniform float u_Time;
const float c_PI = 3.141592;

#define MAX_POINTS 100
uniform vec4 u_Points[MAX_POINTS];

// 1117 add Texture
void Flag() // 1014
{
    vec4 newPosition = vec4(a_Position, 1);

    float value = a_Position.x + 0.5; // 0~1

    //newPosition.y = newPosition.y * (1 - value);
   
    float dX = 0f;
    float dY = 0.5 * value * sin(2 * value * c_PI - u_Time * 4);
    float newColor = (sin(2 * value * c_PI - u_Time * 4) + 1) / 2;
   
    newPosition += vec4(dX, dY, 0, 0);
   
    gl_Position = newPosition;

    v_Color = vec4(newColor);
    v_UV = vec2(a_Position.x + 0.5, 0.5 - a_Position.y);
}

void Wave()
{
    vec4 newPosition = vec4(a_Position, 1);
    
    float dX = 0;
    float dY = 0;
    
    vec2 pos = vec2(a_Position.xy);
    vec2 cen = vec2(0, 0);
    float d = distance(pos, cen);
    float v = 2 * clamp(0.5 - d, 0, 1);
    
    float newColor = v * sin(d * 4 *  c_PI * 10 - u_Time * 30);
        
    newPosition += vec4(dX, dY, 0, 0);
    gl_Position = newPosition;
    
    v_Color = vec4(newColor);
}

void RainDrop()
{
    vec4 newPosition = vec4(a_Position, 1);
    
    float dX = 0;
    float dY = 0;
    float newColor = 0;
    
    vec2 pos = vec2(a_Position.xy);
    
    for (int i = 0; i < MAX_POINTS; i++)
    {
        float sTime = u_Points[i].z;
        float lTime = u_Points[i].w;
        float newTime = u_Time - sTime;
        
        if (newTime > 0)
        {
            float baseTime = fract(newTime / lTime);
            
            float oneMinus = 1 - baseTime;
            
            //float t = newTime;              // 이렇게 하면 순차적으로 파동 생성
            float t = baseTime * lTime;
            float range = baseTime * lTime / 15;
            vec2 cen = u_Points[i].xy;
            float d = distance(pos, cen);
            float v = 30 * clamp(range - d, 0, 1); // 이때 앞에 range 는.. 퍼져나가는 범위, 맨 앞 숫자는.. 진하기?
            newColor += oneMinus * v * sin(d * 4 * c_PI * 10 - t * 30);
        }
    }
    
    newPosition += vec4(dX, dY, 0, 0);
    gl_Position = newPosition;
    
    v_Color = vec4(newColor);
}


void main()
{
    Flag();
    //Wave();
    //RainDrop();

}