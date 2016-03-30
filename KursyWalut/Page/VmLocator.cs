using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace KursyWalut.Page
{
    public class VmLocator
    {
        static VmLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainVm>();
            SimpleIoc.Default.Register<HistoryVm>();
        }

        public MainVm Main => ServiceLocator.Current.GetInstance<MainVm>();
        public HistoryVm History => ServiceLocator.Current.GetInstance<HistoryVm>();
    }
}