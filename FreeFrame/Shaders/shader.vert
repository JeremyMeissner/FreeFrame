﻿#version 330 core

layout(location = 0) in vec2 position;

uniform mat4 u_Model_To_NDC;
uniform mat4 u_Transformation;

uniform vec2 u_Resolution;
uniform vec2 u_Position;
uniform vec2 u_Size;


void main()
{
	vec4 finalPosition = vec4(position,1.0, 1.0);

	vec4 origin = vec4(u_Position.x + u_Size.x/2.0, u_Position.y + u_Size.y/2.0, 0.0, 0.0);

	finalPosition -= origin;

	finalPosition = finalPosition * u_Transformation;

	finalPosition += origin;

	finalPosition = u_Model_To_NDC * finalPosition;

	gl_Position = finalPosition;
}