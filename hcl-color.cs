// Name: HCL/L*a*b* color
// Submenu:
// Author: Michael Schmidt
// Title: HCL/L*a*b* color
// Version: 0.1
// Desc: A plugin for adjusting the brightness, saturation, and hue of an image in HCL and L*a*b* color space.
// Keywords: LCh, HCL, Lab, brightness, saturation, hue
// URL: https://github.com/RunDevelopment/hcl-color-adjust-plugin
// Help: https://github.com/RunDevelopment/hcl-color-adjust-plugin
#region UICode
IntSliderControl HAdd = 0; // [-180,180,2] Hue
DoubleSliderControl CAdd = 0; // [-1,1,3] Chroma Add
DoubleSliderControl CMult = 1; // [0,2,3] Chroma Multiply (Log scale)
DoubleSliderControl CExp = 1; // [0,2] Chroma Exponent
DoubleSliderControl LAdd = 0; // [-10,10,5] Luminance Add
DoubleSliderControl LMult = 1; // [0,2,5] Luminance Multiply (Log scale)
DoubleSliderControl LExp = 1; // [0,2] Luminance Exponent (Contrast)
DoubleSliderControl AAdd = 0; // [-1,1,7] a* (log scale)
DoubleSliderControl BAdd = 0; // [-1,1,6] b* (log scale)
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
	// Delete any of these lines you don't need
	Rectangle selection = EnvironmentParameters.SelectionBounds;
	int centerX = ((selection.Right - selection.Left) / 2) + selection.Left;
	int centerY = ((selection.Bottom - selection.Top) / 2) + selection.Top;
	ColorBgra primaryColor = EnvironmentParameters.PrimaryColor;
	ColorBgra secondaryColor = EnvironmentParameters.SecondaryColor;
	int brushWidth = (int)EnvironmentParameters.BrushWidth;

	var PI2 = Math.PI * 2;

	static double TransformMult(double x)
	{
		double factor = 3;
		var add = Math.Min(0, (x - 1) * Math.Pow(2, -factor));
		return add + Math.Pow(2, (x - 1) * factor);
	}
	var cMult = TransformMult(CMult);
	var lMult = TransformMult(LMult);

	var hAdd = HAdd * PI2 / 360;

	double TransformAB(double x) => Math.Sign(x) * Math.Pow(Math.Abs(x), 3) * 100;
	var aAdd = TransformAB(AAdd);
	var bAdd = TransformAB(BAdd);

	ColorBgra currentPixel;
	for (int y = rect.Top; y < rect.Bottom; y++)
	{
		if (IsCancelRequested) return;
		for (int x = rect.Left; x < rect.Right; x++)
		{
			currentPixel = src[x, y];

			var lch = ColorLCh.FromSRgb(currentPixel);
			lch.L = Math.Max(0, Math.Pow((lch.L + LAdd) * lMult, LExp));
			lch.C = Math.Max(0, Math.Pow((lch.C + CAdd) * cMult, CExp));
			lch.H = (lch.H + hAdd + PI2) % PI2;

			var lab = lch.ToLab();
			lab.A -= aAdd;
			lab.B += bAdd;

			dst[x, y] = lab.ToSRgb(currentPixel.A);
		}
	}
}

struct ColorXYZ
{
	public double X;
	public double Y;
	public double Z;

	public ColorXYZ(double x, double y, double z) => (X, Y, Z) = (x, y, z);

	public static ColorXYZ FromSRgb(ColorBgra color)
	{
		return FromLinearRgb(
			SrgbUtility.ToLinear(color.R),
			SrgbUtility.ToLinear(color.G),
			SrgbUtility.ToLinear(color.B)
		);
	}
	public static ColorXYZ FromLinearRgb(double r, double g, double b)
	{
		return new ColorXYZ(
			0.4124 * r + 0.3576 * g + 0.1805 * b,
			0.2126 * r + 0.7152 * g + 0.0722 * b,
			0.0193 * r + 0.1192 * g + 0.9505 * b
		);
	}

	public ColorBgra ToSRgb(byte a)
	{
		var r = 3.2406 * X - 1.5372 * Y - 0.4986 * Z;
		var g = -.9689 * X + 1.8758 * Y + 0.0415 * Z;
		var b = 0.0557 * X - 0.2040 * Y + 1.0570 * Z;

		var color = ColorBgra.FromBgraClamped(
			(float)SrgbUtility.ToSrgbClamped(b) * 255,
			(float)SrgbUtility.ToSrgbClamped(g) * 255,
			(float)SrgbUtility.ToSrgbClamped(r) * 255,
			1
		);
		color.A = a;
		return color;
	}
}

struct ColorLab
{
	public double L;
	public double A;
	public double B;

	private const double Xn = 95.0489;
	private const double Yn = 100;
	private const double Zn = 108.884;

	public ColorLab(double l, double a, double b) => (L, A, B) = (l, a, b);

	public static ColorLab FromXYZ(ColorXYZ color)
	{
		return new ColorLab(
			116 * F(color.Y / Yn) - 16,
			500 * (F(color.X / Xn) - F(color.Y / Yn)),
			200 * (F(color.Y / Yn) - F(color.Z / Zn))
		);
	}
	public static ColorLab FromSRgb(ColorBgra color) => FromXYZ(ColorXYZ.FromSRgb(color));

	private static double F(double t)
	{
		if (t > 0.008856451679035631)
			return Math.Cbrt(t);
		else
			return t * 7.787037037037036 + 0.13793103448275862;
	}
	private static double FInv(double t)
	{
		if (t > 0.20689655172413793)
			return t * t * t;
		else
			return (t - 0.13793103448275862) * 0.12841854934601665;
	}

	public ColorXYZ ToXYZ()
	{
		return new ColorXYZ(
			Xn * FInv((L + 16) / 116 + A / 500),
			Yn * FInv((L + 16) / 116),
			Zn * FInv((L + 16) / 116 - B / 200)
		);
	}
	public ColorBgra ToSRgb(byte a) => ToXYZ().ToSRgb(a);
}


struct ColorLCh
{
	public double L;
	public double C;
	public double H;

	public ColorLCh(double l, double c, double h) => (L, C, H) = (l, c, h.IsFinite() ? h : 0);

	public static ColorLCh FromLab(ColorLab color)
	{
		return new ColorLCh(
			color.L,
			Math.Sqrt(color.A * color.A + color.B * color.B),
			(color.A > 0 ? 0 : Math.PI) + Math.Atan(color.B / color.A)
		);
	}
	public static ColorLCh FromSRgb(ColorBgra color) => FromLab(ColorLab.FromSRgb(color));

	public ColorLab ToLab()
	{
		return new ColorLab(
			L,
			C * Math.Cos(H),
			C * Math.Sin(H)
		);
	}
	public ColorBgra ToSRgb(byte a) => ToLab().ToSRgb(a);
}
