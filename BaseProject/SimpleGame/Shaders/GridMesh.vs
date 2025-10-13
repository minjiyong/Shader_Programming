#version 330

in vec3 a_Position;

out vec4 v_Color;

uniform float u_Time;

const float c_Pi = 3.141592;

void main()
{
	// a_Position.x -0.5 ~ 0.5
	vec4 newPosition = vec4(a_Position, 1);

	float value = a_Position.x + 0.5f;	// 0 ~ 1

	newPosition.y *= (1-value);

	float dX = 0;
	float dY = 0.5f * sin(2.f*value*c_Pi  + u_Time*10) * value;

	float newColor = (sin(2.f*value*c_Pi  + u_Time*10) + 1) / 2;

	newPosition += vec4(dX, dY, 0, 0);

	gl_Position = newPosition;

	v_Color = vec4(newColor);
}
