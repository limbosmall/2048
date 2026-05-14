shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform float blur_amount : hint_range(0.0, 10.0) = 0.1;
uniform vec4 tint_color : source_color = vec4(0.4, 0.4, 0.4, 0.4);

void fragment() {
    vec2 uv = SCREEN_UV;
    vec4 col = vec4(0.0);
    float total = 0.0;

    // Простой box blur
	for (float x = -2.0; x <= 2.0; x++) {
	    for (float y = -2.0; y <= 2.0; y++) {
	        vec2 offset = vec2(x, y) * SCREEN_PIXEL_SIZE * blur_amount;
	        col += texture(screen_texture, uv + offset);
	        total += 1.0;
	    }
	}

    col /= total;
    COLOR = mix(col, tint_color, tint_color.a);
    COLOR.a = 1.0;
}