using System;
using System.Collections.Generic;
using System.Linq;

namespace KursyWalut.Progress
{
    public class PProgress : IPProgress
    {
        private readonly PProgress _parent;
        private readonly IList<PProgress> _childs;

        private readonly Integer _currentValue;
        private readonly Integer _lastReported;

        public PProgress(int maxValue) : this(maxValue, null, 0)
        {
        }

        private PProgress(int maxValue, PProgress parent, Integer currentValue)
        {
            MaxValue = maxValue;

            _parent = parent;
            _childs = new List<PProgress>();

            _currentValue = currentValue;
            _lastReported = 0;
        }

        public int MaxValue { get; }
        public int CurrentValue => _currentValue.Get();
        public event EventHandler<int> ProgressChanged;

        public void ReportProgress(double percent)
        {
            if (percent < 0.00) NotifyChange(-1);

            var computePercent = ComputePercent(percent);
            var calculateLastReported = CalculateLastReported();

            var incrValue = computePercent - calculateLastReported;
            if (incrValue <= 0) return;

            _lastReported.Set(calculateLastReported + incrValue);
            var cur = _currentValue.Increment(incrValue);
            _childs.Clear();

            NotifyChange(cur);
        }

        public IPProgress SubPercent(double percentFrom, double percentTo)
        {
            var child = new PProgress(ComputePercent(percentTo - percentFrom), this, _currentValue);
            _childs.Add(child);
            return child;
        }

        public IPProgress SubPart(int partIndex, int partCount)
        {
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

        private int ComputePercent(double percent)
        {
            return (int) Math.Round(MaxValue*percent);
        }

        public static IPProgress NewMaster()
        {
            return new PProgress(100000);
        }
    }
}