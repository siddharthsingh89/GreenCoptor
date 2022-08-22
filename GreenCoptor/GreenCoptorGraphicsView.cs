using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GreenCoptor
{
    internal class GreenCoptorGraphicsView: GraphicsView
    {
        private int _fpsElapsed;
        private int _fpsCount = 0;
        private const double _fps = 30;
        private readonly Stopwatch _stopWatch = new Stopwatch();

        public static GreenCoptorDrawable Drawable;

        public static ICommand Fire = new Command(() => Drawable.Fire(true));

        public static readonly BindableProperty YAxisScaleProperty = BindableProperty.Create(nameof(YAxisScale),
          typeof(double),
          typeof(GreenCoptorGraphicsView),
          0.5,
          propertyChanged: (b, o, n) => {
              Drawable.YAxis = (double)n;
          });

        public double YAxisScale
        {
            get => (double)GetValue(YAxisScaleProperty);
            set => SetValue(YAxisScaleProperty, value);
        }

        [Obsolete]
        public GreenCoptorGraphicsView()
        {
            base.Drawable = Drawable = new GreenCoptorDrawable();

            var ms = 1000.0 / _fps;
            var ts = TimeSpan.FromMilliseconds(ms);           
            Device.StartTimer(ts, TimerLoop);
        }

        private bool TimerLoop()
        {
            // get the elapsed time from the stopwatch because the 1/30 timer interval is not accurate and can be off by 2 ms
            var dt = _stopWatch.Elapsed.TotalSeconds;
            _stopWatch.Restart();

            // calculate current fps
            var fps = dt > 0 ? 1.0 / dt : 0;

            // when the fps is too low reduce the load by skipping the frame
            if (fps < _fps / 2)
                return true;

            _fpsCount++;
            _fpsElapsed++;

            if (_fpsCount == 20)
                _fpsCount = 0;

            //Its been a second
            if (_fpsElapsed == _fps)
            {
                _fpsElapsed = 0;
                //Drawable.Move();
            }

            Invalidate();

            return true;
        }
    }
}
