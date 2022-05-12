#version 330 core

layout(location = 0) in vec2 position;

uniform mat4 u_Model_To_NDC;
uniform mat4 u_Transformation;
uniform mat4 u_View;
uniform mat4 u_Projection;


void main()
{
	gl_Position = u_Transformation * (u_Model_To_NDC * vec4(position,1.0, 1.0)); // * u_View * u_Projection);
}