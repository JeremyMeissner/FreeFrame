#version 330

layout(location = 0) out vec4 color;

uniform vec2 u_Resolution;
uniform vec4 u_Color;
uniform vec2 u_Position;
uniform vec2 u_Size;
uniform float u_Radius;

float roundedBox(vec2 centerPosition, vec2 size, float rad) {
    return length(max(abs(centerPosition) - size + rad, 0.0)) - rad;
}

void main() {
    // How soft the edges should be (in pixels). Higher values could be used to simulate a drop shadow.
    float edgeSoftness = 0.5f;
    
    // The radius of the corners (in pixels).
    float radius = clamp(u_Radius, 0.0f, min(u_Size.x, u_Size.y) / 2.0f);
    
    vec2 position = vec2(u_Position.x, u_Resolution.y - u_Position.y);

    // Calculate distance to edge.   
    float distance = roundedBox(gl_FragCoord.xy - position + vec2(-u_Size.x/2.0f, u_Size.y/2.0f), u_Size/2.0f, radius);
    
    // Smooth the result (free antialiasing).
    float smoothedAlpha = 1.0f - smoothstep(0.0f, 1.0f, distance);
    
    // Return the resultant shape.
    vec4 quadColor = vec4(u_Color.xyz, smoothedAlpha * u_Color.w);
    
    color = quadColor;
}