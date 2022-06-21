#version 330

layout(location = 0) out vec4 color;

uniform vec2 u_Resolution;
uniform vec4 u_Color;
uniform vec2 u_Position;
uniform vec2 u_Size;
uniform float u_Radius;

// Draw a circle at a given position with radius and color
vec4 circle(vec2 uv, vec2 pos, float rad, vec4 color) {
	float d = length(pos - uv) - rad;
	float t = clamp(d, 0.0, 1.0);
	return vec4(color.xyz, (1.0 - t) * color.w);
}

void main() {

	vec2 uv = gl_FragCoord.xy;

	float radius = u_Size.x / 2.0;

	vec2 position = vec2(u_Position.x + u_Size.x / 2.0, u_Resolution.y - (u_Position.y + u_Size.y / 2.0));  // Invert y axis

	color = circle(uv, position, radius, u_Color);
}