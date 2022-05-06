using SFML.Graphics;
using SFML.System;
using SFML.Window;
using ImageDithering;

//                Floyd–Steinberg dithering
//                      X  7
//                   3  5  1

Image img = new Image("img.png");
Utils.Dither(img, 32);

VideoMode vm = new VideoMode(800, 800);
RenderWindow rw = new RenderWindow(vm, "Labirinth", Styles.Close, new ContextSettings(32, 32, 8));
Texture t = new Texture(img);
t.Smooth = false;
Sprite s = new Sprite(t);


rw.Closed += OnClose;



while (rw.IsOpen)
{
    rw.DispatchEvents();
    rw.Clear();
    rw.Draw(s);
    rw.Display();
}



static void OnClose(object sender, EventArgs e)
{
    (sender as RenderWindow)?.Close();
}
