using System;
using System.Diagnostics;
using GalaSoft.MvvmLight.Ioc;
using KursyWalut.Helper;
using KursyWalut.ViewModel;

namespace KursyWalut.Page
{
    public sealed partial class HistoryPage : Windows.UI.Xaml.Controls.Page
    {
        private readonly HistoryViewModel _historyViewModel;
        private readonly EventHandler<int> _toProgressNotifier;

        public HistoryPage()
        {
            InitializeComponent();
            _historyViewModel = SimpleIoc.Default.GetInstance<HistoryViewModel>();
            _toProgressNotifier = (sender, i) => _historyViewModel.Progress = i;

            Init();
        }

        private async void Init()
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            using (var h = new ProviderHelper(_toProgressNotifier))
            {
            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine("{0} time: {1}", nameof(Init), sw.Elapsed.ToString("mm':'ss':'fff"));
#endif
        }
    }
}