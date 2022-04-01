using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace ImageDithering
{
    public static class Utils
    {
        public static Color Divide(this Color self, byte n)
        {
            return new Color((byte)(self.R / n), (byte)(self.G / n), (byte)(self.B / n));
        }


        public static Color Multiply(this Color self, byte n)
        {
            return new Color((byte)(self.R * n), (byte)(self.G * n), (byte)(self.B * n));
        }

        public static Color Add(this Color self, Color other)
        {
            int r, g, b;
            r = Math.Clamp(self.R + other.R, 0, 255);
            g = Math.Clamp(self.G + other.G, 0, 255);
            b = Math.Clamp(self.B + other.B, 0, 255);
            return new Color((byte)r, (byte)g, (byte)b);
        }


        public static void Dither(Image _image, int colorDepth)
        {
            Image image = _image;
            Color[] colors = Quantize(image, colorDepth);

            for (uint x = 0; x < image.Size.X; x++)
            {
                for (uint y = 0; y < image.Size.Y; y++)
                {
                    Color pix = image.GetPixel(x, y);
                    Color wanted = GetNearest(pix, colors, 1000);

                    image.SetPixel(x, y, wanted);

                    Color error = new Color((byte)(pix.R - wanted.R), (byte)(pix.G - wanted.G), (byte)(pix.B - wanted.B));

                    image.SetPixel(x + 1, y, error.Multiply(1 / 7).Add(image.GetPixel(x + 1, y))); //    error distribution
                    image.SetPixel(x + 1, y + 1, error.Multiply(1 / 1).Add(image.GetPixel(x + 1, y + 1)));
                    image.SetPixel(x, y + 1, error.Multiply(1 / 5).Add(image.GetPixel(x, y + 1)));
                    image.SetPixel(x - 1, y + 1, error.Multiply(1 / 3).Add(image.GetPixel(x - 1, y + 1)));
                }
            }
        }


        static Color[] QuantizeMedian(Image img, int colorNum)  // https://en.wikipedia.org/wiki/Median_cut another variant
        {
            Color[] colors = new Color[colorNum];
            Color sum = new Color(0, 0, 0);

            return null;
        }



        static Color[] Quantize(Image img, int colorNum)
        {
            Random random = new Random();
            Color[] means = new Color[colorNum];
            Color color = new Color(0, 0, 0), t = new Color(0, 0, 0);
            int n, j = 0;

            for (int i = 0; i < colorNum; i++)
                means[i] = new Color((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));

            for (int i = 0; i < 16; i++)
            {
                j = 0;

                foreach(Color mean in means)
                {
                    Console.WriteLine(j);
                    t.Multiply(0);
                    n = 0;

                    for (int k = 3; k < img.Pixels.Length; k += 300)
                    {
                        if (GetNearest(new Color(img.Pixels[k], img.Pixels[k - 1], img.Pixels[k - 2]), means, 15) == mean)
                        {
                            t.Add(new Color(img.Pixels[k], img.Pixels[k - 1], img.Pixels[k - 2]));
                            n++;
                        }
                    }

                    if(n != 0)
                        means[j] = t.Multiply((byte)(1/n));
                    j++;
                }
            }

            return means;
        }


        static float DistanceTo(this Color self, Color other)
        {
            return MathF.Sqrt(MathF.Pow(self.R - other.R, 2) + MathF.Pow(self.G - other.G, 2) + MathF.Pow(self.B - other.B, 2));
        }

        static Color GetNearest(Color color, Color[] search, int maxDist)
        {
            float dist = -1;
            Color ret = color;
            float tDist = 0;

            foreach (Color c in search)
            {
                tDist = color.DistanceTo(c);
                if ((dist == -1 || tDist < dist) && tDist < maxDist)
                {
                    dist = tDist;
                    ret = c;
                }
            }

            return ret;
        }
    }
}
