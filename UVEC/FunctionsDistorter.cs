using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Spherical
        }

        public static Tuple<double, double> RunFunction(double x, double y, double multiplier, functions function)
        {
            Tuple<double, double> result = new Tuple<double, double>(0, 0);
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
                default:
                    break;
            }
            return result;
        }

        public static int CBCheck(Color color)
        {
            if ((color.A + color.R + color.G + color.B) == 0)
                return 1;
            return 0;
        }

        public static Bitmap CompleteBitmap(Bitmap bitmap)
        {
            for(var x = 0; x < bitmap.Width; x++)
            {
                for(var y = 0; y < bitmap.Height; y++)
                {
                    var counter = 0;
                    var pixel = bitmap.GetPixel(x, y);
                    if((pixel.A + pixel.R + pixel.G + pixel.B) == 0)
                    {
                        if (!(x == 0 || x == bitmap.Width - 1 || y == 0 || y == bitmap.Height - 1))
                        {
                            var lp = bitmap.GetPixel(x - 1, y);
                            var rp = bitmap.GetPixel(x + 1, y);
                            var up = bitmap.GetPixel(x, y - 1);
                            var dp = bitmap.GetPixel(x, y - 1);
                            counter += CBCheck(lp);
                            counter += CBCheck(rp);
                            counter += CBCheck(up);
                            counter += CBCheck(dp);
                            var r = (lp.R + rp.R + up.R + dp.R) / (4 - counter);
                            var g = (lp.G + rp.G + up.G + dp.G) / (4 - counter);
                            var b = (lp.B + rp.B + up.B + dp.B) / (4 - counter);
                            var op = Color.FromArgb(r, g, b);
                            bitmap.SetPixel(x, y, op);
                        }
                        else
                            bitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                }
            }
            return bitmap;
        }

        public static Tuple<double, double> CalcMaxValues(int width, int height, int numberOfFrames, double multiplier, functions function)
        {
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            for(var t = 0; t < numberOfFrames; t++)
            {
                for (var i = 0; i < width; i++)
                {
                    for (var j = 0; j < height; j++)
                    {
                        var output = RunFunction(i, j, (double)t/numberOfFrames*multiplier + 1, function);
                        maxX = Math.Max(output.Item1, maxX);
                        maxY = Math.Max(output.Item2, maxY);
                    }
                }
                if (multiplier == 0)
                    break;
                Progress.Report(t, numberOfFrames);
            }
            return new Tuple<double, double>(maxX, maxY);
        }

        public static void Run(string videoPath, double multiplier, functions function)
        {
            Bitmap probeBitmap = new Bitmap($"{videoPath}InputSequence\\1.png");
            var numberOfFrames = new DirectoryInfo($"{videoPath}InputSequence").GetFiles().Length;
            Console.WriteLine($"Считаем максимумы. Может занять длительное время, если множитель >0(сейчас {multiplier})");
            var maxValues = CalcMaxValues(probeBitmap.Width, probeBitmap.Height, numberOfFrames, multiplier, function);
            var maxX = maxValues.Item1;
            var maxY = maxValues.Item2;
            Console.WriteLine($"Maximum x: {maxX}\nMaximum y: {maxY}");
            Console.WriteLine("Начинаем рендер");
            for (var t = 0; t < numberOfFrames; t++)
            {
                var convertedBitmap = new Bitmap(probeBitmap.Width, probeBitmap.Height);
                var currentBitmap = new Bitmap($"{videoPath}InputSequence\\{(t + 1).ToString()}.png");
                for(var x = 0; x < probeBitmap.Width; x++)
                {
                    for(var y = 0; y < probeBitmap.Height; y++)
                    {
                        var pixel = RunFunction(x, y, (double)t/numberOfFrames*multiplier + 1, function);
                        var convX = Math.Abs((int)(pixel.Item1 / maxX * (probeBitmap.Width - 1)));
                        var convY = Math.Abs((int)(pixel.Item2 / maxY * (probeBitmap.Height - 1)));
                        convertedBitmap.SetPixel(convX, convY, currentBitmap.GetPixel(x, y));
                    }
                }
                CompleteBitmap(convertedBitmap);
                convertedBitmap.Save($"{videoPath}OutputSequence\\{(t + 1).ToString()}.png");
                Progress.Report(t, numberOfFrames);
            }
        }
    }
}