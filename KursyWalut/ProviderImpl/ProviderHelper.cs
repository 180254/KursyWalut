using System;
using KursyWalut.Cache;
using KursyWalut.Progress;
using KursyWalut.Provider;

namespace KursyWalut.ProviderImpl
{
    internal class ProviderHelper : IDisposable
    {
        private readonly IPProgress _pprogress;

        public ProviderHelper(EventHandler<int> progressSubscriber)
        {
            _pprogress = PProgress.NewMaster();
            _pprogress.ProgressChanged += progressSubscriber;

            ICache cache = LocalStorageCache.GetStandard();

            var nbpProvider = new NbpExchangeRatesProvider(cache, _pprogress.SubPercent(0.00, 0.025));
            var cacheProvider = new CacheExchangeRateProvider(nbpProvider, cache, _pprogress.SubPercent(0.025, 0.05));
            ErService = new StandardExchangeRateService(cacheProvider);

            Progress = _pprogress.SubPercent(0.05, 0.95);
        }

        public IPProgress Progress { get; }
        public IExchangeRatesService ErService { get; }

        public void Dispose()
        {
            var cacheable = ErService as ICacheable;
            cacheable?.FlushCache(_pprogress.SubPercent(0.95, 1.00));
            _pprogress.ReportProgress(1.00);
        }
    }
}