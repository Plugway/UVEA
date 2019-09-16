using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UVEC
{
    class Progress
    {
        public static void Report(int currentFrame, int numberOfFrames)
        {
            Console.Write($"\rProgress: {Math.Round((double)currentFrame / (numberOfFrames - 1) * 100, 2)}%, Frame:{currentFrame+1}/{numberOfFrames}    ");
            if (currentFrame + 1 == numberOfFrames)
                Console.WriteLine("\nDone");
        }
    }
}
