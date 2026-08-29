#version 330 core

in vec2 texCoord;

out vec4 outputColor;

uniform sampler2D texture0;
uniform vec4 lineColor;

void main()
{
    outputColor = texture(texture0, texCoord) * lineColor;
}