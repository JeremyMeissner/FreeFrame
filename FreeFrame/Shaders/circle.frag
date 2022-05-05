#version 330

layout(location = 0) out vec4 color;

uniform vec2 u_Resolution;
uniform vec4 u_Color;
uniform vec2 u_Position;
uniform float u_Radius;

// Draw a circle at position with radius and color
vec4 circle(vec2 uv, vec2 pos, float rad, vec4 color) {
	float d = length(pos - uv) - rad;
	float t = clamp(d, 0.0, 1.0);
	return vec4(color.xyz, (1.0 - t) * color.w);
}

void main() {

	vec2 uv = gl_FragCoord.xy;

	vec2 position = vec2(u_Position.x, u_Resolution.y - u_Position.y); // Invert y axis

	// Circle
	vec4 layer2 = circle(uv, position, u_Radius, u_Color);
	
	color = layer2;
}