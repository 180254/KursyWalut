using System;
using System.Threading.Tasks;
using KursyWalut.Cache;
using KursyWalut.Progress;
using KursyWalut.Provider;
using KursyWalut.ProviderImpl;

namespace KursyWalut.Helper
{
    public class ProviderHelper
    {
        private readonly IErService _erService;

        private readonly int _progressMax;
        private readonly EventHandler<int> _progressSubscriber;

        private bool _cacheAlreadyInit;

        public ProviderHelper(ICache cache, int progressMax, EventHandler<int> progressSubscriber)
        {
            _progressMax = progressMax;
            _progressSubscriber = progressSubscriber;

            var nbpProvider = new NbpErProvider(cache);
            var cacheProvider = new CacheErProvider(nbpProvider, cache);
            _erService = new StandardErService(cacheProvider);
        }

        public ProviderHelper2 Helper()
        {
            return new ProviderHelper2(this);
        }

        // ---------------------------------------------------------------------------------------------------------------

        public class ProviderHelper2 : IDisposable
        {
            private readonly ProviderHelper _providerHelper;
            private readonly IPProgress _pprogress;

            public ProviderHelper2(ProviderHelper providerHelper)
            {
                _providerHelper = providerHelper;
                ErService = _providerHelper._erService;

                _pprogress = new PProgress(providerHelper._progressMax);
                _pprogress.ProgressChanged += providerHelper._progressSubscriber;
//                _pprogress.ProgressChanged += (sender, i) => Debug.WriteLine("progress-{0}", i);

                _pprogress.ReportProgress(0.00);
                Progress = _pprogress.SubPercent(0.05, 0.95);
            }

            public IPProgress Progress { get; }
            public IErService ErService { get; }

            public void Dispose()
            {
            }

            public async Task InitCache()
            {
                if (!_providerHelper._cacheAlreadyInit)
                {
                    var cacheable = ErService as ICacheable;
                    var initCache = cacheable?.InitCache(_pprogress.SubPercent(0.00, 0.05));
                    if (initCache != null)
                        await initCache;

                    _providerHelper._cacheAlreadyInit = true;
                }

                _pprogress.ReportProgress(0.05);
            }

            public async Task FlushCache()
            {
                var cacheable = ErService as ICacheable;
                var flushCache = cacheable?.FlushCache(_pprogress.SubPercent(0.95, 1.00));
                if (flushCache != null)
                    await flushCache;

                _pprogress.ReportProgress(1.00);
            }
        }
    }
}