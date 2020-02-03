using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Accord.Video.FFMPEG;
using static UVEA.FastUtils;

namespace UVEA
{
    class FunctionsProcessing
    {
        public static Tuple<double, double> Polar(double x, double y, double multiplier)
        {
            var radius = Math.Sqrt(x * x + y * y * multiplier);
            var angle = Math.Atan2(y, x);
            return new Tuple<double, double>(angle, radius);
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
            return new Tuple<double, double>(x*multiplier, y*multiplier);
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

    public enum Functions
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

    public class OneFrameDistorter
    {
        private double maxX = double.MinValue;
        private double maxY = double.MinValue;
        private double minX = double.MaxValue;
        private double minY = double.MaxValue;
        public static Tuple<double, double> RunFunction(double x, double y, double multiplier, Functions function, int width, int height)
        {
            var result = new Tuple<double, double>(0, 0);
            switch(function)
            {
                case Functions.Cosine:
                    result = FunctionsProcessing.Cosine(x, y, multiplier);
                    break;
                case Functions.Hyperbolic:
                    result = FunctionsProcessing.Hyperbolic(x, y, multiplier);
                    break;
                case Functions.Parabolic:
                    result = FunctionsProcessing.Parabolic(x, y, multiplier);
                    break;
                case Functions.Polar:
                    result = FunctionsProcessing.Polar(x - width/2, y-height/2, multiplier);
                    break;
                case Functions.Rectangular:
                    result = FunctionsProcessing.Rectangular(x, y, multiplier);
                    break;
                case Functions.Sine:
                    result = FunctionsProcessing.Sine(x - width/2, y - height/2, multiplier);
                    break;
                case Functions.Spherical:
                    result = FunctionsProcessing.Spherical(x, y, multiplier);
                    break;
                case Functions.Tangent:
                    result = FunctionsProcessing.Tangent(x, y, multiplier);
                    break;
                case Functions.Direct:
                    result = FunctionsProcessing.Direct(x, y, multiplier);
                    break;
                case Functions.FishEye:
                    result = FunctionsProcessing.FishEye(x, y, multiplier, width, height);
                    break;
            }
            return result;
        }

        public static Bitmap CompleteBitmap(Bitmap bitmap)
        {
            var bm = (Bitmap) bitmap.Clone();
            var graphics = Graphics.FromImage(bm);
            graphics.DrawImage(bitmap, -1, -1);
            graphics.DrawImage(bitmap, 1, -1);
            graphics.DrawImage(bitmap, -1, 1);
            graphics.DrawImage(bitmap, 1, 1);
            graphics.Dispose();
            var blured = new GaussianBlur(bm).Process(2);
            graphics = Graphics.FromImage(blured);
            graphics.DrawImage(bitmap, 0, 0);
            return blured;
            /*var bfast = new FastBitmap(blured); //draw glich
            bfast.LockBits();
            for (int x = 0; x < blured.Width; x++)
            {
                for (int y = 0; y < blured.Height; y++)
                {
                    var color = bfast.GetPixel(x, y);
                    Color res = Color.FromArgb(0,0,0);
                    if (color.R > color.G && color.R > color.B)
                    {
                        res = Color.FromArgb(255, color.G + 255 - color.R, color.B + 255 - color.R);
                    }
                    else if (color.G > color.R && color.G > color.B)
                    {
                        res = Color.FromArgb(color.R + 255 - color.G, 255, color.B + 255 - color.G);
                    }
                    else if (color.B > color.R && color.B > color.G)
                    {
                        res = Color.FromArgb(color.R + 255 - color.B, color.G + 255 - color.B,255);
                    }
                    bfast.SetPixel(x, y, res);
                }
            }
            bfast.UnlockBits();*/
            /*
            for(var x = 1; x < bitmap.Width-1; x++)
            {
                for(var y = 1; y < bitmap.Height-1; y++)
                {
                    if(CECheck(bitmap.GetPixel(x, y)) == 0)
                    {
                        var lp = bitmap.GetPixel(x - 1, y);
                        var rp = bitmap.GetPixel(x + 1, y);
                        var up = bitmap.GetPixel(x, y - 1);
                        var dp = bitmap.GetPixel(x, y - 1);
                        var emptyCount = CECheck(lp)+CECheck(rp)+CECheck(up)+CECheck(dp);
                        if (emptyCount < 4)
                        {
                            var r = (lp.R + rp.R + up.R + dp.R) / (4 - emptyCount);
                            var g = (lp.G + rp.G + up.G + dp.G) / (4 - emptyCount);
                            var b = (lp.B + rp.B + up.B + dp.B) / (4 - emptyCount);
                            bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                        }
                        else
                        {
                            bitmap.SetPixel(x, y, Color.Black);
                        }
                    }
                }
            }*/
        }
        public static Tuple<double, double, double, double> CalcMaxValuesParallel(int width, int height, double multiplierFrom,
            double multiplierTo, int frameCount, Functions function)
        {
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            //for (var t = 0; t < frameCount; t++)
            Parallel.For(0, frameCount, t =>
            {
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var output = RunFunction(x, y, GetMultiplier(t, frameCount, multiplierFrom, multiplierTo),
                            function, width, height);
                        maxX = Math.Max(output.Item1, maxX);
                        maxY = Math.Max(output.Item2, maxY);
                        minX = Math.Min(output.Item1, minX);
                        minY = Math.Min(output.Item2, minY);
                    }
                }
            });
            return new Tuple<double, double, double, double>(maxX, maxY, minX, minY);
        }

        private void CalcMaxValuesOneFrame(int width, int height, double multiplier, Functions function)
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var output = RunFunction(x, y, multiplier, function, width, height);
                    maxX = Math.Max(output.Item1, maxX);
                    maxY = Math.Max(output.Item2, maxY);
                    minX = Math.Min(output.Item1, minX);
                    minY = Math.Min(output.Item2, minY);
                }
            }
        }

        public void ResetMaxMinValues()
        {
            maxX = double.MinValue;
            maxY = double.MinValue;
            minX = double.MaxValue;
            minY = double.MaxValue;
        }

        public static Tuple<double, double, double, double> CalcMaxValues(int width, int height, double multiplierFrom,
            double multiplierTo, int frameCount, Functions function)
        {
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            for (var t = 0; t < frameCount; t++)
            {
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var output = RunFunction(x, y, GetMultiplier(t, frameCount, multiplierFrom, multiplierTo),
                            function, width, height);
                        maxX = Math.Max(output.Item1, maxX);
                        maxY = Math.Max(output.Item2, maxY);
                        minX = Math.Min(output.Item1, minX);
                        minY = Math.Min(output.Item2, minY);
                    }
                }
            }
            return new Tuple<double, double, double, double>(maxX, maxY, minX, minY);
        }
        public Bitmap RunOneFrame(Bitmap frame, double multiplier, Functions function)//fix
        {
            CalcMaxValuesOneFrame(frame.Width, frame.Height, multiplier, function);
            var width = frame.Width;
            var height = frame.Height;
            var convertedBitmap = new FastBitmap(new Bitmap(width, height));
            convertedBitmap.LockBits();
            var currentBitmap = new FastBitmap(frame);
            currentBitmap.LockBits();
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var pixel = RunFunction(x, y, multiplier,
                        function, width, height);
                    var convX = FastRoundInt(Map(pixel.Item1, minX, maxX, 0, width-1));
                    var convY = FastRoundInt(Map(pixel.Item2, minY, maxY, 0, height-1));
                    convertedBitmap.SetPixel(convX, convY, currentBitmap.GetPixel(x, y));
                }
            }
            currentBitmap.UnlockBits();
            currentBitmap.DisposeSource();
            convertedBitmap.UnlockBits();
            return CompleteBitmap(convertedBitmap.GetSource());
        }
        public Bitmap RunOneFrame(VideoFileReader reader, int frameNum, double multiplierFrom,
            double multiplierTo, Functions function)
        {
            if (!reader.IsOpen)
            {
                throw new Exception("Файл не открыт.");
            }
            var numberOfFrames = (int)reader.FrameCount;
            var multiplier = GetMultiplier(frameNum, numberOfFrames, multiplierFrom, multiplierTo);
            CalcMaxValuesOneFrame(reader.Width, reader.Height, multiplier, function);
            var width = reader.Width;
            var height = reader.Height;
            var convertedBitmap = new FastBitmap(new Bitmap(width, height));
            convertedBitmap.LockBits();
            FastBitmap currentBitmap;
            try
            {
                currentBitmap = new FastBitmap(reader.ReadVideoFrame(frameNum));
            }
            catch (Exception ignored)
            {
                return new Bitmap(width, height);
            }
            currentBitmap.LockBits();
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var pixel = RunFunction(x, y, multiplier,
                        function, width, height);
                    var convX = FastRoundInt(Map(pixel.Item1, minX, maxX, 0, width-1));
                    var convY = FastRoundInt(Map(pixel.Item2, minY, maxY, 0, height-1));
                    convertedBitmap.SetPixel(convX, convY, currentBitmap.GetPixel(x, y));
                }
            }
            currentBitmap.UnlockBits();
            currentBitmap.DisposeSource();
            convertedBitmap.UnlockBits();
            return CompleteBitmap(convertedBitmap.GetSource());
        }
        public Bitmap RunOneFrame(VideoFileReader reader, int frameNum, double multiplierFrom,
            double multiplierTo, Functions function, int maxWidth, int maxHeight)
        {
            if (!reader.IsOpen)
            {
                throw new Exception("Файл не открыт.");
            }
            var numberOfFrames = (int)reader.FrameCount;
            var multiplier = GetMultiplier(frameNum, numberOfFrames, multiplierFrom, multiplierTo);
            FastBitmap currentBitmap;
            try
            {
                currentBitmap = new FastBitmap(ScaleImage(reader.ReadVideoFrame(frameNum), maxWidth, maxHeight,
                    true, false));
            }
            catch (Exception ignored)
            {
                return new Bitmap(maxWidth, maxHeight);
            }
            currentBitmap.LockBits();
            var width = currentBitmap.Width;
            var height = currentBitmap.Height;
            CalcMaxValuesOneFrame(width, height, multiplier, function);
            var convertedBitmap = new FastBitmap(new Bitmap(width, height));
            convertedBitmap.LockBits();
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var pixel = RunFunction(x, y, multiplier,
                        function, width, height);
                    var convX = FastRoundInt(Map(pixel.Item1, minX, maxX, 0, width-1));
                    var convY = FastRoundInt(Map(pixel.Item2, minY, maxY, 0, height-1));
                    convertedBitmap.SetPixel(convX, convY, currentBitmap.GetPixel(x, y));
                }
            }
            currentBitmap.UnlockBits();
            currentBitmap.DisposeSource();
            convertedBitmap.UnlockBits();
            return CompleteBitmap(convertedBitmap.GetSource());
        }
        public static void Run(VideoFileReader reader, VideoFileWriter writer, double multiplierFrom, double multiplierTo,
            Functions function, BackgroundWorker renderWorker)
        {
            if (!reader.IsOpen || !writer.IsOpen)
            {
                throw new Exception("Файл не открыт.");
            }
            var numberOfFrames = (int)reader.FrameCount;
            Console.WriteLine("Считаем максимумы.");
            var maxValues = CalcMaxValuesParallel(reader.Width, reader.Height, multiplierFrom, multiplierTo, numberOfFrames, function);
            var maxX = maxValues.Item1;
            var maxY = maxValues.Item2;
            var minX = maxValues.Item3;
            var minY = maxValues.Item4;
            var width = reader.Width;
            var height = reader.Height;
            Console.WriteLine($"Maximum x: {maxX}; Maximum y: {maxY}; Minimum x: {minX}; Minimum y: {minY}");
            Console.WriteLine("Начинаем рендер");
            for(var t = 0; t < numberOfFrames; t++)
            {
                var convertedBitmap = new FastBitmap(new Bitmap(width, height));
                convertedBitmap.LockBits();
                FastBitmap currentBitmap;
                try
                {
                    currentBitmap = new FastBitmap(reader.ReadVideoFrame(t));
                }
                catch (Exception ignored)
                {
                    break;
                }
                currentBitmap.LockBits();
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var pixel = RunFunction(x, y, GetMultiplier(t, numberOfFrames, multiplierFrom, multiplierTo),
                            function, width, height);
                        var convX = FastRoundInt(Map(pixel.Item1, minX, maxX, 0, width-1));
                        var convY = FastRoundInt(Map(pixel.Item2, minY, maxY, 0, height-1));
                        convertedBitmap.SetPixel(convX, convY, currentBitmap.GetPixel(x, y));
                    }
                }
                currentBitmap.UnlockBits();
                currentBitmap.DisposeSource();
                convertedBitmap.UnlockBits();
                //timeLine.Value = t;
                var res = CompleteBitmap(convertedBitmap.GetSource());
                writer.WriteVideoFrame(res);
                AppForm.PreviewBitmap = (Bitmap)res.Clone();
                renderWorker.ReportProgress(FastRoundInt(t*1000.0/numberOfFrames));
                //Progress.ReportFastTime(t++, numberOfFrames);
            }
        }

        public static double GetMultiplier(int currentFrameNum, int numberOfAllFrames, double multiplierFrom,
            double multiplierTo)
        {
            return Map(currentFrameNum, 0, numberOfAllFrames, multiplierFrom, multiplierTo);
        }

        public static void RunParallel(VideoFileReader reader, VideoFileWriter writer, double multiplierFrom, double multiplierTo,
            Functions function)
        {
            if (!reader.IsOpen || !writer.IsOpen)
            {
                throw new Exception("Файл не открыт.");
            }
            var numberOfFrames = (int)reader.FrameCount;
            Console.WriteLine("Считаем максимумы.");
            var maxValues = CalcMaxValuesParallel(reader.Width, reader.Height, multiplierFrom, multiplierTo, numberOfFrames, function);
            var maxX = maxValues.Item1;
            var maxY = maxValues.Item2;
            var minX = maxValues.Item3;
            var minY = maxValues.Item4;
            var width = reader.Width;
            var height = reader.Height;
            Console.WriteLine($"Maximum x: {maxX}; Maximum y: {maxY}; Minimum x: {minX}; Minimum y: {minY}");
            Console.WriteLine("Начинаем рендер");
            for(var t = 0; t < numberOfFrames; t++)
            {
                var convertedBitmap = new FastBitmap(new Bitmap(width, height));
                convertedBitmap.LockBits();
                FastBitmap currentBitmap;
                try
                {
                    currentBitmap = new FastBitmap(reader.ReadVideoFrame(t));
                }
                catch (Exception ignored)
                {
                    break;
                }
                currentBitmap.LockBits();
                Parallel.For(0, width, x =>
                {
                    for (var y = 0; y < height; y++)
                    {
                        var pixel = RunFunction(x, y,
                            GetMultiplier(t, numberOfFrames, multiplierFrom, multiplierTo),
                            function, width, height);
                        var convX = FastRoundInt(Map(pixel.Item1, minX, maxX, 0, width - 1));
                        var convY = FastRoundInt(Map(pixel.Item2, minY, maxY, 0, height - 1));
                        convertedBitmap.SetPixel(convX, convY, currentBitmap.GetPixel(x, y));
                    }
                });
                currentBitmap.UnlockBits();
                currentBitmap.DisposeSource();
                convertedBitmap.UnlockBits();
                writer.WriteVideoFrame(CompleteBitmap(convertedBitmap.GetSource()));
            }
        }
        /*public static void RunParallel(VideoFileReader reader, VideoFileWriter writer, double multiplierFrom, double multiplierTo,
            Functions function)
        {
            if (!reader.IsOpen || !writer.IsOpen)
            {
                throw new Exception("Файл не открыт.");
            }
            var numberOfFrames = (int)reader.FrameCount;
            Console.WriteLine("Считаем максимумы.");
            var maxValues = CalcMaxValuesParallel(reader.Width, reader.Height, multiplierFrom, multiplierTo, numberOfFrames, function);
            var maxX = maxValues.Item1;
            var maxY = maxValues.Item2;
            var minX = maxValues.Item3;
            var minY = maxValues.Item4;
            var width = reader.Width;
            var height = reader.Height;
            var progress = 0;
            var lastFrame = -1;
            var frameBuffer = new Bitmap[numberOfFrames];
            Console.WriteLine($"Maximum x: {maxX}; Maximum y: {maxY}; Minimum x: {minX}; Minimum y: {minY}");
            Console.WriteLine("Начинаем рендер");
            try
            {
                Parallel.For(0, numberOfFrames, t =>
                    {
                        var convertedBitmap = new FastBitmap(new Bitmap(width, height));
                        convertedBitmap.LockBits();
                        FastBitmap currentBitmap;
                        lock (reader)
                        {
                            currentBitmap = new FastBitmap(reader.ReadVideoFrame(t));                            
                        }
                        currentBitmap.LockBits();
                        for (var x = 0; x < width; x++)
                        {
                            for (var y = 0; y < height; y++)
                            {
                                var pixel = RunFunction(x, y, GetMultiplier(t, numberOfFrames, multiplierFrom, multiplierTo),
                                    function, width, height);
                                var convX = FastRoundInt(Map(pixel.Item1, minX, maxX, 0, width-1));
                                var convY = FastRoundInt(Map(pixel.Item2, minY, maxY, 0, height-1));
                                convertedBitmap.SetPixel(convX, convY, currentBitmap.GetPixel(x, y));
                            }
                        }
                        currentBitmap.UnlockBits();
                        currentBitmap.DisposeSource();
                        convertedBitmap.UnlockBits();
                        var res = CompleteBitmap(convertedBitmap.GetSource());
                        Progress.ReportFastTime(progress++, numberOfFrames);
                    }
                );
            }
            catch (Exception e) {}
            writer.Flush();
        }*/
    }
}