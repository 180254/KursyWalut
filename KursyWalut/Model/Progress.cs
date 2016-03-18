using System;

namespace KursyWalut.Model
{
    internal class Progress
    {
        public static readonly Progress Master = new Progress(0, 1000);

        public readonly int Start;
        public readonly int End;

        public Progress(int start, int end)
        {
            if (start > end) throw new ArgumentException(nameof(start) + " < " + nameof(end));

            Start = start;
            End = end;
        }

        public Progress PartialPercent(double percentFrom, double percentTo)
        {
            if (!(percentFrom >= 0 && percentFrom <= 100)) throw new ArgumentOutOfRangeException(nameof(percentFrom));
            if (!(percentTo >= 0 && percentTo <= 100)) throw new ArgumentOutOfRangeException(nameof(percentTo));
            if (percentFrom > percentTo) throw new ArgumentException(nameof(percentFrom) + " < " + nameof(percentTo));

            return new Progress(ComputePercent(percentFrom), ComputePercent(percentTo));
        }

        public Progress PartialPart(int partIndex, int partCount)
        {
            if (partIndex >= partCount) throw new ArgumentException(nameof(partIndex) + " >= " + nameof(partCount));

            var percentPerPart = 1.0/partCount;
            return PartialPercent(partIndex*percentPerPart, (partIndex + 1)*percentPerPart);
        }

        public int ComputePercent(double percent)
        {
            return (int) Math.Round(Start + (End - Start)*percent);
        }
    }
}