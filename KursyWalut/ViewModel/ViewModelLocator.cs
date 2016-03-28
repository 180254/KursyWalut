using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace KursyWalut.ViewModel
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<HistoryViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public HistoryViewModel History => ServiceLocator.Current.GetInstance<HistoryViewModel>();
    }
}