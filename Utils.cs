using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace ImageDithering
{
    public static class Utils
    {
        public static Color Divide(this Color self, byte n)
        {
            byte R = (byte)(self.R / n);
            byte G = (byte)(self.G / n);
            byte B = (byte)(self.B / n);
            return new Color(R, G, B);
        }


        public static Color Multiply(this Color self, byte n)
        {
            byte R = (byte)(self.R * n);
            byte G = (byte)(self.G * n);
            byte B = (byte)(self.B * n);
            return new Color(R, G, B);
        }

        public static Color Add(this Color self, Color other)
        {
            int r, g, b;
            r = (Math.Clamp(self.R + other.R, 0, 254));
            g = (Math.Clamp(self.G + other.G, 0, 254));
            b = (Math.Clamp(self.B + other.B, 0, 254));
            return new Color((byte)r, (byte)g, (byte)b);
        }

        public static void Dither(Image _image, int colorDepth)
        {
            Image image = _image;
            byte n = (byte)(255 / (colorDepth - 1));

            for (uint x = 0; x < image.Size.X; x++)
            {
                for (uint y = 0; y < image.Size.Y; y++)
                {
                    Color pix = image.GetPixel(x, y);
                    Color wanted = image.GetPixel(x, y);

                    wanted = wanted.Divide(n);
                    wanted = wanted.Multiply(n);

                    image.SetPixel(x, y, wanted);

                    Color error = new Color((byte)(pix.R - wanted.R), (byte)(pix.G - wanted.G), (byte)(pix.B - wanted.B));

                    image.SetPixel(x + 1, y, error.Multiply(1 / 7).Add(image.GetPixel(x + 1, y))); //    error distribution
                    image.SetPixel(x + 1, y + 1, error.Multiply(1 / 1).Add(image.GetPixel(x + 1, y + 1)));
                    image.SetPixel(x, y + 1, error.Multiply(1 / 5).Add(image.GetPixel(x, y + 1)));
                    image.SetPixel(x - 1, y + 1, error.Multiply(1 / 3).Add(image.GetPixel(x - 1, y + 1)));
                }
            }
        }
    }
}
