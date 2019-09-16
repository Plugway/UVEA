using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UVEC
{
    class Progress
    {
        private static long averageTicks = 0;
        private static long previousTime = 0;
        private static long startTime = 0;
        public static void Report(int numberOfFrames, string path)
        {
            var currentFrame = new DirectoryInfo($"{path}OutputSequence").GetFiles().Length;
            Console.Write($"\rProgress: {Math.Round((double)currentFrame / (numberOfFrames) * 100, 2)}%, Frame:{currentFrame}/{numberOfFrames}    ");
            if (currentFrame == numberOfFrames)
                Console.WriteLine("\nDone");
        }

        public static void ReportFast(int currentFrame, int numberOfFrames)
        {
            Console.Write($"\rProgress: {Math.Round((double)currentFrame / (numberOfFrames - 1) * 100, 2)}%, Frame:{currentFrame + 1}/{numberOfFrames}    ");
            if (currentFrame + 1 == numberOfFrames)
                Console.WriteLine("\nDone");
        }

        public static void ReportFastTime(int currentFrame, int numberOfFrames)
        {
            var timeLeft = new TimeSpan();
            var now = DateTime.Now.Ticks;
            if (previousTime != 0)
            {
                if (averageTicks == 0)
                {
                    startTime = now;
                    averageTicks = now - previousTime;
                    timeLeft = new TimeSpan(averageTicks * (numberOfFrames - currentFrame - 1));
                }
                else
                {
                    var delta = FastUtils.FastAbs((int)(averageTicks - (now - previousTime)));
                    if (delta > 250000 && delta < 1500000)
                        averageTicks = (averageTicks + (now - previousTime)) / 2;
                    timeLeft = new TimeSpan(averageTicks * (numberOfFrames - currentFrame - 1));
                }
            }
            Console.Write($"\rProgress: {Math.Round((double)currentFrame / (numberOfFrames - 1) * 100, 2)}%, Time left: {timeLeft}, Frame: {currentFrame + 1}/{numberOfFrames}         ");
            if (currentFrame + 1 == numberOfFrames)
            {
                Console.WriteLine($"\nDone. Total time elapsed: {new TimeSpan(now - startTime)}");
                startTime = 0;
                averageTicks = 0;
                previousTime = 0;
            }
            previousTime = now;
        }

        public static void ResetAverageTime()
        {
            startTime = 0;
            averageTicks = 0;
            previousTime = 0;
        }
    }
}
