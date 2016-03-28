using System.Threading;

namespace KursyWalut.Progress
{
    public class Integer
    {
        private int _value;

        public Integer(int value)
        {
            _value = value;
        }

        public int Value => _value;

        public int Get()
        {
            return _value;
        }

        public int Set(int value)
        {
            return Interlocked.Exchange(ref _value, value);
        }

        public int Increment(int incr)
        {
            return Interlocked.Add(ref _value, incr);
        }

        public int Decrement(int decr)
        {
            return Interlocked.Add(ref _value, -decr);
        }

        public static implicit operator Integer(int value)
        {
            return new Integer(value);
        }
    }
}