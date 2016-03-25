using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight.Ioc;
using KursyWalut.ViewModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KursyWalut
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MainViewModel _mainViewModel;

        public MainPage()
        {
            InitializeComponent();
            _mainViewModel = SimpleIoc.Default.GetInstance<MainViewModel>();
        }

        private void CalendarDatePicker_OnCalendarViewDayItemChanging(
            CalendarView sender,
            CalendarViewDayItemChangingEventArgs e)
        {
            if (e.Item != null && !_mainViewModel.AvailDates.Contains(e.Item.Date.DateTime.Date))
            {
                e.Item.IsBlackout = true;
            }
        }

        private void CalendarDatePicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate != null) { 
                _mainViewModel.Reload(args.NewDate.Value.Date);
            }
        }
    }
}