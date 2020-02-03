using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Accord.Video.FFMPEG;

namespace UVEA
{
    public enum EffectClass
    {
        SingleFrame,
        MultiFrame
    }

    public enum MultiFrameFunctions
    {
        PhotoFinish,
        Swirl
    }
    class Program
    {
        static readonly int OutputFps = 30;
        static readonly int FreeMemory = 700;  //В мегабайтах
        const int PixelsInMb = 61440;

        public static string VideoPath = @"E:\test\";
        public static string VideoName = "test.mp4";
        public static void CalculateSomeInformation()
        {
            Bitmap probeBitmap = new Bitmap(VideoPath + @"InputSequence\1.png");
            var time = probeBitmap.Width / OutputFps;
            Console.WriteLine("Output Fps: " + OutputFps);
            Console.WriteLine("Output time: " + time + " seconds.");
        }

        public static void Generate()
        {
            var reader = new VideoFileReader();
            reader.Open(VideoPath+VideoName);
            var writer = new VideoFileWriter();
            var numberOfFrames = (int)reader.FrameCount;
            Bitmap probeBitmap = reader.ReadVideoFrame(0);
            if (numberOfFrames > probeBitmap.Width)        //if user want -> fit to original width
                numberOfFrames = probeBitmap.Width;
            writer.Open(VideoPath+"out.mp4", numberOfFrames, probeBitmap.Height, OutputFps, VideoCodec.H265);
            for (var x = 0; x < probeBitmap.Width; x++)
            {
                var convertedBitmap = new Bitmap(numberOfFrames, probeBitmap.Height);
                reader.Open(VideoPath+VideoName);
                for (var f = 0; f < numberOfFrames; f++)
                {
                    var currentBitmap = reader.ReadVideoFrame();
                    for (var y = 0; y < probeBitmap.Height; y++)
                    {
                        convertedBitmap.SetPixel(f, y, currentBitmap.GetPixel(x, y));
                    }
                    currentBitmap.Dispose();
                }
                writer.WriteVideoFrame(convertedBitmap);
                convertedBitmap.Dispose();
            }
            reader.Dispose();
            writer.Close();
        }
        /*
        public static void GetVideo()
        {
            proc.StartInfo.FileName = FfmpegPath;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.Arguments = ParamsOutput;
            proc.Start();
            proc.WaitForExit();
        }*/
        //Unusual video effects augmentor 
        //Uvea
        [STAThread]
        static void Main()
        {
            
            /*var reader = new VideoFileReader();
            reader.Open(VideoPath+VideoName);
            var writer = new VideoFileWriter();
            writer.BitRate = reader.BitRate;
            writer.VideoCodec = VideoCodec.Mpeg4;
            writer.FrameRate = 30;
            writer.Height = reader.Height;
            writer.Width = reader.Width;
            writer.Open(VideoPath+"out.avi");
            //OneFrameDistorter.RunParallel(reader, writer, -0.01, 0.01, OneFrameDistorter.Functions.Sine);#1#
            OneFrameDistorter.Run(reader, writer, -0.01, 0.01, Functions.Sine, new BackgroundWorker());*/
            Application.EnableVisualStyles();
            Application.Run(new AppForm());
            //CalculateSomeInformation();
            //Generate();
            //PolarCoordsConvertor.Run(VideoPath, VideoName);
            //FunctionsDistorter.Run(VideoPath, 0, FunctionsDistorter.functions.FishEye);
        }
    }
}
