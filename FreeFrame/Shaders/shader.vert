#version 330 core

layout(location = 0) in vec2 position;

uniform mat4 u_Model_To_NDC;
uniform mat4 u_Transformation;
uniform mat4 u_View;
uniform mat4 u_Projection;

uniform vec2 u_Resolution;
uniform vec2 u_Position;
uniform vec2 u_Size;


void main()
{
	vec4 finalPosition = vec4(position,1.0, 1.0);

	finalPosition -= vec4(u_Position.x + u_Size.x/2.0, u_Position.y + u_Size.y/2.0, 0.0, 0.0);

	finalPosition = finalPosition * u_Transformation;

	finalPosition += vec4(u_Position.x + u_Size.x/2.0, u_Position.y + u_Size.y/2.0, 0.0, 0.0);

	finalPosition = u_Model_To_NDC * finalPosition;

	gl_Position = finalPosition; // * u_View * u_Projection);
}