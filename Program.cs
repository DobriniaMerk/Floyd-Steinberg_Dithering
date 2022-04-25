using SFML.Graphics;
using SFML.System;
using SFML.Window;
using ImageDithering;

//                Floyd–Steinberg dithering
//                      X  7
//                   3  5  1

Image img = new Image("img.png");  // add path
Utils.Dither(img, 2);

VideoMode vm = new VideoMode(800, 800);
RenderWindow rw = new RenderWindow(vm, "Labirinth", Styles.Close, new ContextSettings(32, 32, 8));
Texture t = new Texture(img);
t.Smooth = false;
Sprite s = new Sprite(t);

/*Color[] colors = Utils.QuantizeMedian(img, 8);
RectangleShape[] rects = new RectangleShape[colors.Length];

for (int i = 0; i < colors.Length; i++)
{
    rects[i] = new RectangleShape(new Vector2f(50, 50));
    rects[i].FillColor = colors[i];
    rects[i].Position = new Vector2f(0, i * 50);
}*/


rw.Closed += OnClose;



while (rw.IsOpen)
{
    rw.DispatchEvents();
    rw.Clear();
    rw.Draw(s);

    /*foreach (RectangleShape r in rects)
        rw.Draw(r);*/

    rw.Display();
}



static void OnClose(object sender, EventArgs e)
{
    (sender as RenderWindow)?.Close();
}
