using GalaSoft.MvvmLight;

namespace KursyWalut.ViewModel
{
    public class HistoryViewModel : ViewModelBase
    {
        private int _progress;

        public int Progress
        {
            get { return _progress; }
            set { Set(() => Progress, ref _progress, value); }
        }
    }
}