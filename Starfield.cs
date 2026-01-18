using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace SSaver.Views;

public sealed partial class MainWindow
{
    public class Starfield
    {
        private const int StarCount = 1000;
        private Vector3[] _stars = new Vector3[StarCount];
        private Random _rand = new Random();
        private float _speed = 0.7f;

        private float _width;
        private float _height;

        public void Initialize(CanvasAnimatedControl canvas)
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

        public void Update(ICanvasAnimatedControl canvas)
        {
            counter++;
            if (counter == 1000) counter = 0;

            if (counter % 10 == 0)
            {
                if (_speed > 3f) _speed += 0.01f;
            }


            _width = (float)canvas.Size.Width;
            _height = (float)canvas.Size.Height;

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

        public void Draw(ICanvasAnimatedControl sender, CanvasDrawingSession ds)
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





