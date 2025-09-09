#version 330

in vec3 a_Position;
in vec4 a_Color;

out vec4 v_Color;

uniform vec4 u_Trans;

void main()
{
	vec4 newPosition;
	newPosition.xy = a_Position.xy*u_Trans.w + u_Trans.xy;
	newPosition.z = 0;
	newPosition.w= 1;
	gl_Position = newPosition;

	v_Color = a_Color;
}
