using System;

namespace KursyWalut.Progress
{
    public class PProgress : IPProgress
    {
        private readonly Integer _lastReported;
        private readonly PProgress _parent;

        public PProgress(int maxValue) : this(maxValue, null)
        {
        }

        private PProgress(int maxValue, PProgress parent)
        {
            MaxValue = maxValue;
            _parent = parent;
            _lastReported = 0;
        }

        public int MaxValue { get; }
        public int CurrentValue => _lastReported.Get();
        public event EventHandler<int> ProgressChanged;

        public void ReportProgress(double percent)
        {
            if (percent < 0.00) NotifyChange(-1);

            var computePercent = ComputePercent(percent);
            var incrValue = computePercent - _lastReported.Get();
            if (incrValue <= 0) return;

            IncrementProgress(incrValue);
        }

        public IPProgress SubPercent(double percentFrom, double percentTo)
        {
            return new PProgress(ComputePercent(percentTo - percentFrom), this);
        }

        public IPProgress SubPart(int partIndex, int partCount)
        {
            var percentPerPart = 1.0/partCount;
            return SubPercent(partIndex*percentPerPart, (partIndex + 1)*percentPerPart);
        }

        private void IncrementProgress(int incrValue)
        {
            var currentValue = _lastReported.Increment(incrValue);
            NotifyChange(currentValue);

            _parent?.IncrementProgress(incrValue);
        }

        private void NotifyChange(int value)
        {
            ProgressChanged?.Invoke(this, value);
        }

        private int ComputePercent(double percent)
        {
            return (int) Math.Round(MaxValue*percent);
        }

        public static IPProgress NewMaster()
        {
            return new PProgress(10000);
        }
    }
}