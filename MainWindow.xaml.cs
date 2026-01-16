using System;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Formats.Asn1;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SSaver.Views;

public sealed partial class MainWindow : Window
{
    string SSText = DateTime.Now.ToString("t");
    string SSText_NL = DateTime.Now.ToString("ddd") + " " + DateTime.Now.ToString("d");

    private Starfield _starfield;
    public MainWindow()
    {
        this.InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;

        _starfield = new Starfield();

        Canvas.Draw += Canvas_Draw;
        Canvas.CreateResources += Canvas_CreateResources;

        Canvas.PointerMoved += CloseOnInput;
        Canvas.PointerPressed += CloseOnInput;
        Canvas.KeyDown += CloseOnInput;

    }

    private CanvasBitmap? _image;

    private async Task LoadImageAsync(CanvasControl sender)
    {
        _image = await CanvasBitmap.LoadAsync(sender, "Assets/background.jpg");

        /// load rss feed here and parse headlines
        /// https://www.nu.nl/rss
        /// then display them one by one in the starfield
        

    }

    private void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
    {
        args.TrackAsyncAction(LoadImageAsync(sender).AsAsyncAction());

        _starfield.Initialize(sender);
    }

    private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        int pos = DateTime.Now.Minute % 2;

        var textFormat = new CanvasTextFormat()
        {
            FontSize = 100,
            FontFamily = "Segoe UI",
            HorizontalAlignment = CanvasHorizontalAlignment.Center,
            VerticalAlignment = CanvasVerticalAlignment.Bottom
        };

        if (_image != null)
        {
            args.DrawingSession.DrawImage(_image, 0, 0, new Windows.Foundation.Rect(0,0, (float)sender.ActualWidth, (float)sender.ActualHeight), opacity: 0.2f); // draw at X=100, Y=100 }
        }

        // update to current time\
        SSText = DateTime.Now.ToShortTimeString();

        _starfield.Update(sender);
        _starfield.Draw(sender, args.DrawingSession);

        args.DrawingSession.DrawText(SSText, (float)sender.ActualWidth/2, (float)sender.ActualHeight / 4 * (1+pos),
            Windows.UI.Color.FromArgb((byte)(100 - DateTime.Now.Second), 0xff, 0xff, 0xff),
            textFormat);
        args.DrawingSession.DrawText(SSText_NL, (float)sender.ActualWidth/2, (float)sender.ActualHeight / 4 * (2+pos),
            Windows.UI.Color.FromArgb((byte)(100 + DateTime.Now.Second), 0xff, 0xff, 0xff),
            textFormat);

        //Canvas.CenterPoint = new System.Numerics.Vector3((float)sender.ActualWidth/2, (float)sender.ActualHeight/2, 1);
        //Canvas.Rotation = (DateTime.Now.Millisecond/10)%360-50;
        
        Canvas.Invalidate(); // continuous animation
    }

    private void CloseOnInput(object sender, object e)
    {
        Close();
    }

    public class Starfield
    {
        private const int StarCount = 1000;
        private Vector3[] _stars = new Vector3[StarCount];
        private Random _rand = new Random();
        private float _speed = 0.7f;

        private float _width;
        private float _height;

        public void Initialize(CanvasControl canvas)
        {
            _width = (float)canvas.ActualWidth;
            _height = (float)canvas.ActualHeight;

            for (int i = 0; i < StarCount; i++)
                _stars[i] = CreateStar();
        }

        private Vector3 CreateStar()
        {
            return new Vector3(
                (float)(_rand.NextDouble() * _width  - _width / 2),
                (float)(_rand.NextDouble() * _height - _height / 2),
                (float)(_rand.NextDouble() * _width)
            );
        }

        public void Update(CanvasControl canvas)
        {
            counter++;
            if (counter == 1000) counter = 0;

            if (counter % 10 == 0)
            {
                if (_speed > 3f) _speed += 0.01f;
            }


            _width = (float)canvas.ActualWidth;
            _height = (float)canvas.ActualHeight;

            for (int i = 0; i < StarCount; i++)
            {
                var s = _stars[i];
                s.Z -= _speed;

                if (s.Z <= 1)
                    s = CreateStar();

                _stars[i] = s;
            }

        }

        // Convert System.Drawing.Color to Windows.UI.Color
        Windows.UI.Color ToWinUIColor(System.Drawing.Color c)
        {
            return Windows.UI.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public int counter { get; set; } = 0;

        public void Draw(CanvasControl sender, CanvasDrawingSession ds)
        {
            foreach (var s in _stars)
            {
                float k = 128f / s.Z;
                float x = s.X * k + _width / 2;
                float y = s.Y * k + _height / 2;

                if (x < 0 || x >= _width || y < 0 || y >= _height)
                    continue;

                float brightness = Math.Clamp(1f - (s.Z / _width), 0.1f, 1f);
                byte b = (byte)(brightness * 255);

                if (_speed < 1.5)
                    ds.FillCircle(x, y, 8f, Windows.UI.Color.FromArgb(10,b,b,b));
                
                ds.FillCircle(x, y, 1f, Windows.UI.Color.FromArgb((byte)(b+50), b, b, b));

            }
        }
    }
}



