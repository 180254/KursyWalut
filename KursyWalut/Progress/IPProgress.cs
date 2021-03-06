﻿using System;

namespace KursyWalut.Progress
{
    public interface IPProgress
    {
        int CurrentValue { get; }

        event EventHandler<int> ProgressChanged;

        void ReportProgress(double percent);
        IPProgress SubPart(int partIndex, int partCount);
        IPProgress SubPercent(double percentFrom, double percentTo);
    }
}