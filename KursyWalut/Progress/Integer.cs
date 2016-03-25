using System.ComponentModel;
using System.Threading;

namespace KursyWalut.Progress
{
    public class Integer : INotifyPropertyChanged
    {
        private int _value;

        public Integer(int value)
        {
            _value = value;
        }

        public int Value => _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public static implicit operator Integer(int value)
        {
            return new Integer(value);
        }

        public int Get()
        {
            return _value;
        }

        public int Set(int value)
        {
            var ret = Interlocked.Exchange(ref _value, value);
            OnPropertyChanged();
            return ret;
        }

        public int Increment(int incr)
        {
            var ret = Interlocked.Add(ref _value, incr); OnPropertyChanged();
            return ret;
        }

        public int Decrement(int decr)
        {
            var ret = Interlocked.Add(ref _value, -decr); OnPropertyChanged();
            return ret;
        }

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}