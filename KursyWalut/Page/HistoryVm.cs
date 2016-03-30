using GalaSoft.MvvmLight;
using KursyWalut.Model;

namespace KursyWalut.Page
{
    public class HistoryVm : ViewModelBase
    {
        private Currency _currency;
        private int _progress;

        public Currency Currency
        {
            get { return _currency; }
            set { Set(() => Currency, ref _currency, value); }
        }

        public int Progress
        {
            get { return _progress; }
            set { Set(() => Progress, ref _progress, value); }
        }
    }
}