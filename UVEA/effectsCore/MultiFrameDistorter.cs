using System;
using System.ComponentModel;
using System.Drawing;
using Accord.Video.FFMPEG;

namespace UVEA
{
    public class MultiFrameDistorter
    {
        public Bitmap RunOneFrame(VideoFileReader reader, int frameNum, double multiplierFrom,
            double multiplierTo, Functions function, int maxWidth, int maxHeight)
        {
            return new Bitmap(0,0);
        }

        public static void Run(VideoFileReader reader, VideoFileWriter writer, MultiFrameFunctions function, BackgroundWorker renderWorker)
        {
            switch (function)
            {
                case MultiFrameFunctions.PhotoFinish:
                    PhotoFinish(reader, writer, renderWorker);
                    break;
                case MultiFrameFunctions.Swirl:
                    break;
            }
        }

        private static void PhotoFinish(VideoFileReader reader, VideoFileWriter writer, BackgroundWorker renderWorker)
        {
            var numberOfFrames = (int)reader.FrameCount;
            var width = reader.Width;
            var height = reader.Height;
            if (numberOfFrames > width)        //if user want -> fit to original width
                numberOfFrames = width;
            //writer.Width = numberOfFrames; //rewrite for change resolution, open file in method
            for (var x = 0; x < width; x++)
            {
                var convertedBitmap = new FastBitmap(new Bitmap(numberOfFrames, height)); //photofinish одновременная обработка нескольких кадров
                convertedBitmap.LockBits();
                for (var f = 0; f < numberOfFrames; f++)
                {
                    FastBitmap currentBitmap;
                    try
                    {
                        currentBitmap = new FastBitmap(reader.ReadVideoFrame(f));
                    }
                    catch (Exception ignored)
                    {
                        break;
                    }
                    currentBitmap.LockBits();
                    for (var y = 0; y < height; y++)
                    {
                        convertedBitmap.SetPixel(f, y, currentBitmap.GetPixel(x, y));
                    }
                    currentBitmap.DisposeSource();
                }
                convertedBitmap.UnlockBits();
                writer.WriteVideoFrame(convertedBitmap.GetSource());
                
                AppForm.PreviewBitmap = (Bitmap)convertedBitmap.GetSource().Clone();
                convertedBitmap.DisposeSource();
                renderWorker.ReportProgress(FastUtils.FastRoundInt(x*1000.0/width));
            }
        }
    }
}