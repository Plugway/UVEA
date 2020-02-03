using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UVEA
{
    class Progress
    {
        private long _averageTicks;
        private long _previousTime;
        private long _startTime;
        private int _numberOfFrames;

        public Progress(int numberOfFrames)
        {
            _numberOfFrames = numberOfFrames;
        }

        public void ReportFastTime(int currentFrame)
        {
            var timeLeft = new TimeSpan();
            var now = DateTime.Now.Ticks;
            if (_previousTime != 0)
            {
                if (_averageTicks == 0)
                {
                    _startTime = now;
                    _averageTicks = now - _previousTime;
                    timeLeft = new TimeSpan(_averageTicks * (_numberOfFrames - currentFrame - 1));
                }
                else
                {
                    var delta = FastUtils.FastAbs((int)(_averageTicks - (now - _previousTime)));
                    if (delta > 250000 && delta < 1500000)
                        _averageTicks = (_averageTicks + (now - _previousTime)) / 2;
                    timeLeft = new TimeSpan(_averageTicks * (_numberOfFrames - currentFrame - 1));
                }
            }
            Console.Write($"\rProgress: {Math.Round((double)currentFrame / (_numberOfFrames - 1) * 100, 2)}%, Time left: {timeLeft}, Frame: {currentFrame + 1}/{_numberOfFrames}                        ");
            if (currentFrame + 1 == _numberOfFrames)
            {
                Console.WriteLine($"\nDone. Total time elapsed: {new TimeSpan(now - _startTime)}");
                ResetAverageTime();
            }
            _previousTime = now;
        }

        public void ResetAverageTime()
        {
            _startTime = 0;
            _averageTicks = 0;
            _previousTime = 0;
        }
        /// <summary>
        /// Calculates remaining and elapsed time.
        /// </summary>
        /// <param name="currentFrame">Frame number.</param>
        public Tuple<TimeSpan, TimeSpan> GetTimeElapsed(int currentFrame)
        {
            var timeLeft = new TimeSpan();
            var now = DateTime.Now.Ticks;
            if (_previousTime != 0)
            {
                if (_averageTicks == 0)
                {
                    _startTime = now;
                    _averageTicks = now - _previousTime;
                    timeLeft = new TimeSpan(_averageTicks * (_numberOfFrames - currentFrame - 1));
                }
                else
                {
                    var delta = FastUtils.FastAbs((int)(_averageTicks - (now - _previousTime)));
                    if (delta > 250000 && delta < 1500000)
                        _averageTicks = (_averageTicks + (now - _previousTime)) / 2;
                    timeLeft = new TimeSpan(_averageTicks * (_numberOfFrames - currentFrame - 1));
                }
            }
            _previousTime = now;
            return new Tuple<TimeSpan, TimeSpan>(timeLeft, new TimeSpan(now - _startTime));
        }
    }
}
