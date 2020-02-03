using System;
using System.Diagnostics;
using System.Drawing;
using Accord.IO;
using Accord.Video.FFMPEG;

namespace UVEA
{
    class PolarCoordsConvertor
    {
        public static Tuple<double, double> ConvertToPolar(double x, double y)
        {
            double radius = Math.Sqrt(x * x + y * y);
            double angle = Math.Atan2(y, x);
            return new Tuple<double, double>(radius, angle);
        }

        public static int CECheck(Color color)
        {
            if (color.A == 0)
                return 1;
            return 0;
        }

        public static FastBitmap CompleteBitmap(FastBitmap bitmap)
        {
            bitmap.UnlockBits();
            var result = new FastBitmap((Bitmap)bitmap.GetSource().Clone());
            bitmap.LockBits();
            result.LockBits();
            for(var x = 1; x < bitmap.Width-1; x++)
            {
                for(var y = 1; y < bitmap.Height-1; y++)
                {
                    if(CECheck(bitmap.GetPixel(x, y)) == 1)
                    {
                        var counter = 0;
                        var red = 0;
                        var green = 0;
                        var blue = 0;
                        for (var ay = -1; ay < 2; ay++)
                        {
                            for (var ax = -1; ax < 2; ax++)
                            {
                                var pixel = bitmap.GetPixel(x+ax, y+ay);
                                counter += CECheck(pixel);
                                red += pixel.R;
                                green += pixel.G;
                                blue += pixel.B;
                            }
                        }
                        if (counter < 9)
                        {
                            var op = Color.FromArgb(red/(9 - counter), green/(9 - counter), blue/(9 - counter));
                            result.SetPixel(x, y, op);
                        }
                    }
                }
            }
            return result;
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

        public static void Run(string videoPath, string videoName)
        {
            var reader = new VideoFileReader();
            reader.Open(videoPath+videoName);
            var writer = new VideoFileWriter();
            var numberOfFrames = (int)reader.FrameCount;
            var probeBitmap = new FastBitmap(reader.ReadVideoFrame(0));
            probeBitmap.LockBits();
            var maxValues = CalcMaxValues(probeBitmap.Width, probeBitmap.Height);
            var maxX = maxValues.Item1;
            var maxY = maxValues.Item2;
            //Console.WriteLine($"{maxX}:{maxY}");
            writer.Height = probeBitmap.Height;
            writer.Width = probeBitmap.Width;
            writer.FrameRate = 30;
            writer.VideoCodec = VideoCodec.Mpeg4;
            writer.BitRate = reader.BitRate;
            writer.Open(videoPath+"out.avi");
            for (var t = 70; t < numberOfFrames; t++)
            {
                var convertedBitmap = new FastBitmap(new Bitmap(probeBitmap.Width, probeBitmap.Height));
                FastBitmap currentBitmap;
                try
                {
                    currentBitmap = new FastBitmap(reader.ReadVideoFrame(t));
                }
                catch (Exception ignored)
                {
                    break;
                }
                convertedBitmap.LockBits();
                currentBitmap.LockBits();
                for(var x = 0; x < probeBitmap.Width; x++)
                {
                    for(var y = 0; y < probeBitmap.Height; y++)
                    {
                        var pixel = ConvertToPolar(x - probeBitmap.Width/2, y - probeBitmap.Height/2);
                        var convX = FastUtils.FastAbs((int)(pixel.Item2 / maxY * (probeBitmap.Width - 1)));
                        var convY = FastUtils.FastAbs((int)(pixel.Item1 / maxX * (probeBitmap.Height - 1)));
                        convertedBitmap.SetPixel(convX, convY, currentBitmap.GetPixel(x, y));
                    }
                }
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < 4; i++)
                {
                    convertedBitmap = CompleteBitmap(convertedBitmap);
                }
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
                currentBitmap.UnlockBits();
                currentBitmap.DisposeSource();
                convertedBitmap.GetSource().Save(videoPath + "test.png");
                break;
                //convertedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                writer.WriteVideoFrame(convertedBitmap.GetSource());
                convertedBitmap.DisposeSource();
            }
            probeBitmap.UnlockBits();
            writer.Flush();
        }
    }
}
