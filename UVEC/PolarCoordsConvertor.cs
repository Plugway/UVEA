using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UVEC
{
    class PolarCoordsConvertor
    {
        public static Tuple<double, double> ConvertToPolar(double x, double y)
        {
            double radius = Math.Sqrt((x * x) + (y * y));
            double angle = Math.Atan2(y, x);
            return new Tuple<double, double>(radius, angle);
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

        public static Tuple<double, double> CalcMaxValues(int width, int height)
        {
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var output = ConvertToPolar(i - width / 2, j - height / 2);
                    maxX = Math.Max(output.Item1, maxX);
                    maxY = Math.Max(output.Item2, maxY);
                }
            }
            return new Tuple<double, double>(maxX, maxY);
        }

        public static void Run(string videoPath)
        {
            Bitmap probeBitmap = new Bitmap($"{videoPath}InputSequence\\1.png");
            var numberOfFrames = new DirectoryInfo($"{videoPath}InputSequence").GetFiles().Length;
            var maxValues = CalcMaxValues(probeBitmap.Width, probeBitmap.Height);
            var maxX = maxValues.Item1;
            var maxY = maxValues.Item2;
            //Console.WriteLine($"{maxX}:{maxY}");
            for (var t = 0; t < numberOfFrames; t++)
            {
                var convertedBitmap = new Bitmap(probeBitmap.Width, probeBitmap.Height);
                var currentBitmap = new Bitmap($"{videoPath}InputSequence\\{(t + 1).ToString()}.png");
                for(var x = 0; x < probeBitmap.Width; x++)
                {
                    for(var y = 0; y < probeBitmap.Height; y++)
                    {
                        var pixel = ConvertToPolar(x - probeBitmap.Width/2, y - probeBitmap.Height/2);
                        var convX = Math.Abs((int)(pixel.Item2 / maxY * (probeBitmap.Width - 1)));
                        var convY = Math.Abs((int)(pixel.Item1 / maxX * (probeBitmap.Height - 1)));
                        convertedBitmap.SetPixel(convX, convY, currentBitmap.GetPixel(x, y));
                    }
                }
                CompleteBitmap(convertedBitmap);
                convertedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                convertedBitmap.Save($"{videoPath}OutputSequence\\{(t + 1).ToString()}.png");
                stopwatch.Stop();
                Console.WriteLine($"{stopwatch.ElapsedMilliseconds} ms");
                //Progress.Report(numberOfFrames, videoPath);
            }
        }
    }
}
