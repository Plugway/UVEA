using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static UVEC.FastUtils;

namespace UVEC
{
    class Functions
    {
        public static Tuple<double, double> Polar(double x, double y, double multiplier)
        {
            var radius = Math.Sqrt((x * x) + (y * y) * multiplier);
            var angle = Math.Atan2(y, x);
            return new Tuple<double, double>(radius, angle);
        }

        public static Tuple<double, double> Parabolic(double x, double y, double multiplier)
        {
            var outX = x * x * multiplier;      //x * x + x * multiplier;
            var outY = y * y * multiplier;      //y / multiplier + y * multiplier;
            return new Tuple<double, double>(outX, outY);
        }

        public static Tuple<double, double> Hyperbolic(double x, double y, double multiplier)
        {
            if (x == 0)
                x = 0.01;
            y++;
            var outX = multiplier / x;
            var outY = y / multiplier;
            return new Tuple<double, double>(outX, outY);
        }

        public static Tuple<double, double> Sine(double x, double y, double multiplier)
        {
            var outX = Math.Sin(x * multiplier);
            var outY = Math.Sin(multiplier * y);
            return new Tuple<double, double>(outX, outY);
        }

        public static Tuple<double, double> Cosine(double x, double y, double multiplier)
        {
            var outX = Math.Cos(x * multiplier);
            var outY = Math.Cos(multiplier * y);
            return new Tuple<double, double>(outX, outY);
        }

        public static Tuple<double, double> Tangent(double x, double y, double multiplier)
        {
            var outX = Math.Tan(x * multiplier);
            var outY = Math.Tan(y * multiplier);
            return new Tuple<double, double>(outX, outY);
        }

        public static Tuple<double, double> Rectangular(double x, double y, double multiplier)
        {
            var outX = Math.Cos(x + multiplier) * multiplier;
            var outY = Math.Sin(y - multiplier) * multiplier;
            return new Tuple<double, double>(outX, outY);
        }

        public static Tuple<double, double> Spherical(double x, double y, double multiplier)
        {
            multiplier = multiplier * Math.Cos(x * y);
            var outX = multiplier * Math.Cos(x) * Math.Sin(y);
            var outY = multiplier * Math.Sin(x) * Math.Cos(y);
            return new Tuple<double, double>(outX, outY);
        }

        public static Tuple<double,double> Direct(double x, double y, double multiplier)
        {
            return new Tuple<double, double>(x, y);
        }

        public static Tuple<double, double> FishEye(double x, double y, double multiplier, int width, int height)
        {
            var nx = x / (width / 2.0);
            var ny = y / (height / 2.0);
            var r = Math.Sqrt(nx * nx + ny * ny);
            if (r >= 0.0 && r <= 1.0)
            {
                var nr = Math.Sqrt(1.0 - r * r);
                nr = r + (1.0 - nr) / 2.0;
                if (nr <= 1.0)
                {
                    var theta = Math.Atan2(ny, nx);
                    var nxn = nr * Math.Cos(theta);
                    var nyn = nr * Math.Sin(theta);
                    var x2 = (int)((nxn + 1) * width / 2.0);
                    var y2 = (int)((nyn + 1) * height / 2.0);
                    return new Tuple<double, double>(x2, y2);
                }
            }
            return new Tuple<double, double>(x + width/2, y + height/2);
        }
    }

    class FunctionsDistorter
    {

        public enum functions
        {
            Polar,
            Parabolic,
            Hyperbolic,
            Sine,
            Cosine,
            Tangent,
            Rectangular,
            Spherical,
            Direct,
            FishEye
        }

        public static Tuple<int, int> ChangeCoordsLocation(int x, int y, functions function, int width, int height)
        {
            var pX = x;
            var pY = y;
            if (!(function == functions.Parabolic || function == functions.Direct))
            {
                pX = x - (width - 1) / 2;
                pY = y - (height - 1) / 2;
            }
            return new Tuple<int, int>(pX, pY);
        }

        public static Tuple<double, double> RunFunction(double x, double y, double multiplier, functions function, int width, int height)
        {
            var result = new Tuple<double, double>(0, 0);
            switch(function)
            {
                case functions.Cosine:
                    result = Functions.Cosine(x, y, multiplier);
                    break;
                case functions.Hyperbolic:
                    result = Functions.Hyperbolic(x, y, multiplier);
                    break;
                case functions.Parabolic:
                    result = Functions.Parabolic(x, y, multiplier);
                    break;
                case functions.Polar:
                    result = Functions.Polar(x, y, multiplier);
                    break;
                case functions.Rectangular:
                    result = Functions.Rectangular(x, y, multiplier);
                    break;
                case functions.Sine:
                    result = Functions.Sine(x, y, multiplier);
                    break;
                case functions.Spherical:
                    result = Functions.Spherical(x, y, multiplier);
                    break;
                case functions.Tangent:
                    result = Functions.Tangent(x, y, multiplier);
                    break;
                case functions.Direct:
                    result = Functions.Direct(x, y, multiplier);
                    break;
                case functions.FishEye:
                    result = Functions.FishEye(x, y, multiplier, width, height);
                    break;
                default:
                    break;
            }
            return result;
        }

        public static void CECheck(Color color, ref int EmptyCount)
        {
            if ((color.A + color.R + color.G + color.B) == 0)
                EmptyCount++;
        }

        public static Bitmap CompleteBitmap(Bitmap bitmap)
        {
            for(var x = 0; x < bitmap.Width; x++)
            {
                for(var y = 0; y < bitmap.Height; y++)
                {
                    var EmptyCount = 0;
                    var pixel = bitmap.GetPixel(x, y);
                    if((pixel.A + pixel.R + pixel.G + pixel.B) == 0)
                    {
                        if (!(x == 0 || x == bitmap.Width - 1 || y == 0 || y == bitmap.Height - 1))
                        {
                            var lp = bitmap.GetPixel(x - 1, y);
                            var rp = bitmap.GetPixel(x + 1, y);
                            var up = bitmap.GetPixel(x, y - 1);
                            var dp = bitmap.GetPixel(x, y - 1);
                            CECheck(lp, ref EmptyCount);
                            CECheck(rp, ref EmptyCount);
                            CECheck(up, ref EmptyCount);
                            CECheck(dp, ref EmptyCount);
                            var r = (lp.R + rp.R + up.R + dp.R) / (4 - EmptyCount);
                            var g = (lp.G + rp.G + up.G + dp.G) / (4 - EmptyCount);
                            var b = (lp.B + rp.B + up.B + dp.B) / (4 - EmptyCount);
                            bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                        }
                        else
                            bitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                }
            }
            return bitmap;
        }

        public static Tuple<double, double> CalcMaxValues(int width, int height, int numberOfFrames,
            double multiplier, functions function, string videoPath)
        {
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            for(var t = 0; t < numberOfFrames; t++)
            {
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var coords = ChangeCoordsLocation(x, y, function, width, height);
                        var output = RunFunction(coords.Item1, coords.Item2, (double)t/numberOfFrames*multiplier + 1,
                            function, width, height);
                        maxX = Math.Max(output.Item1, maxX);
                        maxY = Math.Max(output.Item2, maxY);
                    }
                }
                if (multiplier == 0)
                    break;
                Progress.ReportFastTime(t, numberOfFrames);
            }
            return new Tuple<double, double>(maxX, maxY);
        }
        public static void Run(string videoPath, double multiplier, functions function)
        {
            Bitmap probeBitmap = new Bitmap($"{videoPath}InputSequence\\1.png");
            var numberOfFrames = new DirectoryInfo($"{videoPath}InputSequence").GetFiles().Length;
            Console.WriteLine($"Считаем максимумы. Может занять длительное время, если множитель >0(сейчас {multiplier})");
            var maxValues = CalcMaxValues(probeBitmap.Width, probeBitmap.Height, numberOfFrames, multiplier, function, videoPath);
            var maxX = maxValues.Item1;
            var maxY = maxValues.Item2;
            var width = probeBitmap.Width;
            var height = probeBitmap.Height;
            Console.WriteLine($"Maximum x: {maxX}\nMaximum y: {maxY}");
            Console.WriteLine("Начинаем рендер");
            for(var t = 0; t < numberOfFrames; t++)
            {
                var convertedBitmap = new Bitmap(width, height);
                var currentBitmap = new Bitmap($"{videoPath}InputSequence\\{(t + 1).ToString()}.png");
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var coords = ChangeCoordsLocation(x, y, function, width, height);
                        var pixel = RunFunction(coords.Item1, coords.Item2, (double)t / numberOfFrames * multiplier + 1,
                            function, width, height);
                        var convX = FastAbs(FastRoundInt(pixel.Item1 / maxX * (width - 1)));
                        var convY = FastAbs(FastRoundInt(pixel.Item2 / maxY * (height - 1)));
                        convertedBitmap.SetPixel(convX, convY, currentBitmap.GetPixel(x, y));
                    }
                }
                CompleteBitmap(convertedBitmap);
                convertedBitmap.Save($"{videoPath}OutputSequence\\{(t + 1).ToString()}.png");
                Progress.ReportFastTime(t++, numberOfFrames);
            }
        }

        public static void RunParallel(string videoPath, double multiplier, functions function)
        {
            Bitmap probeBitmap = new Bitmap($"{videoPath}InputSequence\\1.png");
            var numberOfFrames = new DirectoryInfo($"{videoPath}InputSequence").GetFiles().Length;
            Console.WriteLine($"Считаем максимумы. Может занять длительное время, если множитель >0(сейчас {multiplier})");
            var maxValues = CalcMaxValues(probeBitmap.Width, probeBitmap.Height, numberOfFrames, multiplier, function, videoPath);
            var maxX = maxValues.Item1;
            var maxY = maxValues.Item2;
            var width = probeBitmap.Width;
            var height = probeBitmap.Height;
            var progress = 0;
            Console.WriteLine($"Maximum x: {maxX}\nMaximum y: {maxY}");
            Console.WriteLine("Начинаем рендер");
            Parallel.For(0, numberOfFrames, t =>
            {
                var convertedBitmap = new Bitmap(width, height);
                var currentBitmap = new Bitmap($"{videoPath}InputSequence\\{(t + 1).ToString()}.png");
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var coords = ChangeCoordsLocation(x, y, function, width, height);
                        var pixel = RunFunction(coords.Item1, coords.Item2, (double)t / numberOfFrames * multiplier + 1,
                            function, width, height);
                        var convX = FastAbs(FastRoundInt(pixel.Item1 / maxX * (width - 1)));
                        var convY = FastAbs(FastRoundInt(pixel.Item2 / maxY * (height - 1)));
                        convertedBitmap.SetPixel(convX, convY, currentBitmap.GetPixel(x, y));
                    }
                }
                CompleteBitmap(convertedBitmap);
                convertedBitmap.Save($"{videoPath}OutputSequence\\{(t + 1).ToString()}.png");
                Progress.ReportFastTime(progress++, numberOfFrames);
            }
            );
        }
    }
}