using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UVEA
{
    class Tests
    {
        public static void ProgressTest()
        {
            var sw = new Stopwatch();
            var res = 0.0;
            sw.Start();
            for (var x = 0; x < 100; x++)
            {
                for (var y = 0; y < 10000; y++)
                {
                    for (var j = 0; j < 500; j++)
                        res = Math.Pow(x * y * j, 0.3);
                }
                //Progress.ReportFastTime(x, 100);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }
    }
}
