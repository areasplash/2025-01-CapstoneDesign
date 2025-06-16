using UnityEngine;



public static class ColorExtensions {

	// Methods

	public static Color HSVtoRGB(float h, float s, float v, float a = 1f) {
		float c = v * s;
		float p = h % 360f * 0.0166667f;
		float x = c * (1f - Mathf.Abs(p % 2f - 1f));
		float m = v - c;
		Color color = new(0f, 0f, 0f, a);
		switch (p) {
			case < 1f: color.r = c; color.g = x; color.b = 0; break;
			case < 2f: color.r = x; color.g = c; color.b = 0; break;
			case < 3f: color.r = 0; color.g = c; color.b = x; break;
			case < 4f: color.r = 0; color.g = x; color.b = c; break;
			case < 5f: color.r = x; color.g = 0; color.b = c; break;
			default: color.r = c; color.g = 0; color.b = x; break;
		}
		color.r += m;
		color.g += m;
		color.b += m;
		return color;
	}

	public static (float, float, float, float) RGBtoHSV(Color color) {
		float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
		float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
		float delta = max - min;
		float h = 0f;
		float s = 0f;
		float v = max;
		float a = color.a;
		if (max != 0f) s = delta / max;
		if (delta != 0f) {
			if (color.r == max) h = (color.g - color.b) / delta + 0f;
			if (color.g == max) h = (color.b - color.r) / delta + 2f;
			if (color.b == max) h = (color.r - color.g) / delta + 4f;
			h *= 60f;
			if (h < 0f) h += 360f;
		}
		return (h, s, v, a);
	}
}
