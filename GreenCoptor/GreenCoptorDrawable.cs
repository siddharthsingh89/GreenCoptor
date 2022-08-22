using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Random = System.Random;
using Microsoft.Maui.Graphics.Skia;
using SkiaSharp;
using System.Numerics;



namespace GreenCoptor
{
    class GreenCoptorDrawable : View, IDrawable
    {
        private const int V = 8;
        public double YAxis { get; set; }
        public float xShift { get; set; }
        private RectF _info;
        public float x = 100;
        public float y;
        public int count=1;
        private const int NumRect = 30;
        float[,] FrontScreen = new float[NumRect,V];
        float[,] BackScreen = new float[NumRect, V];
        double xMax;
        double leftover;
        double yMax;
        int scoreDelta = 10;
        double playerPushDelta = 0.0040;
        double yDelta = 0.0030;
        bool initialize = false;
        float maxWidth = 0;


        private bool IsAndroid() =>
    DeviceInfo.Current.Platform == DevicePlatform.Android;
        public string ButtonText
        {
            get => _buttonText;
            set
            {
                _buttonText = value;
                OnPropertyChanged();
            }
        }
        private string _buttonText;

        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged();
            }
        }
        private int _score;

        public int Level
        {
            get => _level;
            set
            {
                _level = value;
                OnPropertyChanged();
            }
        }
        private int _level;
        public bool IsGameOver { get; private set; }

        public GreenCoptorDrawable()
        {
            YAxis = 0.5;
            this.ButtonText = Constants.fly;
            Score = 0;
            xShift = 5;
            Level = 1;
        }
    
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            _info = dirtyRect;
            canvas.ResetState();
            canvas.StrokeColor = Colors.Green;
            canvas.FillColor = Colors.Green;

            if(!initialize)
            {
                Initilize();
            }

            drawTerrain(canvas);
            Move();

        }

        private void Initilize()
        {
            if(IsAndroid())
            {
                xMax = DeviceDisplay.MainDisplayInfo.Width;
                yMax = DeviceDisplay.MainDisplayInfo.Height-700;
            } else
            {
                xMax = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                yMax = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
                yMax -= 300;
            }
       
            //yMax = 500;
            
            //xMax = _info.Width
            //yMax = _info.Height;
            double max = xMax / NumRect;
            leftover = xMax % NumRect;
            maxWidth = (float)(max);
            float startX = 0, startY = 0, startXX = 0;
            for (int i = 0; i < NumRect; i++)
            {
                FrontScreen[i, 0] = startX;
                FrontScreen[i, 1] = startY;
               
                FrontScreen[i, 2] = maxWidth;
                if (i == NumRect - 1)
                    FrontScreen[i, 2] = maxWidth + (float)leftover;
                FrontScreen[i, 3] = (float)new Random().Next(40, 250);
                FrontScreen[i, 4] = startX;
                float height = (float)new Random().Next(40, 150);
                FrontScreen[i, 5] = (float)yMax -  height;
                FrontScreen[i, 6] = maxWidth;
                FrontScreen[i, 7] = height;
                startX += maxWidth;

                BackScreen[i, 0] = (float)xMax+ startXX;
                BackScreen[i, 1] = startY;
             
                BackScreen[i, 2] = maxWidth;
                if (i == NumRect - 1)
                    BackScreen[i, 2] = maxWidth + (float)leftover;
                BackScreen[i, 3] = (float)new Random().Next(40, 250);
                BackScreen[i, 4] = (float)xMax + startXX;
                float height1 = (float)new Random().Next(40, 300);
                BackScreen[i, 5] = (float)yMax - height;
                BackScreen[i, 6] = maxWidth;
                BackScreen[i, 7] = height;
                startXX += maxWidth;


            }
            initialize = true;
        }

        private void drawTerrain(ICanvas canvas)
        {
            if (IsGameOver)
            {
                PrsentEndGame(canvas);
                return;
            }
            float startX = 0, startY = 0;
            var helicoptor = SKPath.ParseSvgPathData(Constants.helicoptor);
            var _heli = PointsToPath(helicoptor.Points);
            canvas.StrokeColor = Colors.DeepSkyBlue;
            canvas.FillColor = Colors.DarkRed;
          
            var scaleX = 100 / helicoptor.Bounds.Width;
            float Scale = 0.4f;
        var jetScaleMatrix = Matrix3x2.CreateScale(Scale);


            _heli.Transform(jetScaleMatrix);

                var jetTranslationMatrix = Matrix3x2.CreateTranslation((float)0.5*(_info.Width - _heli.Bounds.Width),
                     (float)YAxis*(_info.Height - helicoptor.Bounds.Height));

            _heli.Transform(jetTranslationMatrix);
            canvas.FillPath(_heli);

            //Move helicoptor down
            YAxis = YAxis + yDelta;

            for (int i=0;i< NumRect; i++)
           {
                canvas.StrokeColor = Colors.Green;
                canvas.FillColor = Colors.Green;
                canvas.FillRoundedRectangle(FrontScreen[i,0], FrontScreen[i,1], FrontScreen[i, 2], FrontScreen[i,3],12);
                canvas.FillRoundedRectangle(FrontScreen[i, 4], FrontScreen[i, 5], FrontScreen[i, 6], FrontScreen[i, 7],12);
                canvas.FillRoundedRectangle(BackScreen[i, 0], BackScreen[i, 1], BackScreen[i, 2], BackScreen[i, 3],20);
                canvas.FillRoundedRectangle(BackScreen[i, 4], BackScreen[i, 5], BackScreen[i, 6], BackScreen[i, 7],20);               

                canvas.StrokeColor = Colors.Green;
                canvas.FillColor = Colors.Green;



            }
            bool collide = false;
            //check collisoin
            for(int i=0;i<NumRect;i++)
            {

                var r1 = new RectF(FrontScreen[i, 0], FrontScreen[i, 1], FrontScreen[i, 2], FrontScreen[i, 3]);
                var collide1 = _heli.Bounds.IntersectsWith(r1);
                var r2 = new RectF(FrontScreen[i, 4], FrontScreen[i, 5], FrontScreen[i, 6], FrontScreen[i, 7]);
                var collide2 = _heli.Bounds.IntersectsWith(r2);
                var r3 = new RectF(BackScreen[i, 0], BackScreen[i, 1], BackScreen[i, 2], BackScreen[i, 3]);
                var collide3 = _heli.Bounds.IntersectsWith(r3);
                var r4 = new RectF(BackScreen[i, 4], BackScreen[i, 5], BackScreen[i, 6], BackScreen[i, 7]);
                var collide4 = _heli.Bounds.IntersectsWith(r4);

                collide = collide1 || collide2 || collide3 || collide4;
                if (collide)
                    break;
            }

            if(collide)
            {
                IsGameOver = true;
            }
            Score += scoreDelta;
            setLevelByScore();

        }

        private void setLevelByScore()
        {
            if (Score < 3000)
            {
                Level = 1;
                xShift = 5;
                //playerPushDelta = 0.040;
            }

            else if(Score > 3000 && Score < 7000)
            {
                Level = 2;
                xShift= 10;
               scoreDelta = 20;
               // yDelta = 0.0040;
               // playerPushDelta = 0.050;

            }

            else if (Score > 7000 && Score < 15000)
            {
                Level = 3;
                xShift = 15;
               scoreDelta = 50;
               // yDelta = 0.0050;
               // playerPushDelta = 0.060;

            }
            else if (Score > 15000 && Score < 25000)
            {
                Level = 4;
                xShift = 20;
                scoreDelta = 70;
               // yDelta = 0.0060;
               // playerPushDelta = 0.0700;

            }
            else if (Score > 25000 && Score < 35000)
            {
                Level = 5;
                xShift = 25;
                scoreDelta = 90;
                //yDelta = 0.0070;
                //playerPushDelta = 0.080;
                    
            }
            else if (Score > 35000 && Score < 50000)
            {
                Level = 6;
                xShift = 27;
                scoreDelta = 100;
               // yDelta = 0.0075;
               // playerPushDelta = 0.30;
            }
            else if (Score > 50000)
            {
                Level = 7;
                xShift = maxWidth;
                scoreDelta = 150;
                //yDelta = 0.0080;
                //playerPushDelta = 0.50;
            }



        }


        private void Reset()
        {
            IsGameOver = false;
            YAxis = 0.5;
            Score = 0;
            Initilize();
            ButtonText = Constants.fly;
            xShift = 5;
            scoreDelta = 10;
            yDelta = 0.0030;
            playerPushDelta = 0.030;
        }

        private void PrsentEndGame(ICanvas canvas)
        {
           
            canvas.ResetState();

            canvas.FontColor = Colors.White;
            this.ButtonText = Constants.playAgain;
            canvas.FontSize = 40;
            canvas.DrawString( Constants.GAME_OVER, _info.Center.X, _info.Center.Y, HorizontalAlignment.Center);
            /*string data1 = "DeviceDisplay.MainDisplayInfo.Width : " + DeviceDisplay.MainDisplayInfo.Width;
            string data2 = "DeviceDisplay.MainDisplayInfo.Height : " + DeviceDisplay.MainDisplayInfo.Height;
            string data3 = "DeviceDisplay.MainDisplayInfo.Density : " + DeviceDisplay.MainDisplayInfo.Density;
            string data4 = "X Axis" + DeviceDisplay.MainDisplayInfo.Width/ DeviceDisplay.MainDisplayInfo.Density;
            string data5 = "Y Axis" + DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            canvas.DrawString(data1, _info.Center.X, _info.Center.Y-100, HorizontalAlignment.Center);
            canvas.DrawString(data2, _info.Center.X, _info.Center.Y-50, HorizontalAlignment.Center);
            canvas.DrawString(data3, _info.Center.X, _info.Center.Y + 50, HorizontalAlignment.Center);
            canvas.DrawString(data4, _info.Center.X, _info.Center.Y + 100, HorizontalAlignment.Center);
            canvas.DrawString(data5, _info.Center.X, _info.Center.Y + 150, HorizontalAlignment.Center);
            //canvas.FillRectangle(0, 0, (float)(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density), (float)(DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density));
            //canvas.FillRectangle(0, 0, 1080, 1600);*/
        }
        

        private PathF PointsToPath(SKPoint[] points)
        {
            var path = new PathF();
            for (var i = 0; i < points.Count(); i++)
            {
                var point = new PointF(points[i].X, points[i].Y);

                if (i == 0)
                {
                    path.MoveTo(point);
                }
                else if (i == points.Count() - 1)
                {
                    path.LineTo(point);
                    path.Close();
                }
                else
                {
                    path.LineTo(point);
                }
            }
            return path;
        }

        internal void Fire(bool v)
        {
            if(IsGameOver)
            {
                Reset();
            } 
            else
            {
                YAxis -= playerPushDelta;
            }
           
        }
        public void Move()
        {
            if (FrontScreen[NumRect - 1, 0] <=maxWidth)
            {
                ResetFirstRectToNextScreen();
            }
            if (BackScreen[NumRect-1,0] <= maxWidth)
            {
                ResetSecondRectToNextScreen();
            }
            
            ShiftLeft(FrontScreen);
            ShiftLeft(BackScreen);

        }

        private void ResetFirstRectToNextScreen()
        {
            double max = xMax / NumRect;
            double leftover = xMax % NumRect;
            maxWidth = (float)(max);
            float startX = 0, startY = 0, startXX = 0;
            for (int i = 0; i < NumRect; i++)
            {

                FrontScreen[i, 0] = (float)xMax + startXX;
                FrontScreen[i, 1] = startY;
                FrontScreen[i, 2] = maxWidth;
                FrontScreen[i, 3] = (float)new Random().Next(40, 300);
                FrontScreen[i, 4] = (float)xMax + startXX;
                float height1 = (float)new Random().Next(40, 250);
                FrontScreen[i, 5] = (float)yMax - height1;
                FrontScreen[i, 6] = maxWidth;
                FrontScreen[i, 7] = height1;
                startXX += maxWidth;
            }
        }

        private void ResetSecondRectToNextScreen()
        {

            double max = xMax / NumRect;
            double leftover = xMax % NumRect;
            maxWidth = (float)(max);
            float startX = 0, startY = 0, startXX = 0;
            for (int i = 0; i < NumRect; i++)
            {            

                BackScreen[i, 0] = (float)xMax + startXX;
                BackScreen[i, 1] = startY;
                BackScreen[i, 2] = maxWidth;
                BackScreen[i, 3] = (float)new Random().Next(40, 250);
                BackScreen[i, 4] = (float)xMax + startXX;
                float height1 = (float)new Random().Next(40, 300);
                BackScreen[i, 5] = (float)yMax - height1;
                BackScreen[i, 6] = maxWidth;
                BackScreen[i, 7] = height1;
                startXX += maxWidth;
            }
           
        }

        private void ShiftLeft(float[,] arr)
        {
            for(int i=0;i<NumRect;i++)
            {
                arr[i,0] = arr[i,0]- xShift;
                arr[i, 4] = arr[i, 4] - xShift;
            }

            
        }
    }
}
    
