using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UVEA
{
    public class FastUtils
    {
        public static int FastRoundInt(double num)
        {
            return (int)Math.Floor(num + 0.5);
        }
        
        public static int FastAbs(int num)
        {
            return (num + (num >> 31)) ^ (num >> 31);
        }

        public static double FastSqr(double num)
        {
            return num * num;
        }

        public static double Map(double num, double fromMin, double fromMax, double toMin, double toMax)
        {
            return (num - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
        }

        public static void CompareBitmaps(Bitmap bitmap1, Bitmap bitmap2, bool images, int threshold, string logPath = null)
        {
            var counter = 0;
            StreamWriter sw = null;
            if (logPath != null)
            {
                FileStream aFile = new FileStream($"{logPath}logsThreshold{threshold}.txt", FileMode.OpenOrCreate);
                sw = new StreamWriter(aFile);
                aFile.Seek(0, SeekOrigin.End);                
            }
            var maxDelta = double.MinValue;
            for (var x = 0; x < bitmap1.Width; x++)
            {
                for (var y = 0; y < bitmap1.Height; y++)
                {
                    var pix1 = bitmap1.GetPixel(x, y);
                    var pix2 = bitmap2.GetPixel(x, y);
                    var deltapix = Math.Sqrt(FastSqr(pix1.R - pix2.R) + FastSqr(pix1.G - pix2.G) + FastSqr(pix1.B - pix2.B));
                    if (maxDelta < deltapix)
                        maxDelta = deltapix;
                    if (deltapix >= threshold)
                    {
                        Console.WriteLine($"x: {x}, y: {y}, Current delta: {deltapix}, Max delta: {maxDelta}, Counter: {++counter}\n");
                        if (logPath != null)
                        {
                            sw.Write($"x: {x}, y: {y}, Current delta: {deltapix}, Max delta: {maxDelta}, Counter: {counter}\r\n");
                        }
                        if (images)
                        {
                            var convBitmap = new Bitmap(100, 50);
                            for (var x2 = 0; x2 < 100; x2++)
                            {
                                for (var y2 = 0; y2 < 50; y2++)
                                {
                                    if (x2 < 50)
                                    {
                                        convBitmap.SetPixel(x2, y2, pix1);
                                    }
                                    else
                                        convBitmap.SetPixel(x2, y2, pix2);
                                }
                            }
                            convBitmap.Save($"{logPath}threshold{threshold}out{counter}.png");
                            convBitmap.Dispose();
                        }
                    }
                }
            }
            if (logPath != null)
            {
                sw.Close();                
            }
            Console.WriteLine("Done");
        }
        public static Bitmap ScaleImage(Image image, int maxWidth, int maxHeight, bool allowEnlarge, bool fillWithBlack) //https://stackoverflow.com/questions/28632480/center-image-on-another-image-c-sharp
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);
            
            var newImage = new Bitmap(maxWidth, maxHeight);
            if (!fillWithBlack)
            {
                newImage.Dispose();
                newImage = new Bitmap(newWidth, newHeight);
            }
            using (var graphics = Graphics.FromImage(newImage))
            {
                // Calculate x and y which center the image
                int y = newImage.Height/2 - newHeight / 2;
                int x = newImage.Width/2 - newWidth / 2;

                graphics.FillRectangle(Brushes.Black, 0, 0, maxWidth, maxHeight);
                // Draw image on x and y with newWidth and newHeight
                if (maxHeight < image.Height || maxWidth < image.Width || allowEnlarge)
                {
                    graphics.DrawImage(image, x, y, newWidth, newHeight);
                }
                else
                {
                    graphics.DrawImage(image, maxWidth/2 - image.Width/2, maxHeight/2 - image.Height/2);
                }
            }
            return newImage;
        }
    }
}
