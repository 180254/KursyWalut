﻿using System;
using System.Threading.Tasks;
using KursyWalut.Cache;
using KursyWalut.Progress;
using KursyWalut.Provider;
using KursyWalut.ProviderImpl;

namespace KursyWalut.Helper
{
    public class ProviderHelper : IDisposable
    {
        private readonly IPProgress _pprogressM;
        private readonly NbpExchangeRatesProvider _nbpProvider;
        private readonly CacheExchangeRateProvider _cacheProvider;

        public ProviderHelper(ICache cache, EventHandler<int> progressSubscriber)
        {
            _pprogressM = PProgress.NewMaster();
            _pprogressM.ProgressChanged += progressSubscriber;
#if DEBUG
//            _pprogressM.ProgressChanged += (sender, i) => Debug.WriteLine("progress-{0}", i);
#endif
            _pprogressM.ReportProgress(0.00);

            _nbpProvider = new NbpExchangeRatesProvider(cache);
            _cacheProvider = new CacheExchangeRateProvider(_nbpProvider, cache);

            ErService = new StandardExchangeRateService(_cacheProvider);
            Progress = _pprogressM.SubPercent(0.05, 0.95);
        }

        public IPProgress Progress { get; }
        public IExchangeRatesService ErService { get; }

        public void Dispose()
        {
            var cacheable = ErService as ICacheable;
            cacheable?.FlushCache(_pprogressM.SubPercent(0.95, 1.00));
            _pprogressM.ReportProgress(1.00);
        }

        public async Task Init()
        {
            await _nbpProvider.InitCache(_pprogressM.SubPercent(0.00, 0.025));
            await _cacheProvider.InitCache(_pprogressM.SubPercent(0.025, 0.05));
        }
    }
}