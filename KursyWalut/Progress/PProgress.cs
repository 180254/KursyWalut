using System;
using System.Collections.Generic;
using System.Linq;

namespace KursyWalut.Progress
{
    internal class PProgress : IPProgress
    {
        public readonly int From;
        public readonly int To;

        private readonly PProgress _parent;
        private readonly IList<PProgress> _childs;

        private readonly Integer _currentValue;
        private readonly Integer _lastReported;


        public PProgress(int from, int to) : this(from, to, null, 0)
        {
        }

        private PProgress(int from, int to, PProgress parent, Integer currentValue)
        {
            From = from;
            To = to;

            _parent = parent;
            _childs = new List<PProgress>();

            _currentValue = currentValue;
            _lastReported = 0;
        }

        public int CurrentValue => _currentValue.Get();
        public event EventHandler<int> ProgressChanged;

        public void ReportProgress(double percent)
        {
            var computePercent = ComputePercent(percent);
            var calculateLastReported = CalculateLastReported();

            var incrValue = computePercent - calculateLastReported;

            _lastReported.Set(calculateLastReported + incrValue);
            var cur = _currentValue.Increment(incrValue);

            NotifyChange(cur);
        }

        public IPProgress SubPercent(double percentFrom, double percentTo)
        {
            if (!(percentFrom >= 0 && percentFrom <= 100)) throw new ArgumentOutOfRangeException(nameof(percentFrom));
            if (!(percentTo >= 0 && percentTo <= 100)) throw new ArgumentOutOfRangeException(nameof(percentTo));
            if (percentFrom > percentTo) throw new ArgumentException(nameof(percentFrom) + " < " + nameof(percentTo));

            var child = new PProgress(ComputePercent(percentFrom), ComputePercent(percentTo), this, _currentValue);
            _childs.Add(child);
            return child;
        }

        public IPProgress SubPart(int partIndex, int partCount)
        {
            if (partIndex >= partCount) throw new ArgumentException(nameof(partIndex) + " >= " + nameof(partCount));

            var percentPerPart = 1.0/partCount;
            return SubPercent(partIndex*percentPerPart, (partIndex + 1)*percentPerPart);
        }

        private int CalculateLastReported()
        {
            var childsReported = _childs.Sum(c => c.CalculateLastReported());
            return Math.Max(childsReported, _lastReported.Get());
        }

        private void NotifyChange(int value)
        {
            ProgressChanged?.Invoke(this, value);
            _parent?.NotifyChange(value);
        }

        public static IPProgress Master()
        {
            return new PProgress(0, 10000);
        }

        private int ComputePercent(double percent)
        {
            return (int) Math.Round((To - From)*percent);
        }
    }
}