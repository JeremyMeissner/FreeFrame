#version 330

layout(location = 0) out vec4 color;

uniform vec2 u_Resolution;
uniform vec4 u_Color;
uniform vec2 u_Position;
uniform float u_Radius;

vec3 rgb(float r, float g, float b) {
	return vec3(r / 255.0, g / 255.0, b / 255.0);
}

/**
 * Draw a circle at vec2 `pos` with radius `rad` and
 * color `color`.
 */
vec4 circle(vec2 uv, vec2 pos, float rad, vec4 color) {
	float d = length(pos - uv) - rad;
	float t = clamp(d, 0.0, 1.0);
	return vec4(color.xyz, (1.0 - t) * color.w);
}

void main() {

	vec2 uv = gl_FragCoord.xy;
//	vec2 center = u_Resolution.xy * 0.5;
//	float radius = 0.25 * u_Resolution.y;

    // Background layer
	
	vec2 position = vec2(u_Position.x, u_Resolution.y - u_Position.y); // Invert y axis

	// Circle
	vec4 layer2 = circle(uv, position, u_Radius, u_Color);
	
	// Blend the two
	color = layer2;
}