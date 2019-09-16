using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace UVEC
{
    class Program
    {
        static readonly double OutputFps = 30.0;
        static readonly int FreeMemory = 700;  //В мегабайтах
        const int PixelsInMb = 61440;

        static string VideoPath = @"E:\test\";
        static string VideoName = "test.mp4";
        static string FfmpegPath = @"C:\test\ffmpeg\bin\ffmpeg.exe";
        static string ParamsInput = @"-i " + VideoPath + VideoName + " " + VideoPath + @"InputSequence\%d.png";
        static string ParamsOutput = @"-r " + OutputFps + @" -i " + VideoPath + @"OutputSequence\%d.png -c:v libx264 -preset veryslow -crf 1 -pix_fmt yuv420p " + VideoPath + "output.mp4";
        static string ImageMagickPath = @"C:\test\ImageMagick\magick.exe";

        static Process proc = new Process();


        public static void CalculateSomeInformation()
        {
            Bitmap probeBitmap = new Bitmap(VideoPath + @"InputSequence\1.png");
            var time = probeBitmap.Width / OutputFps;
            Console.WriteLine("Output Fps: " + OutputFps);
            Console.WriteLine("Output time: " + time + " seconds.");
        }
        public static void MakeDir()
        {
            Directory.CreateDirectory(VideoPath + "InputSequence");
            Directory.CreateDirectory(VideoPath + "OutputSequence");
        }
        public static void DeleteDir()
        {
            Directory.Delete(VideoPath + "InputSequence", true);
            Directory.Delete(VideoPath + "OutputSequence", true);
        }
        public static void GetVideoFrames()
        {
            proc.StartInfo.FileName = FfmpegPath;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.Arguments = ParamsInput;

            proc.Start();
            proc.WaitForExit();
        }
        public static Tuple<int, int> GetFramesInLastArray(int numberOfFrames, int framesInArray)
        {
            var count = 0;
            while (numberOfFrames > framesInArray)
            {
                numberOfFrames -= framesInArray;
                count += 1;
            }
            return new Tuple<int, int>(numberOfFrames, count);
        }

        public static void Generate1()
        {
            var numberOfFrames = new DirectoryInfo(VideoPath + @"InputSequence").GetFiles().Length;
            Bitmap probeBitmap = new Bitmap(VideoPath + @"InputSequence\1.png");

            Console.WriteLine("Free RAM: " + FreeMemory + " MB");


            if (numberOfFrames > probeBitmap.Width)
                numberOfFrames = probeBitmap.Width;
            for (var t = 0; t < probeBitmap.Width; t++)
            {
                var convertedBitmap = new Bitmap(numberOfFrames, probeBitmap.Height);
                for (var x = 0; x < numberOfFrames; x++)
                {
                    var currentBitmap = new Bitmap(VideoPath + @"InputSequence\" + (x + 1).ToString() + ".png");
                    for (var y = 0; y < probeBitmap.Height; y++)
                    {
                        convertedBitmap.SetPixel(x, y, currentBitmap.GetPixel(t, y));
                    }
                }
                convertedBitmap.Save(VideoPath + @"OutputSequence\" + (t + 1).ToString() + ".png");
            }
        }
        public static void UniteFrames(int frames, int numberOfDivisions, int frameWidth, int lastFrameWidth)
        {
            for (var i = 0; i < frames; i++)
            {
                var paramsStr = "montage ";
                for (var j = 0; j < numberOfDivisions; j++)
                {
                    paramsStr += VideoPath + @"OutputSequence\" + (i + 1) + '_' + j + ".png ";
                }
                paramsStr += "-tile " + numberOfDivisions + "x1 -geometry +0+0 " + VideoPath + @"OutputSequence\" + (i + 1) + ".png";
                proc.StartInfo.FileName = ImageMagickPath;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.Arguments = paramsStr;
                proc.Start();
                proc.WaitForExit();
            }
        }
        public static void Generate2()
        {
            var numberOfFrames = new DirectoryInfo(VideoPath + @"InputSequence").GetFiles().Length;
            Bitmap probeBitmap = new Bitmap(VideoPath + @"InputSequence\1.png");
            var freeMemForArray = FreeMemory * PixelsInMb;
            var framesInArray = freeMemForArray / (probeBitmap.Width * probeBitmap.Height);
            var framesInLastArray = GetFramesInLastArray(numberOfFrames, framesInArray);
            var framesDifference = numberOfFrames - framesInLastArray.Item1;
            var outFramesNumber = probeBitmap.Width * (framesInLastArray.Item2 + 1);

            Console.WriteLine("Free RAM: " + FreeMemory + " MB");
            Console.WriteLine("Free memory for 1 array: " + freeMemForArray + " pixels");
            Console.WriteLine("Frames in 1 array for resolution " + probeBitmap.Width + 'x' + probeBitmap.Height + ": " + framesInArray);
            Console.WriteLine("Files in out folder: " + outFramesNumber);

            var inputArray = new Color[probeBitmap.Width, probeBitmap.Height, framesInArray];

            for (var i = 0; i < framesInLastArray.Item2; i++)
            {
                for (var t = 0; t < framesInArray; t++)
                {
                    Bitmap current = new Bitmap(VideoPath + @"InputSequence\" + (t + 1 + framesInArray * i).ToString() + ".png");
                    for (var x = 0; x < inputArray.GetLength(0); x++)
                    {
                        for (var y = 0; y < inputArray.GetLength(1); y++)
                        {
                            inputArray[x, y, t] = current.GetPixel(x, y);
                        }
                    }
                }
                for (var x = 0; x < inputArray.GetLength(0); x++)
                {
                    Bitmap output = new Bitmap(framesInArray, probeBitmap.Height);
                    for (var t = 0; t < framesInArray; t++)
                    {
                        for (var y = 0; y < inputArray.GetLength(1); y++)
                        {
                            output.SetPixel(t, y, inputArray[x, y, t]);
                        }
                    }
                    output.Save(VideoPath + @"OutputSequence\" + (x + 1).ToString() + '_' + i + ".png");
                }
            }
            for (var t = 0; t < framesInLastArray.Item1; t++)
            {
                Bitmap current = new Bitmap(VideoPath + @"InputSequence\" + (t + 1 + framesDifference).ToString() + ".png");
                for (var x = 0; x < inputArray.GetLength(0); x++)
                {
                    for (var y = 0; y < inputArray.GetLength(1); y++)
                    {
                        inputArray[x, y, t] = current.GetPixel(x, y);
                    }
                }
            }
            for (var x = 0; x < inputArray.GetLength(0); x++)
            {
                Bitmap output = new Bitmap(framesInLastArray.Item1, probeBitmap.Height);
                for (var t = 0; t < framesInLastArray.Item1; t++)
                {
                    for (var y = 0; y < inputArray.GetLength(1); y++)
                    {
                        output.SetPixel(t, y, inputArray[x, y, t]);
                    }
                }
                output.Save(VideoPath + @"OutputSequence\" + (x + 1).ToString() + '_' + (framesInLastArray.Item2) + ".png");
            }
            UniteFrames(probeBitmap.Width, framesInLastArray.Item2 + 1, framesInArray, framesInLastArray.Item1);
        }
        public static void Generate()
        {
            Console.WriteLine('?');
            if (int.Parse(Console.ReadLine()) == 1)
                Generate1();
            else
                Generate2();
        }
        public static void GetVideo()
        {
            proc.StartInfo.FileName = FfmpegPath;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.Arguments = ParamsOutput;
            proc.Start();
            proc.WaitForExit();
        }
        //Ultimate video effects converter
        static void Main()
        {
            //MakeDir();
            //GetVideoFrames();
            //CalculateSomeInformation();
            //Generate();
            //PolarCoordsConvertor.Run(VideoPath);
            FunctionsDistorter.Run(VideoPath, 0, FunctionsDistorter.functions.FishEye);
            //GetVideo();
            //DeleteDir();
        }
    }
}
