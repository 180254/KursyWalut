using System;
using GalaSoft.MvvmLight.Ioc;
using KursyWalut.Cache;
using KursyWalut.Progress;
using KursyWalut.Provider;
using KursyWalut.ProviderImpl;

namespace KursyWalut.Helper
{
    public class ProviderHelper : IDisposable
    {
        private readonly IPProgress _pprogressM;

        static ProviderHelper()
        {
            SimpleIoc.Default.Register(CacheHelper.GetStandard);
        }

        public ProviderHelper(EventHandler<int> progressSubscriber)
        {
            _pprogressM = PProgress.NewMaster();
            _pprogressM.ProgressChanged += progressSubscriber;
            _pprogressM.ReportProgress(0.00);

            var cache = SimpleIoc.Default.GetInstance<ICache>();
            var nbpProvider = new NbpExchangeRatesProvider(cache, _pprogressM.SubPercent(0.00, 0.025));
            var cacheProvider = new CacheExchangeRateProvider(nbpProvider, cache, _pprogressM.SubPercent(0.025, 0.05));

            ErService = new StandardExchangeRateService(cacheProvider);
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
    }
}