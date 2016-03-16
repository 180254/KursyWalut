using System;

namespace KursyWalut.Provider
{
    internal class Progress
    {
        public static readonly Progress Master = new Progress(0, 1000);

        public readonly int Start;
        public readonly int End;

        public Progress(int start, int end)
        {
            if (start < end) throw new ArgumentException(nameof(start) + " < " + nameof(end));

            Start = start;
            End = end;
        }

        public Progress Partial(double percentFrom, double percentTo)
        {
            if (!(percentFrom >= 0 && percentFrom <= 100)) throw new ArgumentOutOfRangeException(nameof(percentFrom));
            if (!(percentTo >= 0 && percentTo <= 100)) throw new ArgumentOutOfRangeException(nameof(percentTo));
            if (percentFrom < percentTo) throw new ArgumentException(nameof(percentFrom) + " < " + nameof(percentTo));

            return new Progress(ComputePercent(percentFrom), ComputePercent(percentTo));
        }

        public Progress Partial2(int partIndex, int partCount)
        {
            return Partial(partIndex*(1.0/partCount), (partIndex + 1)*(1.0/partCount));
        }

        private int ComputePercent(double percent)
        {
            return (int) (Start + (End - Start)*percent);
        }
    }
}