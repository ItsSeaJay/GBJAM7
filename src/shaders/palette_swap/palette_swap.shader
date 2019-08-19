
shader_type canvas_item;

uniform sampler2D palette;

void fragment()
{
	// Obtain the colour of the current screen fragment
	vec4 screen_fragment = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0);
	// Find out how far along the texture we should move
	// based on how red the current fragment is
	float x = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0).r;
	// Obtain the width of the palette
	int width = textureSize(palette, 0).x;
	// Posterise the fragment colour based on how many colours are in the texture
	x = round(x * float(width)) / float(width);
	
	// Set the colour of the fragment to the corresponding palette colour
	COLOR.rgb = texture(palette, vec2(x, 0.0)).rgb;
}