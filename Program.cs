using SFML.System;
using SFML.Graphics;

Image img = new Image("");  // add path

//                Floyd–Steinberg dithering

Image Dither(Image _image, int colorDepth)
{
    Image image = _image;
    byte n = (byte)(255 / (colorDepth - 1));

    for (uint x = 0; x < image.Size.X; x++)
    {
        for (uint y = 0; y < image.Size.Y; y++)
        {
            Color pix = image.GetPixel(x, y);
            Color wanted = image.GetPixel(x, y);

            wanted.R /= n;
            wanted.R *= n;
            wanted.G /= n;
            wanted.G *= n;
            wanted.B /= n;
            wanted.B *= n;

            image.SetPixel(x, y, wanted);

            Color error = new Color((byte)(pix.R - wanted.R), (byte)(pix.G - wanted.G), (byte)(pix.B - wanted.B));
        }
    }

    return image;
}
