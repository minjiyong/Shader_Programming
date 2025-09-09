#version 330

layout(location=0) out vec4 FragColor;

in vec4 v_Color;

uniform vec4 u_Color;

void main()
{
	//FragColor = vec4(u_Color.r, u_Color.g, u_Color.b, u_Color.a);
	if(v_Color.b < 0.5f)
		FragColor = v_Color;
	else
		discard;
}
