using System;

namespace KursyWalut.Provider
{
    internal class Progress
    {
        public static readonly Progress Master = new Progress(0, 1000);

        private readonly Progress _parent;
        public readonly int Start;
        public readonly int End;

        private Progress(Progress parent, int start, int end)
        {
            if (start < end) throw new ArgumentException(nameof(start) + " < " + nameof(end));

            _parent = parent;
            Start = start;
            End = end;
        }

        public Progress(int start, int end) : this(null, start, end)
        {
        }

        public bool Starting => _parent == null || Start == _parent.Start;
        public bool Ending => _parent == null || End == _parent.End;

        public Progress PartialPercent(double percentFrom, double percentTo)
        {
            if (!(percentFrom >= 0 && percentFrom <= 100)) throw new ArgumentOutOfRangeException(nameof(percentFrom));
            if (!(percentTo >= 0 && percentTo <= 100)) throw new ArgumentOutOfRangeException(nameof(percentTo));
            if (percentFrom < percentTo) throw new ArgumentException(nameof(percentFrom) + " < " + nameof(percentTo));

            return new Progress(this, ComputePercent(percentFrom), ComputePercent(percentTo));
        }

        public Progress PartialPart(int partIndex, int partCount)
        {
            if (partIndex >= partCount) throw new ArgumentException(nameof(partIndex) + " >= " + nameof(partCount));

            var percentPerPart = 1.0/partCount;
            return PartialPercent(partIndex*percentPerPart, (partIndex + 1)*percentPerPart);
        }

        private int ComputePercent(double percent)
        {
            return (int) (Start + (End - Start)*percent);
        }
    }
}