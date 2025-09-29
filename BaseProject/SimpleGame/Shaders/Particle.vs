#version 330

in vec3 a_Position;
in float a_Radius;
in vec4 a_Color;
in float a_STime;
in vec3 a_Velocity;
in float a_LifeTime;
in float a_Mass;

out vec4 v_Color;

uniform float u_Time;
uniform vec3 u_Force;

const float c_PI = 3.141592;
const vec2 c_G = vec2(0, -9.8); // 0923

void main()
{
   float lifeTime = a_LifeTime;
   float newAlpha = 1.0;
   vec4 newPosition = vec4(a_Position, 1);
   float newTime = u_Time - a_STime;

   if(newTime > 0){
      float t = fract(newTime / lifeTime) * lifeTime;     // t: 0 ~ lifeTime
      float tt = t * t;

      float forceX = u_Force.x + c_G.x + a_Mass;
      float forceY = u_Force.y + c_G.y + a_Mass;

      float aX = forceX / a_Mass;
      float aY = forceY / a_Mass;

      float x = a_Velocity.x * t + 0.5 * aX * tt;            
      float y = a_Velocity.y * t + 0.5 * aY * tt; 

      newPosition.xy += vec2(x,y);   

      newAlpha = 1 - t/lifeTime;        // 1 ~ 0
   }
   else
   {
      newPosition.xy = vec2(-100000, 0);   
   }

   gl_Position = newPosition;
   
   vec4 newColor = a_Color;
   v_Color = vec4(a_Color.rgb, newAlpha);
}
