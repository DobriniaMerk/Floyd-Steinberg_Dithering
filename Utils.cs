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
            Color[] colors = QuantizeMedian(image, colorDepth);

            for (uint x = 0; x < image.Size.X; x++)
            {
                for (uint y = 0; y < image.Size.Y; y++)
                {
                    Color pix = image.GetPixel(x, y);
                    Color wanted = GetNearest(pix, colors, 10000000);

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
            Color[][] oldColors = new Color[colorNum][];
            Color[][] newColors = new Color[colorNum][];
            Color[][] t = new Color[colorNum][];
            oldColors[0] = new Color[img.Pixels.Length / 3];

            for (int i = 0; i < colorNum; i++)
                newColors[i] = new Color[img.Pixels.Length / 3];

            for (int i = 2; i < img.Pixels.Length; i += 300)
                oldColors[0][i / 3] = new Color(img.Pixels[i - 2], img.Pixels[i - 1], img.Pixels[i]);

            for (int i = 1; i < colorNum; i *= 2)
            {
                for (int j = 0; j < i; j++)
                {
                    t = QuantizeMedianSplit(oldColors[j]);
                    newColors[j] = t[0];
                    newColors[j + 1] = t[1];
                }

                for (int y = 0; y < i; y++)
                    oldColors[y] = (Color[])newColors[y].Clone();

                Console.WriteLine(i);
            }

            Color[] ret = new Color[colorNum];

            for (int i = 0; i < colorNum; i++)
            {
                Color sum = new Color(0, 0, 0);
                int n = 0;
                foreach (Color c in oldColors[i])
                {
                    sum.Add(c);
                    n++;
                }
                sum.Divide((byte)n);
                ret[i] = sum;
            }

            return ret;
        }


        static Color[][] QuantizeMedianSplit(Color[] colors)
        {
            Color[][] ret = new Color[2][];
            ret[0] = new Color[colors.Length/2];
            ret[1] = new Color[colors.Length / 2];
            Color sum = new Color(0, 0, 0);

            foreach (Color c in colors)
            {
                sum.Add(c);

                if (sum.R > sum.G && sum.R > sum.B)
                {
                    colors = colors.OrderBy(order => order.R).ToArray();
                    ret[0] = colors.Take(colors.Length / 2).ToArray();
                    ret[1] = colors.Skip(colors.Length / 2).ToArray();
                }
                else if (sum.G > sum.R && sum.G > sum.B)
                {
                    colors = colors.OrderBy(order => order.G).ToArray();
                    ret[0] = colors.Take(colors.Length / 2).ToArray();
                    ret[1] = colors.Skip(colors.Length / 2).ToArray();
                }
                else if (sum.B > sum.R && sum.B > sum.G)
                {
                    colors = colors.OrderBy(order => order.B).ToArray();
                    ret[0] = colors.Take(colors.Length / 2).ToArray();
                    ret[1] = colors.Skip(colors.Length / 2).ToArray();
                }
            }

            return ret;
        }


        static Color[] Quantize(Image img, int colorNum)
        {
            Random random = new Random();
            Color[] means = new Color[colorNum];
            Color color = new Color(0, 0, 0), t = new Color(0, 0, 0);
            int n, j = 0;

            for (int i = 0; i < colorNum; i++)
                means[i] = new Color((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));

            for (int i = 0; i < 32; i++)
            {
                j = 0;

                foreach(Color mean in means)
                {
                    Console.WriteLine(j);
                    t.Multiply(0);
                    n = 0;

                    for (int k = 3; k < img.Pixels.Length; k += 300)
                    {
                        color = new Color(img.Pixels[k], img.Pixels[k - 1], img.Pixels[k - 2]);
                        if (GetNearest(color, means, 250) == mean)
                        {
                            t.Add(color);
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
            return (self.R - other.R) * (self.R - other.R) + (self.G - other.G) * (self.G - other.G) + (self.B - other.B) * (self.B - other.B);
        }


        static Color GetNearest(Color color, Color[] search, int maxDist)
        {
            float dist = -1, tDist = 0;
            Color ret = color;

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
