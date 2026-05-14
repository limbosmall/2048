shader_type canvas_item;

uniform vec4 outline_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float outline_size : hint_range(0.0, 10.0) = 0.5;

void fragment() {
    vec4 col = texture(TEXTURE, UV);
    
    if (col.a < 0.1) {
        float alpha = 0.0;
        for (float x = -outline_size; x <= outline_size; x++) {
		    for (float y = -outline_size; y <= outline_size; y++) {
		        if (length(vec2(x, y)) <= outline_size) { // ← только круг
		            vec2 offset = vec2(x, y) * TEXTURE_PIXEL_SIZE;
		            alpha = max(alpha, texture(TEXTURE, UV + offset).a);
		        }
		    }
		}
        if (col.a < 0.5) {        // ← было 0.1
    		COLOR = alpha > 0.5 ? outline_color : col;  // ← было 0.1
			}
    } else {
        COLOR = col;
    }
}