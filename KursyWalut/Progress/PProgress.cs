﻿using System;

namespace KursyWalut.Progress
{
    public class PProgress : IPProgress
    {
        private readonly Integer _lastReported;
        private readonly Integer _lastNotified;
        private readonly PProgress _parent;

        public PProgress(int maxValue) : this(maxValue, null)
        {
        }

        private PProgress(int maxValue, PProgress parent)
        {
            MaxValue = maxValue;
            _parent = parent;
            _lastReported = 0;
            _lastNotified = 0;
        }

        public int MaxValue { get; }
        public int CurrentValue => _lastReported.Get();
        public event EventHandler<int> ProgressChanged;

        public void ReportProgress(double percent)
        {
            if (percent < 0.00) NotifyChange(-1);

            var computePercent = ComputePercent(percent);
            var incrValue = computePercent - _lastReported.Get();
            if (incrValue <= 0) incrValue = 0;

            IncrementProgress(incrValue);
        }

        public IPProgress SubPercent(double percentFrom, double percentTo)
        {
            return new PProgress(ComputePercent(percentTo - percentFrom), this);
        }

        public IPProgress SubPart(int partIndex, int partCount)
        {
            return SubPercent(0, 1.0/partCount);
        }

        private void IncrementProgress(int incrValue)
        {
            var currentValue = _lastReported.Increment(incrValue);
            if (_parent == null) NotifyChange(currentValue);

            _parent?.IncrementProgress(incrValue);
        }

        private void NotifyChange(int value)
        {
            var change = value - _lastNotified.Get();
            var percentageChange = change/(double) MaxValue*100;
            var isMaxNotNotified = (value == MaxValue) && (_lastNotified.Get() != value);

            // ReSharper disable once InvertIf
            if ((percentageChange >= 1.00) || isMaxNotNotified)
            {
                _lastNotified.Set(value);
                ProgressChanged?.Invoke(this, value);
            }
        }

        private int ComputePercent(double percent)
        {
            return (int) (MaxValue*percent);
        }
    }
}