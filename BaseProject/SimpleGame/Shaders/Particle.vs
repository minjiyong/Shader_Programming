#version 330

in vec3 a_Position;
in float a_Value;
in vec4 a_Color;
in float a_STime;
in vec3 a_Velocity;
in float a_LifeTime;
in float a_Mass;
in float a_Period;

out vec4 v_Color;

uniform float u_Time;
uniform vec3 u_Force;

const float c_PI = 3.141592;
const vec2 c_G = vec2(0, -9.8); // 0923

void raining()
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

void sinParticle()
{
   vec4 newPosition = vec4(a_Position, 1);
   
   float newTime = u_Time - a_STime;
   float lifeTime = a_LifeTime;
   float t = fract(newTime/lifeTime) * lifeTime;
   float tt = t * t;

   vec4 centerC = vec4(1, 0, 0, 1);
   vec4 borderC = vec4(1, 1, 1, 0);
   vec4 newColor = a_Color;
   float newAlpha = 1.0;

   if(newTime > 0)
   {
      float period = a_Period * 3.f;

      float x = 2 * t - 1;
      float y = sin(2 * t * c_PI * period) * (a_Value - 0.5) * 2.f * t;
      y *= sin(fract(newTime/lifeTime) * c_PI);

      newPosition.xy += vec2(x,y);

      newAlpha = 1.0 - t/lifeTime;

      // if (-0.05f < y && y < 0.05f) newColor.r = 255;         // ³»°¡ Â§°Å
      newColor = mix(centerC, borderC, abs(y * 4));
   }
   else
   {
      newPosition.xy = vec2(-100000, 0);   
   }

   gl_Position = newPosition;

   v_Color = vec4(newColor.rgb, newAlpha);
}

void circleParticle()
{
   vec4 newPosition = vec4(a_Position, 1);
   
   float newTime = u_Time - a_STime;
   float lifeTime = a_LifeTime;
   float t = fract(newTime/lifeTime) * lifeTime;
   float tt = t * t;

   vec4 centerC = vec4(1, 0, 0, 1);
   vec4 borderC = vec4(1, 1, 1, 0);
   vec4 newColor = a_Color;
   float newAlpha = 1.0;

   if(newTime > 0)
   {
      float x = cos(a_Value * 2 * c_PI);
      float y = sin(a_Value * 2 * c_PI);
      
      float newX = x + 0.5 * c_G.x * tt;
      float newY = y + 0.5 * c_G.y * tt;

      newPosition.xy += vec2(newX, newY);

      newAlpha = 1.0 - t/lifeTime;          // 1~0
   }
   else
   {
      newPosition.xy = vec2(-100000, 0);   
   }

   gl_Position = newPosition;

   v_Color = vec4(newColor.rgb, newAlpha);
}

void main()
{
   // raining();
   // sinParticle();
   circleParticle();
}
