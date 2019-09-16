using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UVEC
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

        public static void CompareBitmaps(Bitmap bitmap1, Bitmap bitmap2, string logPath, bool images, int threshold)
        {
            var counter = 0;
            FileStream aFile = new FileStream($"{logPath}logsThreshold{threshold}.txt", FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(aFile);
            aFile.Seek(0, SeekOrigin.End);
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
                        sw.Write($"x: {x}, y: {y}, Current delta: {deltapix}, Max delta: {maxDelta}, Counter: {counter}\r\n");

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
            sw.Close();
            Console.WriteLine("Done");
        }
    }
}
