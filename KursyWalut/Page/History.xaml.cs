using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Ioc;
using KursyWalut.Helper;
using KursyWalut.Model;

namespace KursyWalut.Page
{
    public sealed partial class HistoryPage : Windows.UI.Xaml.Controls.Page
    {
        private readonly HistoryVm _historyPageVm;
        private readonly EventHandler<int> _toProgressNotifier;

        public HistoryPage()
        {
            InitializeComponent();
            _historyPageVm = SimpleIoc.Default.GetInstance<HistoryVm>();
            _toProgressNotifier = (sender, i) => _historyPageVm.Progress = i;
      
            Init();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _historyPageVm.Currency = (Currency)e.Parameter;
        }

        private async void Init()
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

//            using (var h = new ProviderHelper(_toProgressNotifier))
//            {
//            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine("{0} time: {1}", nameof(Init), sw.Elapsed.ToString("mm':'ss':'fff"));
#endif
        }
    }
}