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


        public static Color[] Quantize(Image img, int colorNum)
        {
            Random random = new Random();
            Color[] means = new Color[colorNum];
            Color color = new Color(0, 0, 0), mean, t;
            int n;

            for (int i = 0; i < colorNum; i++)
                means[i] = new Color((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));

            //  k-means clustering
            //  by this tutorial https://docs.microsoft.com/ru-ru/archive/msdn-magazine/2013/february/data-clustering-detecting-abnormal-data-using-k-means-clustering

            for (int i = 0; i < 8; i++)
            {
                for(int j = 0; j < means.Length; j++)
                {
                    Console.WriteLine(j);
                    mean = means[j];
                    t = new Color(0, 0, 0);
                    n = 0;

                    for (int k = 3; k < img.Pixels.Length; k += 300)
                    {
                        color = new Color(img.Pixels[k], img.Pixels[k - 1], img.Pixels[k - 2]);

                        if (GetNearest(color, means, 20) == mean)
                        {
                            t.Add(color);
                            n++;
                        }
                    }

                    if(n != 0)
                        mean = t.Divide((byte)n);
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
