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
            return new Color((byte)Math.Clamp(self.R + other.R, 0, 255), (byte)Math.Clamp(self.G + other.G, 0, 255), (byte)Math.Clamp(self.B + other.B, 0, 255));
        }


        public static void Dither(Image _image, int colorDepth, bool clustering = false)
        {
            Image image = _image;
            Color[] colors;
            if (clustering)
                colors = Quantize(image, colorDepth);
            else
                colors = QuantizeMedian(image, colorDepth);

            for (uint x = 0; x < image.Size.X; x++)
            {
                for (uint y = 0; y < image.Size.Y; y++)
                {
                    Color pix = image.GetPixel(x, y);

                    Color wanted = GetNearest(pix, colors, 100000000);
                    

                    image.SetPixel(x, y, wanted);

                    Color error = new Color((byte)Math.Clamp(pix.R - wanted.R, 0, 255), (byte)Math.Clamp(pix.G - wanted.G, 0, 255), (byte)Math.Clamp(pix.B - wanted.B, 0, 255));

                    image.SetPixel(x + 1, y, error.Multiply(1 / 7).Add(image.GetPixel(x + 1, y)));      //  error distribution
                    image.SetPixel(x + 1, y + 1, error.Multiply(1 / 1).Add(image.GetPixel(x + 1, y + 1)));
                    image.SetPixel(x, y + 1, error.Multiply(1 / 5).Add(image.GetPixel(x, y + 1)));
                    image.SetPixel(x - 1, y + 1, error.Multiply(1 / 3).Add(image.GetPixel(x - 1, y + 1)));
                }
            }
        }

        /// <summary>
        /// Quatization by median cut
        /// </summary>
        /// <param name="img">Source image</param>
        /// <param name="colorNum">Number of colors to return; Must be a power of two</param>
        /// <returns>Array of Color[colorNum]</returns>
        public static Color[] QuantizeMedian(Image img, int colorNum)  // unfinished
        {
            Color[][] oldColors = new Color[colorNum][];
            Color[][] newColors = new Color[colorNum][];
            Color[][] t = new Color[colorNum][];
            oldColors[0] = new Color[img.Pixels.Length / 3];

            //  Temp variables
            int skip = 300;
            int arraySize = (img.Pixels.Length / 3) / skip;
            int filledRows = 1;
            //  Temp variables

            for (int i = 0; i < colorNum; i++)  // initialize arrays
            {
                newColors[i] = new Color[arraySize];
                oldColors[i] = new Color[arraySize];
            }

            for (int i = 0; i < arraySize; i++)  // set first array of oldColors to img pixels, with interval of skip
                oldColors[0][i] = new Color(img.Pixels[skip * i], img.Pixels[skip * i + 1], img.Pixels[skip * i + 2]);

            while (filledRows < colorNum)  // while not all colors are done
            {
                for (int j = 0; j < filledRows; j++)
                {
                    t = QuantizeMedianSplit(oldColors[j]);  // split each filled row
                    newColors[j * 2] = t[0];
                    newColors[j * 2 + 1] = t[1];  // assign them to newColors
                }

                filledRows *= 2;

                for (int y = 0; y < filledRows; y++)
                {
                    oldColors[y] = (Color[])newColors[y].Clone();  // copy newColors to oldColors
                    newColors[y] = new Color[arraySize];
                }

                Console.WriteLine(filledRows);

            }

            Color[] ret = new Color[colorNum];  // colors to return
            Vector3f sum = new Vector3f(0, 0, 0);

            for (int i = 0; i < colorNum; i++)  // calculate mean color of each array and return them
            {
                int n = 0;
                foreach (Color c in oldColors[i])
                {
                    sum.X += c.R;
                    sum.Y += c.G;
                    sum.Z += c.B;
                    n++;
                }

                sum /= n;
                ret[i] = new Color((byte)sum.X, (byte)sum.Y, (byte)sum.Z);
            }

            return ret;
        }


        static Color[][] QuantizeMedianSplit(Color[] colors)
        {
            Color[][] ret = new Color[2][];
            ret[0] = new Color[colors.Length/2];
            ret[1] = new Color[colors.Length / 2];
            int r = 0, g = 0, b = 0;

            foreach (Color c in colors)
            {
                r += c.R;
                g += c.G;
                b += c.B;
            }

            if (r > g && r > b)
            {
                colors = colors.OrderBy(order => order.R).ToArray();
                ret[0] = colors.Take(colors.Length / 2).ToArray();
                ret[1] = colors.Skip(colors.Length / 2).ToArray();
            }
            else if (g > r && g > b)
            {
                colors = colors.OrderBy(order => order.G).ToArray();
                ret[0] = colors.Take(colors.Length / 2).ToArray();
                ret[1] = colors.Skip(colors.Length / 2).ToArray();
            }
            else if (b > r && b > g)
            {
                colors = colors.OrderBy(order => order.B).ToArray();
                ret[0] = colors.Take(colors.Length / 2).ToArray();
                ret[1] = colors.Skip(colors.Length / 2).ToArray();
            }

            return ret;
        }


        /// <summary>
        /// Color quantization by clustering (very slow)
        /// </summary>
        /// <param name="img">Sourse image to take colors out</param>
        /// <param name="colorNum">Number of colors to return</param>
        /// <returns>Color[colorNum]</returns>
        public static Color[] Quantize(Image img, int colorNum)
        {
            Random random = new Random();
            Color[] means = new Color[colorNum];
            Color color;
            Vector3f sum = new Vector3f(0, 0, 0);
            int n, j;

            for (int i = 0; i < colorNum; i++)
                means[i] = new Color((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));

            for (int i = 0; i < 10; i++)
            {
                j = 0;

                foreach(Color mean in means)
                {
                    Console.WriteLine(j);
                    sum *= 0;
                    n = 0;

                    for (int k = 3; k < img.Pixels.Length; k += 300)
                    {
                        color = new Color(img.Pixels[k], img.Pixels[k - 1], img.Pixels[k - 2]);
                        if (GetNearest(color, means, 250) == mean)
                        {
                            sum.X += color.R;
                            sum.Y += color.G;
                            sum.Z += color.B;
                            n++;
                        }
                    }

                    if (n != 0)
                    {
                        sum /= n;
                        means[j] = new Color((byte)sum.X, (byte)sum.Y, (byte)sum.Z);
                    }
                    j++;
                }
            }

            return means;
        }


        static float DistanceTo(this Color self, Color other)  // to get proper distance you need sqare root of result; not using for optimisation
        {
            return (self.R - other.R) * (self.R - other.R) + (self.G - other.G) * (self.G - other.G) + (self.B - other.B) * (self.B - other.B);
        }

        /// <summary>
        /// Searchs nearest but not farther than maxDist color to color in search array
        /// </summary>
        /// <param name="color">Base color</param>
        /// <param name="search">Array for searching in</param>
        /// <param name="maxDist">Maximum distance of nearest color</param>
        /// <returns></returns>
        static Color GetNearest(Color color, Color[] search, int maxDist)
        {
            float dist = -1, tDist = 0;
            Color ret = color;

            foreach (Color c in search)
            {
                tDist = color.DistanceTo(c);

                if (tDist < maxDist && (dist == -1 || tDist < dist))
                {
                    dist = tDist;
                    ret = c;
                }
            }

            return ret;
        }
    }
}
